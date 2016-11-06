using CoreRCON.PacketFormats;
using System;
using System.Threading.Tasks;

namespace CoreRCON
{
	public class Program
	{
		/// <summary>
		/// Example program for CoreRCON.
		/// </summary>
		internal static void Main(string[] args)
		{
			var task = Task.Run(async () =>
			{
				var rcon = new RCON();
				await rcon.ConnectAsync("192.168.1.8", 27015, "rcon");
				await rcon.StartLogging("192.168.1.8");

				// Set up a listener for any responses that are TF2 statuses
				rcon.Listen<Parsers.TF2.TF2Status>(parsed =>
				{
					Console.WriteLine($"A status was parsed - Hostname: {parsed.Hostname}");
				});

				// Listen to all raw responses as strings
				rcon.Listen(raw =>
				{
					Console.WriteLine($"Received a raw string: {raw.Truncate(100).Replace("\n", "")}");
				});

				// Listen to all raw responses, but get their full packets
				rcon.Listen((LogAddressPacket packet) =>
				{
					Console.WriteLine($"Received a LogAddressPacket: Time - {packet.Timestamp} Body - {packet.Body}");
				});

				await rcon.SendCommandAsync("status");

				// Reconnect if the connection is ever lost
				await rcon.KeepAliveAsync();
			});

			// .Wait() puts exceptions into an AggregateException, while .GetResult() doesn't
			task.GetAwaiter().GetResult();
		}
	}
}
