using CoreRCON.Parsers.Standard;
using OnlineServerWatch.Models.Configuration;
using System;
using System.Collections.Generic;

namespace OnlineServerWatch.Models.Game
{
	public class GameServer
	{
		internal Server ServerConfiguration { get; set; }
		public string RuntimeId { get; } = Guid.NewGuid().ToString();
		public List<Player> Players { get; set; }
		public Status Status { get; set; }
		public bool Connected { get; set; } = false;

		internal event Action<ChatMessage> OnChatReceived;
		internal event Action<KillFeed> OnKillReceived;

		public GameServer(Server server)
		{
			ServerConfiguration = server;
		}

		internal void ChatReceived(ChatMessage message) => OnChatReceived(message);
		internal void KillReceived(KillFeed kill) => OnKillReceived(kill);
	}
}