/*
Leo Tamminen
*/

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

using static System.Console;

class Program
{
	private const string joinRequest = "MORKO REQUEST JOIN";
	private const string joinConfrim = "MORKO CONFIRM JOIN";
	private const string confirmConfirm = "MORKO CONFIRM CONFIRM";

	private const int serverBroadcastPort = 11000;

	public static void Main (string [] arguments)
	{
		// Note(Leo): 0 means that system will assign (random?) available port
		var client 			= new UdpClient(0);
		var serverEndPoint 	= new IPEndPoint(IPAddress.Any, 0);
		
		/* Note(Leo): Direct broadcast address is one where host bits are all set to 1,
		i.e. the biggest possible address in subnet. This comes from protocols. 
		It need however to be gotten from somewhere, using IPAddress.Broadcast is easier*/
		// IPAddress directBroadcastAddress = IPAddress.Parse("192.168.43.255");

		// Note(Leo): We seem to not need this
		// client.EnableBroadcast = true;
		var broadcastEndPoint = new IPEndPoint(IPAddress.Broadcast, serverBroadcastPort);

		/* Note (Leo): This works but ultimately returns only one server's response,
		while there may be multiple. Next step is to use BeginReceive and EndReceive,
		or asynchronously receive for some fixed time. */

		bool joinedServer = false;

		while (true)
		{
			if (joinedServer)
			{
				Write (">>> ");
			}
			else
			{
				Write ("> ");
			}

			var command = ReadLine();
			if (command.ToLower() == "exit")
			{
				break;
			}

			var data = Encoding.ASCII.GetBytes(command);
			client.Send(data, data.Length, broadcastEndPoint);
	
			var serverResponseData 	= client.Receive(ref serverEndPoint);
			var serverResponse 		= Encoding.ASCII.GetString(serverResponseData);
			WriteLine($"Server ({serverEndPoint}) x	responded: {serverResponse}");
		}
		client.Close();

	}
}