using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace CoreRCON
{
	public class RCON
	{
		/// <summary>
		/// When generating the packet ID, use a never-been-used (for automatic packets) ID
		/// </summary>
		private static int packetId = 1;

		/// <summary>
		/// Socket object used to connect to RCON.
		/// </summary>
		private Socket socket { get; set; } = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

		/// <summary>
		/// Map of pending command references.  These are called when a command with the matching Id (key) is received.  Commands are called only once.
		/// </summary>
		private Dictionary<int, Action<string>> pendingCommands { get; set; } = new Dictionary<int, Action<string>>();

		/// <summary>
		/// Allows us to keep track of when authentication succeeds, so we can block Connect from returning until it does.s
		/// </summary>
		private TaskCompletionSource<bool> authenticated = new TaskCompletionSource<bool>();

		/// <summary>
		/// Connect to a server through RCON.  Automatically sends the authentication packet.
		/// </summary>
		/// <param name="hostname">Resolvable hostname.</param>
		/// <param name="port">Port number RCON is listening on.</param>
		/// <param name="password">RCON password.</param>
		/// <returns>Awaitable which will complete when a successful connection is made and authentication is successful.</returns>
		public async Task Connect(string hostname, ushort port, string password)
		{
			if (hostname == null) throw new NullReferenceException("Hostname cannot be null.");
			if (password == null) throw new NullReferenceException("Password cannot be null (authentication will always fail).");

			await socket.ConnectAsync(hostname, port);

			// Set up listener
			var e = new SocketAsyncEventArgs();
			e.Completed += PacketReceived;
			e.SetBuffer(new byte[Constants.MAX_PACKET_SIZE], 0, Constants.MAX_PACKET_SIZE);

			// Start listening for responses
			socket.ReceiveAsync(e);

			// Wait for successful authentication
			SendPacket(new Packet(0, PacketType.Auth, password));
			await authenticated.Task;
		}

		/// <summary>
		/// Event called whenever raw data is received on the socket.
		/// </summary>
		private void PacketReceived(object sender, SocketAsyncEventArgs e)
		{
			// Parse out the actual RCON packet
			Packet packet = Packet.FromBytes(e.Buffer);

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

			// Call pending result and remove from map
			Action<string> action;
			if (pendingCommands.TryGetValue(packet.Id, out action))
			{
				action(packet.Body);
				pendingCommands.Remove(packet.Id);
			}

			socket.ReceiveAsync(e);
		}

		/// <summary>
		/// Send a packet to the server.
		/// </summary>
		/// <param name="packet">Packet to send, which will be serialized.</param>
		public void SendPacket(Packet packet)
		{
			socket.Send(packet.ToBytes());
		}

		/// <summary>
		/// Send a command to the server, and call the result when a response is received.
		/// </summary>
		/// <param name="command">Command to send to the server.</param>
		/// <param name="result">Response from the server.</param>
		public void SendCommand(string command, Action<string> result)
		{
			// Get a unique integer
			Packet packet = new Packet(packetId, PacketType.ExecCommand, command);
			pendingCommands.Add(packetId, result);
			SendPacket(packet);
			++packetId;
		}
	}
}
