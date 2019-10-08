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

		public const int multicastPort = 21000;
		public static readonly IPAddress multicastAddress = IPAddress.Parse("224.0.0.200");
	}
}