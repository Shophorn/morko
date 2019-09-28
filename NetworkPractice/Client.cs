/*
Leo Tamminen
*/

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

class Program
{
	private const int serverPort = 11000;
	static void Print(string text) => Console.WriteLine(text);

	public static void Main (string [] arguments)
	{
		if (arguments.Length == 0)
		{
			Print("Enter message as argument");
			return;
		}

		/* Note(Leo): Broadcast address is one where host bits are all set to 1,
		i.e. the biggest possible address in subnet. This comes from protocols. */
		IPAddress broadcastAddress = IPAddress.Parse("192.168.43.255");

		var client = new UdpClient();
		var requestData = Encoding.ASCII.GetBytes(arguments[0]);
		var serverEndPoint = new IPEndPoint(IPAddress.Any, 0);

		// Note(Leo): We seem to not need this
		// client.EnableBroadcast = true;
		client.Send(requestData, requestData.Length, new IPEndPoint(broadcastAddress, serverPort));

		var serverResponseData = client.Receive(ref serverEndPoint);
		var serverResponse = Encoding.ASCII.GetString(serverResponseData);
		Print($"Server responded: {serverResponse}");

		client.Close();
	}
}