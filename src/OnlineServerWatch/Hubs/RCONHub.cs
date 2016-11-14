using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Hubs;
using OnlineServerWatch.Models.Game;

namespace OnlineServerWatch.Hubs
{
	[HubName("RCONHub")]
	public class RCONHub : Hub
	{
		public override Task OnConnected()
		{
			Clients.Caller.UpdateAll(GameServerManager.GameServers);
			return base.OnConnected();
		}
	}
}