/* 
Leo Tamminen
shophorn@protonmail.com

This is a fixed size string to be used in network structures,
where we would like to have consistency for easier and speedier
content parsing.
*/

using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Morko.Network
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct NetworkName
	{
		private const int _maxLength = 32;
		private unsafe fixed byte _data [_maxLength];
		private int _length;

		public unsafe static implicit operator string(NetworkName name)
		{
			int count = name._length;
			byte [] byteArray = new byte[count];
			fixed(byte * dstBytes = byteArray)
			{
				Buffer.MemoryCopy(name._data, dstBytes, count, count);
			}
			return Encoding.ASCII.GetString(byteArray);
		}

		public unsafe static implicit operator NetworkName (string text)
		{
			NetworkName result = new NetworkName();
			byte [] byteArray = Encoding.ASCII.GetBytes(text);
			int count = byteArray.Length < _maxLength ?
						byteArray.Length :
						_maxLength;
	
			result._length = count;
			fixed(byte * srcBytes = byteArray)
			{
				Buffer.MemoryCopy(srcBytes, result._data, count, count);
			}
			return result;
		}

		public override string ToString()
		{
			return this;	
		}
	}
}