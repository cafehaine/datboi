using System;
using System.Collections.Generic;
using System.Net;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace datboi
{
	public class Behavior : WebSocketBehavior
	{
		private static List<Behavior> clients = new List<Behavior>();

		public static void sendAll(Byte[] data)
		{
			foreach (Behavior client in clients)
				client.Send(data);
		}

		protected override void OnOpen()
		{
			clients.Add(this);
		}

		protected override void OnClose(CloseEventArgs e)
		{
			clients.Remove(this);
		}

		protected override void OnMessage(MessageEventArgs e)
		{
			if (e.RawData.Length != 4)
			{
				Send("Invalid data.");
				return;
			}
			ushort x = (ushort)((e.RawData[0] << 4) + ((e.RawData[1] & 240) >> 4));
			ushort y = (ushort)(((e.RawData[1] & 15) << 8) + e.RawData[2]);
			byte color = e.RawData[3];
			if (x >= 640 || y >= 480)
			{
				//Send("Invalid data.");
				Program.SetPixel(this, e.RawData);
				return;
			}
		}
	}
}
