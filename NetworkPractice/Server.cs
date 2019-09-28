/*
Leo Tamminen

Resources:
	https://docs.microsoft.com/en-us/dotnet/framework/network-programming/using-udp-services
	https://gist.github.com/darkguy2008/413a6fea3a5b4e67e5e0d96f750088a9
	https://stackoverflow.com/questions/22852781/how-to-do-network-discovery-using-udp-broadcast
*/

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

class Program
{
	private const int serverPort = 11000;
	static void Print(string text) => Console.WriteLine(text);

	public static void Main(string [] arguments)
	{
		if (arguments.Length == 0)
		{
			Print("Enter host name as argument");
			return;
		}

		string serverName = arguments[0];

		UdpClient server = new UdpClient(serverPort);
		IPEndPoint receiveEndPoint = new IPEndPoint(new IPAddress(0), 0);

		/* Note(Leo): This is called in case user presses ctrl+c to shut down
		program. Also, this is our only way to exit right now  :) */
		void CancelKeyPressHandler (object sender, ConsoleCancelEventArgs e)
		{
			server.Close();
			Console.WriteLine("Exited gracefully");
		}
		Console.CancelKeyPress += CancelKeyPressHandler;

		Print("Started server, [Ctrl+C] to shut down");

		while(true)
		{
			Print("Wait for client broadcast");
			var requestData = server.Receive(ref receiveEndPoint);
			var request 	= Encoding.ASCII.GetString(requestData, 0, requestData.Length);

			bool properRequest = string.CompareOrdinal(request, 0, "MORKO", 0, 5) == 0;

			string response = 	properRequest ?
								$"Willkommen auf {serverName}":
								"Schleich dich!";

			var responseData = Encoding.ASCII.GetBytes(response);
			server.Send(responseData, responseData.Length, receiveEndPoint);
			Print($"Received, send response to {receiveEndPoint}\n");

		}
	}
}