using CoreRCON.Parsers.Standard;
using OnlineServerWatch.Models.Configuration;
using System;
using System.Collections.Generic;

namespace OnlineServerWatch.Models.Game
{
	public class GameServer
	{
		private static int _runtimeId = 0;
		public bool Connected { get; set; } = false;
		public List<Player> Players { get; set; } = new List<Player>();

		/// <summary>
		/// Identifier used to identify servers uniquely.
		/// </summary>
		public int RuntimeId { get; } = ++_runtimeId;

		public Server ServerConfiguration { get; set; }
		public Status Status { get; set; } = new Status();

		internal event Action<ChatMessage> OnChatReceived;

		internal event Action<KillFeed> OnKillReceived;

		internal event Action OnServerInfoUpdated;

		public GameServer(Server server)
		{
			ServerConfiguration = server;
		}

		internal void ChatReceived(ChatMessage message) => OnChatReceived(message);

		internal void KillReceived(KillFeed kill) => OnKillReceived(kill);

		internal void ServerInfoUpdated() => OnServerInfoUpdated();
	}
}