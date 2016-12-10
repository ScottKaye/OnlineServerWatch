using CoreRCON;
using CoreRCON.Parsers.Standard;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OnlineServerWatch.Hubs;
using OnlineServerWatch.Models.Configuration;
using OnlineServerWatch.Models.Game;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineServerWatch.Models.Connections
{
	public interface ILogAddressService
	{
		List<GameServer> GameServers { get; }
	}

	public class LogAddressService : ILogAddressService
	{
		private IHubContext _context;
		private ILogger<LogAddressService> _log;
		private IConfiguration _config;
		public List<GameServer> GameServers { get; } = new List<GameServer>();

		public LogAddressService(
			IOptionsMonitor<List<Server>> servers,
			IConnectionManager manager,
			ILogger<LogAddressService> log,
			IConfiguration config)
		{
			_context = manager.GetHubContext<RCONHub>();
			_log = log;

			StartListening(servers.CurrentValue);
			servers.OnChange(StartListening);
		}

		private async Task Connect(GameServer server)
		{
			var log = new LogReceiver(
				server.ServerConfiguration.LogPort,
				server.ServerConfiguration.EndPoint
			);

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

			server.Connected = true;
			server.ServerInfoUpdated();
			await Task.Delay(-1);
		}

		private void StartListening(List<Server> servers)
		{
			GameServers.Clear();

			foreach (var server in servers)
			{
				var gameServer = new GameServer(server);
				GameServers.Add(gameServer);

				gameServer.OnChatReceived += message =>
				{
					_context.Clients.All.Chat(gameServer.RuntimeId, message);
				};

				gameServer.OnKillReceived += kill =>
				{
					_context.Clients.All.Kill(gameServer.RuntimeId, kill);
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