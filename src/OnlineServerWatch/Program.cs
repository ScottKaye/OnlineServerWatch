using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace OnlineServerWatch
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var host = new WebHostBuilder()
				.UseUrls("http://*:8000")
				.UseContentRoot(Directory.GetCurrentDirectory())
				.UseStartup<Startup>()
				.UseKestrel()
				.Build();

			host.Run();
		}
	}
}