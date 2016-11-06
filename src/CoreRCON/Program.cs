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
				await rcon.Connect("192.168.1.8", 27015, "rcon");
				await rcon.StartLogging();

				// Set up a listener for any responses that are TF2 statuses
				rcon.Listen<Parsers.TF2.TF2Status>(parsed =>
				{
					Console.WriteLine($"A status was parsed - Hostname: {parsed.Hostname}");
				});

				// Listen to all raw respones as well
				rcon.Listen(raw =>
				{
					Console.WriteLine($"Raw string: {raw}");
				});

				// Reconnect if the connection is ever lost
				await rcon.KeepAlive();
			});

			// .Wait() puts exceptions into an AggregateException, while .GetResult() doesn't
			task.GetAwaiter().GetResult();
		}
	}
}
