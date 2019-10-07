using System.Text;

using static Morko.Logging.Logger;

namespace Morko.Network
{
	public static class ProtocolFormat
	{
		static byte [] Encode(string text) => Encoding.ASCII.GetBytes(text);
		static string Decode(byte [] data) => Encoding.ASCII.GetString(data);
		static string Decode(byte [] data, int firstIndex)
			=> Encoding.ASCII.GetString(data, firstIndex, data.Length - firstIndex);

		// public static byte [] ServerBroadcast(string serverName)
		// {
		// 	var data = Encode($"MORKO server {serverName}");
		// 	return data;
		// }

		// public static byte [] PlayerResponse(string playerName)
		// {
		// 	var data = Encode($"MORKO playerJoin {playerName}");
		// 	return data;
		// }

		// public static bool TryGetServerName (byte [] data, out string serverName)
		// {
		// 	var wordArray = Decode(data).Split(new char [] {' '}, 3);

		// 	bool isServerNameMessage =
		// 		wordArray.Length == 3 
		// 		&& wordArray[0] == "MORKO"
		// 		&& wordArray[1] == "server";

		// 	serverName = isServerNameMessage ? wordArray[2] : "";

		// 	return isServerNameMessage;
		// }

		// public static bool TryGetPlayerName (byte [] data, out string playerName)
		// {
		// 	var wordArray = Decode(data).Split(new char [] {' '}, 3);

		// 	bool isPlayerNameMessage =
		// 		wordArray.Length == 3
		// 		&& wordArray[0] == "MORKO"
		// 		&& wordArray[1] == "playerJoin";

		// 	playerName = isPlayerNameMessage ? wordArray[2] : "";

		// 	return isPlayerNameMessage;
		// }

		private const string appId = "MORKO";
		private static readonly byte [] idBytes = Encode(appId);
		private const int idBytesCount 		= 5;
		private const int commandBytesCount = 1;
		private const int commandByteIndex 	= 5;

		public static byte [] MakeCommand(NetworkCommand command, string arguments = null)
		{
			char commandSymbol = (char)command;
			var data = Encode($"{appId}{commandSymbol}{arguments}");
			Log(Decode(data));
			return data;
		}

		public static bool TryParseCommand(	byte [] data, 
											out NetworkCommand commmand,
											out string arguments)
		{
			/* Note(Leo): data is valid command if it:
				- 	starts with 'MORKO'
				- 	has the following byte being valid NetworkCommmand value, i.e. less than
					NetworkCommand.Undefined
				-	this means to conclusion that data must be atleast 6 bytes long
			*/

			bool isCommand = data.Length >= (idBytesCount + commandBytesCount);

			for (int byteIndex = 0; byteIndex < idBytesCount && isCommand; byteIndex++)
			{
				isCommand = isCommand && (data[byteIndex] == idBytes[byteIndex]);
			}
			isCommand = isCommand && (data[commandByteIndex] < (byte)NetworkCommand.Undefined);

			if (isCommand)
			{
				commmand = (NetworkCommand)data[commandByteIndex];
				arguments = Decode(data, idBytesCount + commandBytesCount);
			}
			else
			{
				commmand = NetworkCommand.Undefined;
				arguments = null;
			}

			return isCommand; 	
		}
	}
}