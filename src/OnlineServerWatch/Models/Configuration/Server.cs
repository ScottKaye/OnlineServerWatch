using Newtonsoft.Json;
using System.Net;
using System.Runtime.Serialization;

namespace OnlineServerWatch.Models.Configuration
{
	public class Server
	{
		/// <summary>
		/// IP of the remote server.
		/// </summary>
		public string IP { get; set; }

		/// <summary>
		/// Local port expected to recieve logaddress packets on.
		/// </summary>
		[IgnoreDataMember, JsonIgnore]
		public ushort LogPort { get; set; }

		/// <summary>
		/// Vanity name of the server.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Port of the remote server.
		/// </summary>
		public ushort Port { get; set; }

		internal IPEndPoint EndPoint
		{
			get
			{
				return new IPEndPoint(IPAddress.Parse(IP), Port);
			}
		}

		public override string ToString()
		{
			return $"{Name} ({IP}:{Port})";
		}
	}
}