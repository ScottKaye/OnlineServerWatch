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

				rcon.SendCommand("status", result =>
				{
					Console.WriteLine("status received:");
					Console.WriteLine(result);
				});
			}).Wait();

			Console.ReadKey();
		}
	}
}
