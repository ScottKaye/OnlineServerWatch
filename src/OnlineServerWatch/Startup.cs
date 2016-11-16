using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using OnlineServerWatch.Models.Configuration;
using OnlineServerWatch.Models.Connections;
using OnlineServerWatch.Models.Game;
using System.Collections.Generic;

namespace OnlineServerWatch
{
	public class Startup
	{
		public static IConfigurationRoot Configuration { get; private set; }

		public Startup(IHostingEnvironment env)
		{
			var builder = new ConfigurationBuilder()
				.SetBasePath(env.ContentRootPath)
				.AddJsonFile("servers.json", false, true);

			Configuration = builder.Build();
			var token = Configuration.GetReloadToken();

			// How to actually monitor changes with reloadOnChange
			// https://github.com/aspnet/Configuration/issues/432#issuecomment-221704063
			ChangeToken.OnChange(() => Configuration.GetReloadToken(), () =>
			{

			});
		}

		public void ConfigureServices(IServiceCollection services)
		{
			services.AddMvc();
			services.AddOptions();

			services.AddSignalR(options =>
			{
				options.Hubs.EnableDetailedErrors = true;
			});

			// Manually build the Server list
			services.Configure<List<Server>>(options =>
			{
				foreach (var child in Configuration.GetSection("Servers").GetChildren())
				{
					var server = new Server();
					child.Bind(server);

					// Manually bind password
					server.Password = child.GetValue<string>("Password");
					options.Add(server);
				}
			});
		}

		public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IConnectionManager connectionManager, IOptions<List<Server>> servers)
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

			GameServerManager.ConnectionManager = connectionManager;
			GameServerManager.Import(servers);
			RCONService.Start();
		}
	}
}