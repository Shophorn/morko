using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Linq;

using Morko.Network;

using static System.Console;
using static Morko.Logging.Logger;

class Program
{
	public static void Main(string [] commandLine)
	{
		string name = commandLine.Length > 0 ? commandLine[0] : "DefaultServer";
		Server server = Server.Create(name, 11000);

		void CancelKeyPressHandler (object sender, ConsoleCancelEventArgs e)
		{
			server.StopBroadcasting();
			WriteLine ($"Stopped server '{server.Name}'");
		}
		Console.CancelKeyPress += CancelKeyPressHandler;

		Log("Start broadcasting");

		server.StartBroadcasting();

		// Just sleep for now, for ever
		while(true)
		{
			Thread.Sleep(100);
		}
	}
}

class PlayerInfo
{
	public string name;
	public IPEndPoint endPoint;
	public DateTime lastConnectionTime;
}

struct UdpState
{
	public UdpClient client;
	public IPEndPoint endPoint;
}

class ThreadControl
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

class Server
{
	static byte [] Encode(string text) => System.Text.Encoding.ASCII.GetBytes(text);
	static string Decode(byte [] data) => System.Text.Encoding.ASCII.GetString(data);

	public int 		BroadcastPort	{ get; private set; }
	public string 	Name 			{ get; private set; }

	private static int broadcasDelayMs = 500;
	private static int receiveTimeoutMs = 5000;

	private ThreadControl broadcastControl;

	private UdpClient udpClient;


	private List<PlayerInfo> players;

	// Note(Leo): this disables the use of constructor outside class
	private Server() {}

	public static Server Create(string name, int broadcastPort)
	{
		var server = new Server
		{
			Name = name,
			udpClient = new UdpClient(0),
			BroadcastPort = broadcastPort,
		};

		server.udpClient.Client.ReceiveTimeout = receiveTimeoutMs;
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
			Thread.Sleep(broadcasDelayMs);
			// WriteLine("Sent data");
		}
		WriteLine("Stopped broadcast thread");
	}

	private void ReceiveCallback(IAsyncResult result)
	{
		var state = (UdpState)result.AsyncState;
		var udpClient = state.client;
		var endPoint = state.endPoint;

		var data = udpClient.EndReceive(result, ref endPoint);
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
		WriteLine("Hello before stop");

		broadcastControl.Stop();
		

		WriteLine($"Stopped server");

		udpClient.Close();
		WriteLine("Hello after stop");
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