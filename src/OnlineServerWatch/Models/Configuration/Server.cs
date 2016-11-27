using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace OnlineServerWatch.Models.Configuration
{
	public class Server
	{
		public string IP { get; set; }
		public string Name { get; set; }

		public ushort Port { get; set; }

		[IgnoreDataMember, JsonIgnore]
		public ushort LogPort { get; set; }

		[IgnoreDataMember, JsonIgnore]
		public string Password { get; set; }

		public override string ToString()
		{
			return $"{Name} ({IP}:{Port})";
		}
	}
}