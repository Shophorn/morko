/*
Leo Tamminen

Call site file and line attributes:
https://stackoverflow.com/questions/12556767/how-do-i-get-the-current-line-number
*/

using System;
using System.IO;

namespace Morko.Logging
{
	public class Logger
	{
		private static object threadLock = new object();

		public static void Log(string text)
		{
			var filename = "log.txt";
			var timeString = DateTime.Now.ToString("HH:mm:ss");
			var content = $"{timeString}: {text}\n";

			lock(threadLock) File.AppendAllText(filename, content);
		}
	}
}