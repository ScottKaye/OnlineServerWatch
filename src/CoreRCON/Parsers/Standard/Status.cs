using System;
using System.Text.RegularExpressions;

namespace CoreRCON.Parsers.Standard
{
	[Parser(typeof(StatusParser))]
	public class Status
	{
		public string Hostname { get; set; }
	}

	internal class StatusParser : IParser<Status>
	{
		public string Pattern { get; } = @"hostname: (?<Hostname>.+?)\n";

		public bool IsMatch(string line)
		{
			return new Regex(Pattern, RegexOptions.Singleline).IsMatch(line);
		}

		public Status Load(GroupCollection groups)
		{
			return new Status
			{
				Hostname = groups["Hostname"].Value
			};
		}

		public Status Parse(string line)
		{
			var groups = new Regex(Pattern, RegexOptions.Singleline).Match(line).Groups;
			return Load(groups);
		}
	}
}
