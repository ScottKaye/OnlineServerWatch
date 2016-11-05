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
			Task.Run(async () =>
			{
				var rcon = new RCON();
				await rcon.Connect("192.168.1.8", 27015, "rcon");

				// Set up a listener for any responses that are TF2 statuses
				rcon.Listen<Parsers.TF2.TF2Status>(parsed =>
				{
					Console.WriteLine("Got hostname: " + parsed.Hostname);
				});

				// Send status with callback
				await rcon.SendCommand("status", result =>
				{
					Console.WriteLine("Status received!");
				});

				// Send status with no callback
				await rcon.SendCommand("status", result =>
				{
					Console.WriteLine("received again!");
				});
			}).Wait();

			Console.ReadKey();
		}
	}
}
