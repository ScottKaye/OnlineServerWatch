using CoreRCON;
using CoreRCON.Parsers.Standard;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Infrastructure;
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
		public List<GameServer> GameServers { get; } = new List<GameServer>();
		private IHubContext Context;

		public RCONService(IOptionsMonitor<List<Server>> servers, IConnectionManager manager)
		{
			Context = manager.GetHubContext<RCONHub>();

			Setup(servers.CurrentValue);
			servers.OnChange(Setup);
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
					Context.Clients.All.Chat(gameServer, message);
				};

				gameServer.OnKillReceived += kill =>
				{
					Context.Clients.All.Kill(gameServer, kill);
				};

				gameServer.OnServerInfoUpdated += () =>
				{
					Context.Clients.All.Update(gameServer);
				};

				Task.Run(() => Connect(gameServer));
			}
		}

		private static async Task Connect(GameServer server)
		{
			string thisServer = "192.168.1.8"; // TODO get real IP of the current server

			var rcon = new RCON(
				host: IPAddress.Parse(server.ServerConfiguration.IP),
				port: server.ServerConfiguration.Port,
				password: server.ServerConfiguration.Password
			);

			var log = new LogReceiver(
				self: IPAddress.Parse(thisServer),
				port: 0
			);

			await rcon.SendCommandAsync($"logaddress_add {thisServer}:{log.ResolvedPort}");

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
			server.Connected = true;
			server.ServerInfoUpdated();
			await Task.Delay(-1);
		}
	}
}
