using Microsoft.Extensions.Options;
using OnlineServerWatch.Models.Configuration;
using System.Collections.Generic;
using Microsoft.AspNetCore.SignalR.Infrastructure;
using OnlineServerWatch.Hubs;
using System.Diagnostics.Contracts;
using System;

namespace OnlineServerWatch.Models.Game
{
	public class GameServerManager
	{
		internal static IConnectionManager ConnectionManager { get; set; }
		public static List<GameServer> GameServers { get; set; } = new List<GameServer>();

		public static void Import(IOptions<List<Server>> servers)
		{
			var context = ConnectionManager.GetHubContext<RCONHub>();

			foreach (var server in servers.Value)
			{
				var gameServer = new GameServer(server);
				GameServers.Add(gameServer);

				gameServer.OnChatReceived += message =>
				{
					context.Clients.All.Chat(gameServer, message);
				};

				gameServer.OnKillReceived += kill =>
				{
					context.Clients.All.Kill(gameServer, kill);
				};

				gameServer.OnServerInfoUpdated += () =>
				{
					context.Clients.All.Update(gameServer);
				};
			}
		}
	}
}