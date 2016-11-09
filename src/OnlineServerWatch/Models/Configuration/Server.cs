namespace OnlineServerWatch.Models.Configuration
{
	public class Server
	{
		public string IP { get; set; }
		public ushort Port { get; set; }
		internal string Password { get; set; }
	}
}