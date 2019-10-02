/*
Leo Tamminen

Resources:
	https://docs.microsoft.com/en-us/dotnet/framework/network-programming/using-udp-services
	https://gist.github.com/darkguy2008/413a6fea3a5b4e67e5e0d96f750088a9
	https://stackoverflow.com/questions/22852781/how-to-do-network-discovery-using-udp-broadcast

How to make forms application:
	https://docs.microsoft.com/en-us/dotnet/framework/winforms/how-to-create-a-windows-forms-application-from-the-command-line

Protocol:
	Server: <listening broadcasts>
	Client:	<broadcast> "MORKO get servers" 
	Server:	"MORKO server introduction [server name]"
	CLient:	"MORKO join request [player name, (other info??)]"
	Server:	"MORKO join confirm [player id, -1 if declined]"
	Client: "MORKO join complete"

*/

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

using Morko.Network;

using static System.Console;

public class PlayerInfo
{
	public string name;
	public IPEndPoint endPoint;
}

class Program
{

	public const int serverBroadcastPort = 11000;
	static void WriteLine(string text) => Console.WriteLine(text);

	public static void Main(string [] arguments)
	{
		if (arguments.Length == 0)
		{
			WriteLine("Enter host name as argument");
			return;
		}

		/* Note(Leo): This is called in case user presses ctrl+c to shut down
		program. Also, this is our only way to exit right now  :).

		TODO(Leo): This should instead set false a boolean variable that
		controls the main loop. Now some operations may be halted midway.
		*/

		string serverName = arguments[0];
		Server server = Server.Create(serverName);
		
		void CancelKeyPressHandler (object sender, ConsoleCancelEventArgs e)
		{
			server.Close();
			Console.WriteLine("Exited gracefully");
		}
		Console.CancelKeyPress += CancelKeyPressHandler;

		WriteLine("Started udpServer, [Ctrl+C] to shut down");

		server.WaitForPlayersThread();
	}
}

public class Server
{
	private string name;
	private UdpClient udpClient;
	private Dictionary<int, PlayerInfo> players;

	private bool threadsAlive;

	private Server() {}

	public static Server Create(string name)
	{
		Server server = new Server();
		
		server.name = name;
		server.udpClient = new UdpClient(Program.serverBroadcastPort);
		server.players = new Dictionary<int, PlayerInfo>();

		return server;
	}

	public void Close()
	{
		threadsAlive = false;
		udpClient.Close();
	}

	int playerCount = 0;

	public void WaitForPlayersThread()
	{
		threadsAlive = true;

		while(threadsAlive)
		{
			IPEndPoint receiveEndPoint 	= new IPEndPoint(new IPAddress(0), 0);
			var data = udpClient.Receive(ref receiveEndPoint);

			if (Commands.IsCommand(data) == false)
				continue;

			var command = Commands.GetCommand(data);
			var arguments = Commands.GetArguments(data);
			WriteLine($"Command = {command}");

			byte [] response = null;
			switch (command)
			{
				case NetworkCommand.GetServers:
					response = Commands.MakeCommand(NetworkCommand.IntroduceServer, name);
					break;

				case NetworkCommand.JoinRequest:
				{
					var newPlayer = new PlayerInfo
					{
						name = arguments,
						endPoint = receiveEndPoint
					};

					int id = playerCount++;
					players.Add(id, newPlayer);

					WriteLine($"{arguments} requested access to server!"); 
				} break;

				case NetworkCommand.JoinComplete:
				{
					int id = int.Parse(arguments);
					WriteLine($"{players[id].name} confirmed joining on server!");
				} break;

				case NetworkCommand.StartGame:
				{
					int count = players.Count;
					WriteLine($"{count} players have joined");
					foreach (var entry in players)
					{
						WriteLine($"\t#{entry.Key}: {entry.Value.name}@{entry.Value.endPoint}");
					}

				} break;

				default:
					response = Commands.MakeCommand(NetworkCommand.Invalid);
					break;

			}
			if (response != null)
			{
				udpClient.Send(response, response.Length, receiveEndPoint);
			}
		}	
	}
}