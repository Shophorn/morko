/*
Leo Tamminen
shophorn@protonmail.com

This class defines conversion to and from network packages in Morko Network Library.
All conversions should happen here, so it is easy to change and locata both when 
either one needs to change.
*/

using System;

namespace Morko.Network
{
	public static class ProtocolFormat
	{
		static byte [] Encode(string text) => System.Text.Encoding.ASCII.GetBytes(text);

		private const string appId = "MORKO";
		private static readonly byte [] idBytes = Encode(appId);
		private const int idBytesCount 		= 5;
		private const int commandBytesCount = 1;
		private const int commandByteIndex 	= 5;

		public static byte [] MakeCommand<T>(T arguments, byte [] package = null) 
			where T : struct, INetworkCommandArgs
		{
			char commandSymbol = (char)arguments.Command;
			var header = Encode($"{appId}{commandSymbol}");
			var argsData = arguments.ToBinary();

			int headerSize = header.Length;
			int argsSize = argsData.Length;
			int packageSize = package != null ? package.Length : 0;

			var data = new byte[headerSize + argsSize + packageSize];

			Buffer.BlockCopy(header, 0, data, 0, header.Length);
			Buffer.BlockCopy(argsData, 0, data, header.Length, argsData.Length);

			if (package != null)
			{
				Buffer.BlockCopy(package, 0, data, headerSize + argsSize, packageSize);
			}
			
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