using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreRCON
{
	public struct Packet
	{
		private static UTF8Encoding utf8 = new UTF8Encoding();

		public readonly int Id;
		public readonly PacketType Type;
		public readonly string Body;

		public Packet(int id, PacketType type, string body)
		{
			Id = id;
			Type = type;
			Body = body;
		}

		internal byte[] ToBytes()
		{
			byte[] body = utf8.GetBytes(Body + "\0");
			int bl = body.Length;

			var packet = new MemoryStream(12 + bl);

			packet.Write(BitConverter.GetBytes(9 + bl), 0, 4);
			packet.Write(BitConverter.GetBytes(Id), 0, 4);
			packet.Write(BitConverter.GetBytes((int)Type), 0, 4);
			packet.Write(body, 0, bl);
			packet.Write(new byte[] { 0 }, 0, 1);

			return packet.ToArray();
		}

		internal static Packet FromBytes(byte[] buffer)
		{
			int size = BitConverter.ToInt32(buffer, 0);
			int id = BitConverter.ToInt32(buffer, 4);
			PacketType type = (PacketType)BitConverter.ToInt32(buffer, 8);

			char[] body = new char[size - 10];
			for (int i = 0; i < size - 10; ++i)
				body[i] = (char)buffer[i + 12];

			return new Packet(id, type, new string(body, 0, size - 10));
		}
	}
}
