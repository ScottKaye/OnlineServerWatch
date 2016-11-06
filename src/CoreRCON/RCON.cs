using CoreRCON.Parsers;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;
using System.Security;

namespace CoreRCON
{
	public class RCON
	{
		#region Fields & Properties
		/// <summary>
		/// When generating the packet ID, use a never-been-used (for automatic packets) ID.
		/// </summary>
		private static int packetId = 1;

		/// <summary>
		/// Socket object used to connect to RCON.
		/// </summary>
		private Socket socket { get; set; }

		/// <summary>
		/// Map of pending command references.  These are called when a command with the matching Id (key) is received.  Commands are called only once.
		/// </summary>
		private Dictionary<int, Action<string>> pendingCommands { get; set; } = new Dictionary<int, Action<string>>();

		/// <summary>
		/// Allows us to keep track of when authentication succeeds, so we can block Connect from returning until it does.
		/// </summary>
		private TaskCompletionSource<bool> authenticated;

		/// <summary>
		/// List of listeners to be polled whenever a packet is received.
		/// </summary>
		private List<ParserContainer> parsers { get; set; } = new List<ParserContainer>();

		/// <summary>
		/// Raw string listeners
		/// </summary>
		private List<Action<string>> listeners { get; set; } = new List<Action<string>>();

		private string Hostname { get; set; }
		private ushort Port { get; set; }
		private string Password { get; set; }
		#endregion

		/// <summary>
		/// Connect to a server through RCON.  Automatically sends the authentication packet.
		/// </summary>
		/// <param name="hostname">Resolvable hostname.</param>
		/// <param name="port">Port number RCON is listening on.</param>
		/// <param name="password">RCON password.</param>
		/// <returns>Awaitable which will complete when a successful connection is made and authentication is successful.</returns>
		public async Task Connect(string hostname, ushort port, string password)
		{
			Hostname = hostname;
			Port = port;
			Password = password;

			if (Hostname == null) throw new NullReferenceException("Hostname cannot be null.");
			if (Password == null) throw new NullReferenceException("Password cannot be null (authentication will always fail).");

			socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			await socket.ConnectAsync(Hostname, Port);

			// Set up listener
			var e = new SocketAsyncEventArgs();
			e.Completed += PacketReceived;
			e.SetBuffer(new byte[Constants.MAX_PACKET_SIZE], 0, Constants.MAX_PACKET_SIZE);

			// Start listening for responses
			socket.ReceiveAsync(e);

			// Wait for successful authentication
			authenticated = new TaskCompletionSource<bool>();
			await SendPacket(new Packet(0, PacketType.Auth, Password));
			await authenticated.Task;
		}

		/// <summary>
		/// Listens on the socket for a parseable class to read.
		/// </summary>
		/// <typeparam name="T">Class to be parsed; must have a ParserAttribute.</typeparam>
		/// <param name="result">Parsed class.</param>
		public void Listen<T>(Action<T> result)
			where T : class
		{
			// Instantiate the parser associated with the type parameter
			var parserAttribute = typeof(T).GetTypeInfo().GetCustomAttribute<ParserAttribute>();
			if (parserAttribute == null) throw new ArgumentException($"Class {typeof(T).FullName} does not have a Parser attribute.");

			var instance = (IParser<T>)Activator.CreateInstance(parserAttribute.ParserType);

			// Create the parser container
			parsers.Add(new ParserContainer
			{
				IsMatch = line => instance.IsMatch(line),
				Parse = line => instance.Parse(line),
				Callback = parsed =>
				{
					result(parsed as T);
				}
			});
		}

		/// <summary>
		/// Listens on the socket for anything.
		/// </summary>
		/// <param name="result">Raw string returned by the server.</param>
		public void Listen(Action<string> result)
		{
			listeners.Add(result);
		}

		/// <summary>
		/// Polls the server to check if RCON is still authenticated.  Will still throw if the password was changed elsewhere.
		/// </summary>
		/// <param name="delay">Time in milliseconds to wait between polls.</param>
		public async Task KeepAlive(int delay = 60000)
		{
			while (true)
			{
				try
				{
					await SendCommand("");
				}
				catch (Exception)
				{
					Console.Error.WriteLine($"{DateTime.Now} - Disconnected from {socket.RemoteEndPoint}... Attempting to reconnect.");
					await Connect(Hostname, Port, Password);
					return;
				}

				await Task.Delay(delay);
			}
		}

		public async Task StartLogging()
		{
			await SendCommand($"logaddress_add {socket.LocalEndPoint}", result =>
			{
				Console.WriteLine("Now logging top kljasd");
			});
		}

		/// <summary>
		/// Send a packet to the server.
		/// </summary>
		/// <param name="packet">Packet to send, which will be serialized.</param>
		public async Task SendPacket(Packet packet)
		{
			socket.Send(packet.ToBytes());
			await Task.Delay(10);
		}

		/// <summary>
		/// Send a command to the server, and call the result when a response is received.
		/// </summary>
		/// <param name="command">Command to send to the server.</param>
		/// <param name="result">Response from the server.</param>
		public async Task SendCommand(string command, Action<string> result = null)
		{
			// Get a unique integer
			Packet packet = new Packet(++packetId, PacketType.ExecCommand, command);
			pendingCommands.Add(packetId, result);
			await SendPacket(packet);
		}

		/// <summary>
		/// Event called whenever raw data is received on the socket.
		/// </summary>
		private void PacketReceived(object sender, SocketAsyncEventArgs e)
		{
			// Parse out the actual RCON packet
			Packet packet = Packet.FromBytes(e.Buffer);

			Console.WriteLine($"PACKET RECEIVED! {packet.Id}-{packet.Type}-{packet.Body}");

			if (packet.Type == PacketType.AuthResponse)
			{
				// Failed auth responses return with an ID of -1
				if (packet.Id == -1)
				{
					throw new AuthenticationException($"Authentication failed for {socket.RemoteEndPoint}.");
				}

				// Tell Connect that authentication succeeded
				authenticated.SetResult(true);
			}

			// Call listeners
			foreach (var listener in listeners)
			{
				listener(packet.Body);
			}

			// Call pending result and remove from map
			Action<string> action;
			if (pendingCommands.TryGetValue(packet.Id, out action))
			{
				action?.Invoke(packet.Body);
				pendingCommands.Remove(packet.Id);
			}

			// Call parsers
			foreach (var parser in parsers)
			{
				parser.TryCallback(packet.Body);
			}

			socket.ReceiveAsync(e);
		}
	}
}