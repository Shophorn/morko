/*
Leo Tamminen
shophorn@protonmail.com

Global constants that are used in and outside this network library.
*/

using System.Net;

namespace Morko.Network
{
	public static class Constants
	{
		public const int broadcastPort = 11000;
		public static readonly IPAddress broadcastAddress = IPAddress.Broadcast; // 255.255.255.255
		// // Note(Leo): Only for testing
		// public static readonly IPAddress broadcastAddress = IPAddress.Parse("192.168.43.255");

		// Note(Leo): https://en.wikipedia.org/wiki/Multicast_address
		public const int multicastPort = 21001;
		public static readonly IPAddress multicastAddress = IPAddress.Parse("224.0.0.200");

		public const int serverReceivePort = 21002;
		public const int serverTcpListenPort = 21003;
	}
}