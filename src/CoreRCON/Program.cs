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

				// Set up a listener for any responses that are TF2 statuses
				rcon.Listen<Parsers.TF2.TF2Status>(parsed =>
				{
					Console.WriteLine($"A status was parsed - Hostname: {parsed.Hostname}");
				});

				// Send status with callback
				await rcon.SendCommand("status", result =>
				{
					Console.WriteLine("Status received, callback executed!");
				});

				// Send status with no callback
				await rcon.SendCommand("status");
			});

			// .Wait() puts exceptions into an AggregateException, while .GetResult() doesn't
			task.GetAwaiter().GetResult();

			Console.ReadKey();
		}
	}
}
