/*
Leo Tamminen
shophorn@protonmail.com
*/

namespace Morko.Network
{
	public enum NetworkCommand : byte
	{
		/* Note(Leo): When no other values between 1 and Undefined are specified
		Undefined will have biggest and we can easily check if any value might
		be a proper NetworkCommand. */
		None = 0,

		ServerIntroduce = 1,
		PlayerJoin,

		Undefined,
	}

	// static class Commands
	// {
	// 	private const string idString = "MORKO";
	// 	private const int commandByteIndex = 5;

	// 	private static readonly byte [] idBytes = Encoding.ASCII.GetBytes("MORKO");
	// 	private const int idBytesCount = 5;

	// 	public static bool IsCommand(byte [] data)
	// 	{
	// 		bool isCommand = data.Length > commandByteIndex;
	// 		for (int i = 0; i < idBytesCount && isCommand; i++)
	// 		{
	// 			isCommand = isCommand && (idBytes[i] == data[i]);
	// 		}
	// 		return isCommand;
	// 	}

	// 	public static NetworkCommand GetCommand(byte [] data)
	// 	{
	// 		NetworkCommand command = (NetworkCommand)data[commandByteIndex];
	// 		if (command < NetworkCommand.Undefined)
	// 			return command;
	// 		else
	// 			return NetworkCommand.Undefined;
	// 	} 

	// 	public static byte [] MakeCommand (NetworkCommand command, string arguments = null)
	// 	{
	// 		// Note(Leo): Underscore denotes the position of command bytes
	// 		byte [] bytes = Encoding.ASCII.GetBytes($"MORKO_{arguments}");
	// 		bytes[commandByteIndex] = (byte)command;
	// 		return bytes;
	// 	}

	// 	public static string GetArguments(byte [] data)
	// 	{
	// 		int count = data.Length - 6;
	// 		var arguments = Encoding.ASCII.GetString(data, 6, count);
	// 		return arguments;
	// 	}
	// }

}