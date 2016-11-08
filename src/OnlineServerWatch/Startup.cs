using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OnlineServerWatch.Models.Configuration;
using OnlineServerWatch.Models.Connections;

namespace OnlineServerWatch
{
	public class Startup
	{
		public static IConfigurationRoot Configuration { get; private set; }

		public Startup(IHostingEnvironment env)
		{
			var builder = new ConfigurationBuilder()
				.SetBasePath(env.ContentRootPath);

			Configuration = builder.Build();
		}

		public void ConfigureServices(IServiceCollection services)
		{
			services.AddSignalR(options =>
			{
				options.Hubs.EnableDetailedErrors = true;
			});
			services.AddMvc();
			services.AddOptions();
		}

		public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IConnectionManager connectionManager)
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

			new RCONService(connectionManager);
		}
	}
}