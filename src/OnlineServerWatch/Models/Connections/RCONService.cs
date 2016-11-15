using CoreRCON;
using CoreRCON.Parsers.Standard;
using OnlineServerWatch.Models.Game;
using System;
using System.Linq;
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
				Task.Run(() => Connect(server));
			}
		}

		private static async Task Connect(GameServer server, int retries = 0)
		{
			var rcon = new RCON();

			try
			{
				await rcon.ConnectAsync(
					ip: server.ServerConfiguration.IP,
					port: server.ServerConfiguration.Port,
					password: server.ServerConfiguration.Password,
					udpPort: 0 // Use first free port
				);
				await rcon.StartLogging("192.168.1.8"); // TODO get public IP of server

				rcon.Listen<ChatMessage>(server.ChatReceived);
				rcon.Listen<KillFeed>(server.KillReceived);

				rcon.Listen<PlayerConnected>(connected =>
				{
					server.Players.Add(connected.Player);
					server.ServerInfoUpdated();
				});

				rcon.Listen<PlayerDisconnected>(disconnected =>
				{
					var player = server.Players.FirstOrDefault(p => p.ClientId == disconnected.Player.ClientId);
					if (player == null) return;
					server.Players.Remove(player);
					server.ServerInfoUpdated();
				});

				rcon.Listen<NameChange>(change =>
				{
					var player = server.Players.FirstOrDefault(p => p.ClientId == change.Player.ClientId);
					if (player == null) return;
					player.Name = change.NewName;
					server.ServerInfoUpdated();
				});

				server.Status = await rcon.SendCommandAsync<Status>("status");
				server.Connected = true;
				server.ServerInfoUpdated();
				await rcon.KeepAliveAsync();
			}
			catch (Exception ex)
			{
				Console.Error.WriteLine(ex.Message);

				server.Connected = false;
				server.ServerInfoUpdated();

				// Keep trying to reconnect
				// Wait an increasing amount of time so the website doesn't inadvertently DoS somebody forever
				await Task.Delay(10000 * ++retries);
				await Connect(server, retries);
			}
		}
	}
}
