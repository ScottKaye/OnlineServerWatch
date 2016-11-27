using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OnlineServerWatch.Models.Configuration;
using OnlineServerWatch.Models.Connections;
using System.Collections.Generic;

namespace OnlineServerWatch
{
	public class Startup
	{
		public static IConfigurationRoot Configuration { get; private set; }

		public Startup(IHostingEnvironment env)
		{
			Configuration = new ConfigurationBuilder()
				.SetBasePath(env.ContentRootPath)
				.AddJsonFile("servers.json", false, true)
				.Build();
		}

		public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
		{
			loggerFactory.AddConsole(LogLevel.Warning);

			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			app.UseStaticFiles();

			app.UseMvc(routes =>
			{
				routes.MapRoute(
					name: "Default",
					template: "{controller=Home}/{action=Index}/{id?}"
				);
			});

			app.UseWebSockets();
			app.UseSignalR("/signalr");
		}

		public void ConfigureServices(IServiceCollection services)
		{
			services.AddMvc();
			services.AddOptions();

			services.AddSignalR(options =>
			{
				options.Hubs.EnableDetailedErrors = true;
			});

			services.Configure<List<Server>>(Configuration.GetSection("Servers"));
			services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
			services.AddSingleton<IRCONService, RCONService>();
			services.AddSingleton<IConfiguration>(Configuration);
		}
	}
}