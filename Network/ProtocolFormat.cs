using System;
using System.Text;

namespace Morko.Network
{
	public static class ProtocolFormat
	{
		static byte [] Encode(string text) => Encoding.ASCII.GetBytes(text);
		static string Decode(byte [] data) => Encoding.ASCII.GetString(data);
		static string Decode(byte [] data, int firstIndex)
			=> Encoding.ASCII.GetString(data, firstIndex, data.Length - firstIndex);

		private const string appId = "MORKO";
		private static readonly byte [] idBytes = Encode(appId);
		private const int idBytesCount 		= 5;
		private const int commandBytesCount = 1;
		private const int commandByteIndex 	= 5;

		public static byte [] MakeCommand(NetworkCommand command, string arguments = null)
		{
			char commandSymbol = (char)command;
			var data = Encode($"{appId}{commandSymbol}{arguments}");
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