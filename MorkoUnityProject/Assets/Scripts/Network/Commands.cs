using System.Text;

namespace Morko.Network
{
	enum NetworkCommand
	{
		Invalid = 0,

		// Note(Leo): These start from 1 and go up
		GetServers,
		IntroduceServer,
		JoinRequest,
		JoinConfirm,
		JoinComplete,
		StartGame,

		// Note(Leo): Undefined is always of highest value
		Undefined
	}

	static class Commands
	{
		private const string idString = "MORKO";
		private const int commandByteIndex = 5;

		private static readonly byte [] idBytes = Encoding.ASCII.GetBytes("MORKO");
		private const int idBytesCount = 5;

		public static bool IsCommand(byte [] data)
		{
			bool isCommand = data.Length > commandByteIndex;
			for (int i = 0; i < idBytesCount && isCommand; i++)
			{
				isCommand = isCommand && (idBytes[i] == data[i]);
			}
			return isCommand;
		}

		public static NetworkCommand GetCommand(byte [] data)
		{
			NetworkCommand command = (NetworkCommand)data[commandByteIndex];
			if (command < NetworkCommand.Undefined)
				return command;
			else
				return NetworkCommand.Undefined;
		} 

		public static byte [] MakeCommand (NetworkCommand command, string arguments = null)
		{
			byte [] bytes = Encoding.ASCII.GetBytes($"MORKO_{arguments}");
			bytes[commandByteIndex] = (byte)command;
			return bytes;
		}

		public static string GetArguments(byte [] data)
		{
			int count = data.Length - 6;
			var arguments = Encoding.ASCII.GetString(data, 6, count);
			return arguments;
		}
	}
}