using CoreRCON;
using CoreRCON.Parsers.Standard;
using Microsoft.AspNetCore.SignalR.Infrastructure;
using Microsoft.Extensions.Options;
using OnlineServerWatch.Hubs;
using OnlineServerWatch.Models.Configuration;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace OnlineServerWatch.Models.Connections
{
	public class RCONService
	{
		private IConnectionManager ConnectionManager { get; set; }
		private List<Server> Servers { get; set; }

		public RCONService(IConnectionManager connectionManager, IOptions<List<Server>> servers)
		{
			ConnectionManager = connectionManager;
			Servers = servers.Value;

			var context = ConnectionManager.GetHubContext<RCONHub>();

			foreach (var server in Servers)
			{
				// Connect to RCON
				Task.Run(async () =>
				{
					var rcon = new RCON();
					await rcon.ConnectAsync(
						ip: server.IP,
						port: server.Port,
						password: server.Password
					);
					await rcon.StartLogging("192.168.1.8"); // TODO get public IP of server

					rcon.Listen<ChatMessage>(message =>
					{
						context.Clients.All.SendMessage(server, message);
					});

					// Listen to all raw responses, but get their full packets
					rcon.Listen((CoreRCON.PacketFormats.LogAddressPacket packet) =>
					{
						Debug.WriteLine(packet.RawBody);
					});

					await rcon.KeepAliveAsync();
				});
			}
		}
	}
}
