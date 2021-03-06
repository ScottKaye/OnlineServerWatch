﻿using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Hubs;
using OnlineServerWatch.Models.Connections;
using System.Threading.Tasks;

namespace OnlineServerWatch.Hubs
{
	[HubName("RCONHub")]
	public class RCONHub : Hub
	{
		private ILogAddressService _rcon;

		public RCONHub(ILogAddressService rcon)
		{
			_rcon = rcon;
		}

		public override Task OnConnected()
		{
			Clients.Caller.UpdateAll(_rcon.GameServers);
			return base.OnConnected();
		}
	}
}