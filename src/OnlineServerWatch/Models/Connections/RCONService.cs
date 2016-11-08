using CoreRCON;
using CoreRCON.Parsers.Standard;
using Microsoft.AspNetCore.SignalR.Infrastructure;
using OnlineServerWatch.Hubs;
using System.Diagnostics;
using System.Threading.Tasks;

namespace OnlineServerWatch.Models.Connections
{
	public class RCONService
	{
		private IConnectionManager ConnectionManager { get; set; }

		public RCONService(IConnectionManager connectionManager)
		{
			ConnectionManager = connectionManager;

			var context = ConnectionManager.GetHubContext<RCONHub>();

			// Connect to RCON
			Task.Run(async () =>
			{
				var rcon = new RCON();
				await rcon.ConnectAsync("192.168.1.8", 27015, "rcon");
				await rcon.StartLogging("192.168.1.8");

				rcon.Listen<ChatMessage>(message =>
				{
					context.Clients.All.SendMessage(message);
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
