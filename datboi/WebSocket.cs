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

		public static void sendAll(byte[] data)
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
			Program.SetPixel(this, e.RawData);
			return;
		}
	}
}
