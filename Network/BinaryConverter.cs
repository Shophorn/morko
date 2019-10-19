/*
Leo Tamminen
shophorn@protonmail.com

Extension methods for converting structures to byte arrays and back.
No guarantees are made, so only use structs where layout is explicitly
specified with 'StructLayout' attribute.
*/

using System;
using System.Runtime.InteropServices;

namespace Morko.Network
{
	public static class BinaryConverter
	{
		public unsafe static byte [] ToBinary<T>(this T value) where T : struct
		{
			int size = Marshal.SizeOf(value);
			var result = new byte [size];

			fixed(byte * resultPtr = result)
			{
				Marshal.StructureToPtr(value, (IntPtr)resultPtr, false);
			}

			return result;
		}

		public unsafe static byte[] ToBinary<T>(this T [] array) where T : struct
		{
			int itemCount = array.Length;
			if (itemCount == 0)
				return Array.Empty<byte>();

			int itemSize = Marshal.SizeOf(array[0]);
			int totalSize = itemSize * itemCount;

			var result = new byte[totalSize];

			fixed (byte * resultPtr = result)
			{
				for (int itemIndex = 0; itemIndex < itemCount; itemIndex++)
				{
					IntPtr target = (IntPtr)(resultPtr + itemSize * itemIndex);
					Marshal.StructureToPtr(	array[itemIndex], target, false);
				}
			}
			return result;
		}

		public unsafe static T ToStructure<T> (this byte[] data, int offset = 0) where T : struct
		{
			int structureSize = Marshal.SizeOf(default(T));
			if (data.Length < structureSize)
			{
				throw new ArgumentException($"Data amount is too little for {typeof(T)}. Required size is {structureSize}, actual size is {data.Length}");
			}

			T result;
			fixed(byte * source = data)
			{
				result = Marshal.PtrToStructure<T>((IntPtr)(source + offset));
			}
			return result;
		}

		public static T ToStructure<T>(this byte [] data, out byte [] leftovers) where T : struct
		{
			T result = data.ToStructure<T>();

			int structureSize 	= Marshal.SizeOf(default(T));
			int leftoverSize 	= data.Length - structureSize;
			leftovers 			= new byte[leftoverSize];

			Buffer.BlockCopy(data, structureSize, leftovers, 0, leftoverSize);

			return result;
		}

		public static T[] ToArray<T> (this byte [] data, int count) where T : struct
		{
			if (count == 0)
				return Array.Empty<T>();

			var result = new T [count];

			int itemSize = Marshal.SizeOf(result[0]);

			int offset = 0;

			for (int itemId = 0; itemId < count; itemId++)
			{
				result [itemId] = data.ToStructure<T>(offset);
				offset += itemSize;
			}

			return result;
		}

		// public static T ToStructure<T>(this byte [] data, int startIndex, out int nextIndex) where T : struct
		// {
		// 	T result = data.ToStructure<T>();

		// 	int structureSize 	= Marshal.SizeOf(default(T));
		// 	int leftoverSize 	= data.Length - structureSize;
		// 	leftovers 			= new byte[leftoverSize];

		// 	Buffer.BlockCopy(data, structureSize, leftovers, 0, leftoverSize);

		// 	return result;
		// }
	}
}