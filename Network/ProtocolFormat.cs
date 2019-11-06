/*
Leo Tamminen
shophorn@protonmail.com

This class defines conversion to and from network packages in Morko Network Library.
All conversions should happen here, so it is easy to change and locata both when 
either one needs to change.
*/

using System;
using System.Runtime.InteropServices;

using Morko.Logging;

namespace Morko.Network
{
	public static class ProtocolFormat
	{
		// CONTINUE FROM HERE:
		//  - make CommandWriter and CommandReader classes that take care of counting sent
		//  and received messages.

		public static byte [] Encode(string text) => System.Text.Encoding.ASCII.GetBytes(text);

		public const string appId = "MORKO";
		public static readonly byte [] idBytes = Encode(appId);

		public const int idByteIndex 			= 0;
		public const int idBytesCount 			= 5; // appId.Length;
		public const int commandByteIndex 		= idByteIndex + idBytesCount;
		public const int commandBytesCount 		= 1;
		public const int cmdCounterBytesIndex	= commandByteIndex + commandBytesCount;
		public const int cmdCounterBytesCount 	= sizeof(int);

		public const int headerSize = cmdCounterBytesIndex + cmdCounterBytesCount;

		public static byte [] MakeCommand<T>(T arguments, byte [] package = null) 
			where T : struct, INetworkCommandArgs
		{
			char commandSymbol = (char)arguments.Command;
			var header = Encode($"{appId}{commandSymbol}____");
			
			int commandCounter = 2754;
			Buffer.BlockCopy(BitConverter.GetBytes(commandCounter), 0, header, cmdCounterBytesIndex, cmdCounterBytesCount);


			var argsData = arguments.ToBinary();

			int argsSize = argsData.Length;
			int packageSize = package != null ? package.Length : 0;

			var data = new byte[headerSize + argsSize + packageSize];

			Buffer.BlockCopy(header, 0, data, 0, headerSize);
			Buffer.BlockCopy(argsData, 0, data, headerSize, argsSize);

			if (package != null)
			{
				Buffer.BlockCopy(package, 0, data, headerSize + argsSize, packageSize);
			}
			
			return data;
		}

		public static bool TryParseCommand(	byte [] input,
											out NetworkCommand command,
											out byte [] data)
		{
			/* Note(Leo): input is valid command if it:
				- 	starts with 'MORKO'
				- 	has the following byte being valid NetworkCommmand value, i.e. less than
					NetworkCommand.Undefined
				- 	has the following 4 bytes describe command counter, that tells the order messages were sent
				-	this means to conclusion that input must be atleast 6 bytes long
			*/
			bool isCommand = input.Length >= (idBytesCount + commandBytesCount);
			for (int byteIndex = 0; byteIndex < idBytesCount && isCommand; byteIndex++)
			{
				isCommand = isCommand && (input[byteIndex] == idBytes[byteIndex]);
			}

			byte commandByte = input[commandByteIndex];
			int commandCounter = BitConverter.ToInt32(input, cmdCounterBytesIndex);

			Logger.Log($"Got data, command counter value = {commandCounter}");

			isCommand = isCommand && (input[commandByteIndex] < (byte)NetworkCommand.Undefined);

			// command = GetCommand(input);
			// if (command != NetworkCommand.Undefined)
			// {

			if (isCommand)
			{
				command = (NetworkCommand)commandByte;

				int dataBytesCount = input.Length - headerSize;
				data = new byte[dataBytesCount];
				Buffer.BlockCopy(input, headerSize, data, 0, dataBytesCount);
				
				return true;
			}
			else
			{
				command = NetworkCommand.Undefined;
				data = null;
				return false;
			}
		}

		private static int MakeKey(int playerId, int counter)
		{
			int key = (playerId << 24) | (0x00FFFFFF & counter);
			return key;
		}

		public static byte [] MakeTcpMessage<T>(T arguments)
			where T : struct, INetworkCommandArgs
		{
			/*
			1)	5 bytes:	"MORKO", magic word.
			2)	2 bytes:	length of arguments structure, NOT including header.
			3)	1 byte:		instruction byte.
			
			4)	n bytes:	arguments structure, n is equal to length above.
			*/

			byte[] argumentBytes = arguments.ToBinary();
			ushort argumentLength = (ushort)argumentBytes.Length;

			var data = new byte [8 + argumentLength];
			int dataIndex = 0;

			// 1)
			for (int idIndex = 0; idIndex < 5; idIndex++, dataIndex++)
				data[dataIndex] = idBytes[idIndex];

			// 2)
			byte[] lengthBytes = BitConverter.GetBytes(argumentLength);
			for (int lengthIndex = 0; lengthIndex < 2; lengthIndex++, dataIndex++)
				data[dataIndex] = lengthBytes[lengthIndex];

			// 3)
			data[dataIndex] = (byte)arguments.Command;
			dataIndex++;

			// 4)
			for (int argumentIndex = 0; argumentIndex < argumentLength; argumentIndex++, dataIndex++)
				data[dataIndex] = argumentBytes[argumentIndex];

			return data;
		}
	}
}