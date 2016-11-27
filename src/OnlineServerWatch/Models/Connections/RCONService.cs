using CoreRCON;
using CoreRCON.Parsers.Standard;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OnlineServerWatch.Hubs;
using OnlineServerWatch.Models.Configuration;
using OnlineServerWatch.Models.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace OnlineServerWatch.Models.Connections
{
	public interface IRCONService
	{
		List<GameServer> GameServers { get; }
	}

	public class RCONService : IRCONService
	{
		private IHubContext _context;
		private IPAddress _host;
		private ILogger<RCONService> _log;
		private IConfiguration _config;
		public List<GameServer> GameServers { get; } = new List<GameServer>();

		public RCONService(
			IOptionsMonitor<List<Server>> servers,
			IConnectionManager manager,
			ILogger<RCONService> log,
			IConfiguration config)
		{
			_context = manager.GetHubContext<RCONHub>();
			_log = log;
			_host = IPAddress.Parse(config.GetSection("PublicIP").Value);

			Setup(servers.CurrentValue);
			servers.OnChange(Setup);
		}

		private async Task Connect(GameServer server)
		{
			RCON rcon;

			try
			{
				rcon = new RCON(
					host: IPAddress.Parse(server.ServerConfiguration.IP),
					port: server.ServerConfiguration.Port,
					password: server.ServerConfiguration.Password
				);
			}
			catch
			{
				await Reconnect(server);
				return;
			}

			var log = new LogReceiver(
				self: _host,
				port: server.ServerConfiguration.LogPort // Defaults to 0 if not set, which will use the first-available port anyway
			);

			rcon.OnDisconnected += async () => await Reconnect(server);

			await rcon.SendCommandAsync($"logaddress_add {_host}:{log.ResolvedPort}");

			log.Listen<ChatMessage>(server.ChatReceived);
			log.Listen<KillFeed>(server.KillReceived);

			log.Listen<PlayerConnected>(connected =>
			{
				server.Players.Add(connected.Player);
				server.ServerInfoUpdated();
			});

			log.Listen<PlayerDisconnected>(disconnected =>
			{
				var player = server.Players.FirstOrDefault(p => p.ClientId == disconnected.Player.ClientId);
				if (player == null) return;
				server.Players.Remove(player);
				server.ServerInfoUpdated();
			});

			log.Listen<NameChange>(change =>
			{
				var player = server.Players.FirstOrDefault(p => p.ClientId == change.Player.ClientId);
				if (player == null) return;
				player.Name = change.NewName;
				server.ServerInfoUpdated();
			});

			server.Status = await rcon.SendCommandAsync<Status>("status");
			server.ReconnectionRetries = 0;
			server.Connected = true;
			server.ServerInfoUpdated();
			await Task.Delay(-1);
		}

		private async Task Reconnect(GameServer server)
		{
			if (server.ReconnectionRetries++ >= Constants.MAX_RETRIES)
			{
				_log.LogCritical($"Reached a maximum of {Constants.MAX_RETRIES} failed connection attempts for {server.ServerConfiguration}.  Stopping.");
				return;
			}

			server.Connected = false;
			server.ServerInfoUpdated();
			_log.LogError($"Failed to connect to {server.ServerConfiguration}...  Trying again in 10 seconds.");
			await Task.Delay(10000);
			_log.LogInformation($"Attempting to reconnect to {server.ServerConfiguration}.");
			await Connect(server);
		}

		private void Setup(List<Server> servers)
		{
			GameServers.Clear();

			foreach (var server in servers)
			{
				var gameServer = new GameServer(server);
				GameServers.Add(gameServer);

				gameServer.OnChatReceived += message =>
				{
					_context.Clients.All.Chat(gameServer, message);
				};

				gameServer.OnKillReceived += kill =>
				{
					_context.Clients.All.Kill(gameServer, kill);
				};

				gameServer.OnServerInfoUpdated += () =>
				{
					_context.Clients.All.Update(gameServer);
				};

				Task.Run(() => Connect(gameServer));
			}
		}
	}
}