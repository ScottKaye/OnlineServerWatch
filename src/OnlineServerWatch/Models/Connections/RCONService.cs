using CoreRCON;
using CoreRCON.Parsers.Standard;
using OnlineServerWatch.Models.Game;
using System.Diagnostics;
using System.Threading.Tasks;

namespace OnlineServerWatch.Models.Connections
{
	public class RCONService
	{
		public static void Start()
		{
			foreach (var server in GameServerManager.GameServers)
			{
				// Connect to RCON
				Task.Run(async () =>
				{
					var rcon = new RCON();
					await rcon.ConnectAsync(
						ip: server.ServerConfiguration.IP,
						port: server.ServerConfiguration.Port,
						password: server.ServerConfiguration.Password
					);
					await rcon.StartLogging("192.168.1.8"); // TODO get public IP of server

					rcon.Listen<ChatMessage>(server.ChatReceived);
					rcon.Listen<KillFeed>(server.KillReceived);
					rcon.Listen((CoreRCON.PacketFormats.LogAddressPacket packet) =>
					{
						Debug.WriteLine(packet.RawBody);
					});

					server.Status = await rcon.SendCommandAsync<Status>("status");
					server.Connected = true;
					await rcon.KeepAliveAsync();
				});
			}
		}
	}
}
