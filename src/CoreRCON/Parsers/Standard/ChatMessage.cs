using System;
using System.Text.RegularExpressions;

namespace CoreRCON.Parsers.Standard
{
	[Parser(typeof(ChatMessageParser))]
	public class ChatMessage
	{
		public MessageChannel Channel { get; set; }
		public string Message { get; set; }
		public Player Player { get; set; }
	}

	public class ChatMessageParser : IParser<ChatMessage>
	{
		private static PlayerParser playerParser { get; } = new PlayerParser();
		public string Pattern { get; } = $"{playerParser.Pattern} (?<Channel>say_team|say) \"(?<Message>.+?)\"";

		public bool IsMatch(string input)
		{
			return new Regex(Pattern).IsMatch(input);
		}

		public ChatMessage Load(GroupCollection groups)
		{
			return new ChatMessage
			{
				Player = playerParser.Load(groups),
				Message = groups["Message"].Value,
				Channel = groups["Channel"].Value == "say" ? MessageChannel.All : MessageChannel.Team
			};
		}

		public ChatMessage Parse(string input)
		{
			var groups = new Regex(Pattern).Match(input).Groups;
			return Load(groups);
		}
	}

	public enum MessageChannel
	{
		Team,
		All
	}
}
