using System;
using System.Text.RegularExpressions;

namespace CoreRCON.Parsers.TF2
{
	[Parser(typeof(TF2StatusParser))]
	public class TF2Status
	{
		public string Hostname { get; set; }
	}

	internal class TF2StatusParser : IParser<TF2Status>
	{
		private string pattern { get; set; } = @"hostname: (?<Hostname>.+?)\n";
		private RegexOptions options { get; set; } = RegexOptions.Singleline;

		public bool IsMatch(string line)
		{
			return new Regex(pattern, options).IsMatch(line);
		}

		public TF2Status Parse(string line)
		{
			var groups = new Regex(pattern, options).Match(line).Groups;
			return new TF2Status
			{
				Hostname = groups["Hostname"].Value
			};
		}
	}
}
