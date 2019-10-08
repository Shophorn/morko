/*
Leo Tamminen

Resources:
	https://docs.microsoft.com/en-us/dotnet/framework/network-programming/using-udp-services
	https://gist.github.com/darkguy2008/413a6fea3a5b4e67e5e0d96f750088a9
	https://stackoverflow.com/questions/22852781/how-to-do-network-discovery-using-udp-broadcast

How to make forms application:
	https://docs.microsoft.com/en-us/dotnet/framework/winforms/how-to-create-a-windows-forms-application-from-the-command-line
*/

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Linq;

using Morko.Network;

// using static System.Console;
using static Morko.Logging.Logger;


internal class PlayerInfo
{
	public string name;
	public IPEndPoint endPoint;
	public DateTime lastConnectionTime;
}

internal struct UdpState
{
	public UdpClient client;
	public IPEndPoint endPoint;
}

public class ServerInfo
{
	public string serverName;
	public string logFileName;
	public int broadcastPort;
}

// Todo(Leo): Think if we really need this class
public class ThreadControl
{
	private Thread thread;
	public bool IsAlive { get; private set; }

	public delegate void ControlledThreadFunction(ThreadControl control);

	private ThreadControl() {}

	public static ThreadControl Start(ControlledThreadFunction threadFunction)
	{
		var control = new ThreadControl { IsAlive = true };
		var thread = new Thread(() => threadFunction(control));
		control.thread = thread;
		control.thread.Start();
		return control;
	}

	public Thread Stop()
	{
		IsAlive = false;
		thread.Join();
		return thread;
	}
}

public class Server
{
	static byte [] Encode(string text) => System.Text.Encoding.ASCII.GetBytes(text);
	static string Decode(byte [] data) => System.Text.Encoding.ASCII.GetString(data);

	public int 		BroadcastPort	{ get; private set; }
	public int 		MulticastPort 	{ get; private set; } = 21000;
	public string 	Name 			{ get; private set; }


	private int broadcastDelayMs = 500;
	private int receiveTimeoutMs = 5000;
	private int gameUpdateThreadDelayMs = 500;

	// Note(Leo): https://en.wikipedia.org/wiki/Multicast_address
	private string multicastAddress = "224.0.0.200"; 

	private ThreadControl broadcastControl;
	private bool stopPlayerJoining = false;

	private ThreadControl gameUpdateThreadControl;

	private UdpClient udpClient;

	private List<PlayerInfo> players;

	// Note(Leo): this disables the use of constructor outside class
	private Server() {}

	public static Server Create(ServerInfo info)
	{
		var server = new Server
		{
			Name = info.serverName,
			udpClient = new UdpClient(0),
			BroadcastPort = info.broadcastPort,
		};

		server.udpClient.Client.ReceiveTimeout = server.receiveTimeoutMs;
		server.players = new List<PlayerInfo>();
		return server;
	}

	private void BroadcastThread(ThreadControl control)
	{
		var broadcastEndPoint = new IPEndPoint(IPAddress.Broadcast, BroadcastPort);
		while(control.IsAlive)
		{
			var data = ProtocolFormat.MakeCommand(NetworkCommand.ServerIntroduce, Name);
			udpClient.Send(data, data.Length, broadcastEndPoint);
			Thread.Sleep(broadcastDelayMs);
		}
		Log("Stopped broadcast thread");
	}

	private void ReceiveCallback(IAsyncResult result)
	{
		var state = (UdpState)result.AsyncState;
		var udpClient = state.client;
		var endPoint = state.endPoint;

		var data = udpClient.EndReceive(result, ref endPoint);

		if (stopPlayerJoining)
			return;

		if (ProtocolFormat.TryParseCommand(data, out NetworkCommand command, out string arguments))
		{
			switch (command)
			{
				case NetworkCommand.PlayerJoin:
					var existingPlayer = players.Find(player => IPEndPoint.Equals(player.endPoint, endPoint));
					if (existingPlayer != null)
					{
						existingPlayer.lastConnectionTime = DateTime.Now;
					}
					else
					{
						var playerName = arguments;
						players.Add(new PlayerInfo 
						{
							endPoint 			= endPoint,
							name 				= playerName,
							lastConnectionTime 	= DateTime.Now
						});
						Log($"Added player {playerName}");
					}

					break;
				
			}
		}
	
		udpClient.BeginReceive(ReceiveCallback, state);
	}

	public void StartBroadcasting()
	{
		stopPlayerJoining = false;
		broadcastControl = ThreadControl.Start(BroadcastThread);

		UdpState state = new UdpState
		{
			client = udpClient,
			endPoint = new IPEndPoint(IPAddress.Any, 0)
		};

		udpClient.BeginReceive(ReceiveCallback, state);
	}

	public void StopBroadcasting()
	{
		stopPlayerJoining = true;
		broadcastControl.Stop();
	}

	public void StartGame()
	{
		gameUpdateThreadControl = ThreadControl.Start(GameUpdateThread);
	}

	public void StopGame()
	{
		gameUpdateThreadControl.Stop();
	}

	public int PlayerCount => players.Count;
	public string [] PlayersNames => players.Select(player => player.name).ToArray();
 	
 	public void Close()
	{
		udpClient.Close();
	}

	private void GameUpdateThread(ThreadControl control)
	{
		IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(multicastAddress), MulticastPort);
		udpClient.JoinMulticastGroup(IPAddress.Parse(multicastAddress));
		while(control.IsAlive)
		{
			var data = ProtocolFormat.MakeCommand(NetworkCommand.ServerMulticastTest, "I am multicaster");
			udpClient.Send(data, data.Length, endPoint);
			Log(Decode(data));

			Thread.Sleep(gameUpdateThreadDelayMs);
		}
		udpClient.DropMulticastGroup(IPAddress.Parse(multicastAddress));
	}

	// public static string GetLocalIPAddress()
	// {
	//     var host = Dns.GetHostEntry(Dns.GetHostName());
	//     foreach (var ip in host.AddressList)
	//     {
	//         if (ip.AddressFamily == AddressFamily.InterNetwork)
	//         {
	//             return ip.ToString();
	//         }
	//     }
	//     throw new Exception("No network adapters with an IPv4 address in the system!");
	// }

}