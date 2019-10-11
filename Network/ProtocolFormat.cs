/*
Leo Tamminen
shophorn@protonmail.com

This class defines conversion to and from network packages in Morko Network Library.
All conversions should happen here, so it is easy to change and locata both when 
either one needs to change.
*/

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

		public static byte [] MakeCommand(NetworkCommand command, byte [] content = null)
		{
			char commandSymbol = (char)command;
			var header = Encode($"{appId}{commandSymbol}");

			var data = new byte[header.Length + content.Length];
			Buffer.BlockCopy(header, 0, data, 0, header.Length);
			Buffer.BlockCopy(content, 0, data, header.Length, content.Length);
			
			return data;
		}

		private static NetworkCommand GetCommand(byte[] input)
		{
			/* Note(Leo): input is valid command if it:
				- 	starts with 'MORKO'
				- 	has the following byte being valid NetworkCommmand value, i.e. less than
					NetworkCommand.Undefined
				-	this means to conclusion that input must be atleast 6 bytes long
			*/
			bool isCommand = input.Length >= (idBytesCount + commandBytesCount);
			for (int byteIndex = 0; byteIndex < idBytesCount && isCommand; byteIndex++)
			{
				isCommand = isCommand && (input[byteIndex] == idBytes[byteIndex]);
			}

			byte commandByte = input[commandByteIndex];
			isCommand = isCommand && (input[commandByteIndex] < (byte)NetworkCommand.Undefined);

			return isCommand ?
				(NetworkCommand)commandByte :
				NetworkCommand.Undefined;
		}

		public static bool TryParseCommand(	byte [] input, 
											out NetworkCommand command,
											out string arguments)
		{
			command = GetCommand(input);
			if (command != NetworkCommand.Undefined)
			{
				arguments = Decode(input, idBytesCount + commandBytesCount);
				return true;
			}
			else
			{
				arguments = null;
				return false;
			}
		}

		public static bool TryParseCommand(	byte [] input,
											out NetworkCommand command,
											out byte [] data)
		{
			command = GetCommand(input);
			if (command != NetworkCommand.Undefined)
			{
				int headerLength = idBytesCount + commandBytesCount;
				int dataBytesCount = input.Length - headerLength;
				
				data = new byte[dataBytesCount];
				Buffer.BlockCopy(input, headerLength, data, 0, dataBytesCount);
				
				return true;
			}
			else
			{
				data = null;
				return false;
			}
		}
	}
}