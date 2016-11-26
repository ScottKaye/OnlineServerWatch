using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace OnlineServerWatch.Models.Configuration
{
	public class Server
	{
		public string Name { get; set; }
		public string IP { get; set; }
		public ushort Port { get; set; }

		[IgnoreDataMember, JsonIgnore]
		public string Password { get; set; }
	}
}