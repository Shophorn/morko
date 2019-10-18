/*
Leo Tamminen
shophorn@protonmail.com

NetworkCommand enum describes the function we want to execute
*/

using System.Runtime.InteropServices;	

namespace Morko.Network
{
	public enum NetworkCommand : byte
	{
		/* Note(Leo): When no other values between 1 and Undefined are specified
		Undefined will have biggest and we can easily check if any value might
		be a proper NetworkCommand. */
		None = 0,

		ServerIntroduce = 1,
		ServerConfirmJoin,
		ServerStartGame,

		ClientRequestJoin,


		// Note(Leo): Updates may have some data in addition to Args structure.
		ServerLobbyUpdate,
		ServerGameUpdate,
		ClientGameUpdate,

		Undefined,
	}

	public interface INetworkCommandArgs
	{
		NetworkCommand Command { get; }
	}

	/* Note(Leo): These structs are to be sent over network,
	so they must be constructed carefully.*/
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct ServerIntroduceArgs : INetworkCommandArgs
	{
		public NetworkCommand Command => NetworkCommand.ServerIntroduce;

		public NetworkName name;
		public int mapId;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct ServerConfirmJoinArgs : INetworkCommandArgs
	{
		public NetworkCommand Command => NetworkCommand.ServerConfirmJoin;

		public int playerId;

		[MarshalAs(UnmanagedType.I1)] // 1-byte integer, can be used as a bool, like in C
		public bool accepted;
	} 

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct ServerStartGameArgs : INetworkCommandArgs
	{
		public NetworkCommand Command => NetworkCommand.ServerStartGame;

		public int playerCount;
		public int mapId;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct ClientRequestJoinArgs : INetworkCommandArgs
	{
		public NetworkCommand Command => NetworkCommand.ClientRequestJoin;

		public NetworkName playerName;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct ClientGameUpdateArgs : INetworkCommandArgs
	{
		public NetworkCommand Command => NetworkCommand.ClientGameUpdate;

		public int playerId;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct ServerGameUpdateArgs : INetworkCommandArgs
	{
		public NetworkCommand Command => NetworkCommand.ServerGameUpdate;

		public int playerId;
	}
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct PlayerStartInfo
{
	public Morko.Network.NetworkName 	name;
	public int 							playerId;
	public int 							avatarId;
}
