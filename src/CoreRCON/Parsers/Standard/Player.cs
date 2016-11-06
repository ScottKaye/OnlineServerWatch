using System;
using System.Text.RegularExpressions;

namespace CoreRCON.Parsers.Standard
{
	[Parser(typeof(PlayerParser))]
	public class Player
	{
		public string Name { get; set; }
		public string SteamId { get; set; }
		public int ClientId { get; set; }
		public string Team { get; set; }
	}

	public class PlayerParser : IParser<Player>
	{
		public string Pattern { get; } = "\"(?<Name>.+?(?:<\\d+?>)?)<(?<ClientID>\\d+?)><(?<SteamID>.+?)><(?<Team>.+?)>\"";

		public bool IsMatch(string line)
		{
			return new Regex(Pattern).IsMatch(line);
		}

		public Player Load(GroupCollection groups)
		{
			return new Player
			{
				Name = groups["Name"].Value,
				SteamId = groups["SteamID"].Value,
				ClientId = int.Parse(groups["ClientID"].Value),
				Team = groups["Team"].Value
			};
		}

		public Player Parse(string line)
		{
			var groups = new Regex(Pattern).Match(line).Groups;
			return Load(groups);
		}
	}
}
