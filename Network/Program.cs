/*
Leo Tamminen
shophorn@protonmail.com

Test standalone console app for MorkoNetwork server
*/

using System;
using System.Runtime.InteropServices;
using System.Threading;
using Morko.Network;

using static System.Console;

class Program
{
	[DllImport("kernel32.dll")]
	static extern IntPtr GetConsoleWindow();

	[DllImport("kernel32.dll")]
	static extern IntPtr GetCurrentProcessId();

	[DllImport("user32.dll")]
	static extern int GetWindowThreadProcessId(IntPtr hWnd, ref IntPtr ProcessId);

	private static bool IsInOwnConsole()
	{
		IntPtr hConsole = GetConsoleWindow();
		IntPtr hProcessId = IntPtr.Zero;
		GetWindowThreadProcessId(hConsole, ref hProcessId);

		bool result = GetCurrentProcessId().Equals(hProcessId);
		return result;
	}

	private static string GetInput()
	{
		Write ("> ");
		var input = ReadLine();
		return input;
	}

	private static Server StartServer(string serverName)
	{
		var serverInfo = new ServerInfo
		{
			serverName = serverName,
			broadcastPort = 11000
		};
		Server server = Server.Create(serverInfo);

		WriteLine($"Starting server '{serverName}'.");

		return server;
	}

	private static void PrintInstructions()
	{
		string format = "\t{0,12}\t{1,20}\t{2}";
		string info = 
			string.Format(format,"create", "<server name>", "Create new server with a name\n")
			+ string.Format(format,"start bc", "-", "Start broadcasting\n")
			+ string.Format(format,"stop bc", "-", "Stop broadcasting\n")
			+ string.Format(format,"start game", "-", "Start multicasting update to players\n")
			+ string.Format(format,"stop game", "-", "Stop multicasting update to players\n")
			+ string.Format(format,"status", "-", "Print current server status\n")
			+ string.Format(format,"exit", "-", "Exit server. Remember to stop things first\n");
	
		Console.WriteLine(info);
	}

	public static void Main()
	{
		Server server = null;
		string serverName = null;

		bool doLoop = true;
		while(doLoop)
		{
			WriteLine("Enter action ('help' for help");
			string input = GetInput();
			string args = null;

			var parts = input.Split(new char [] {' '}, 2);
			if (parts.Length == 2 && parts[0] == "create")
			{
				input = parts[0];
				args = parts[1];
			}

			switch(input)
			{
				case "help":
					PrintInstructions();
					break;

				case "create":
					if (args != null)
					{
						serverName = args;
						server = StartServer(serverName);
					}
					else
					{
						WriteLine("No server name found!");
					}
					break;

				case "start bc":
					server.StartBroadcasting();
					break;

				case "stop bc":
					server.StopBroadcasting();
					break;

				case "start game":
					server.StartGame();
					break;

				case "stop game":
					server.StopGame();
					break;

				case "status":
					WriteLine($"Server status:\n\tplayers: {server.PlayerCount}");
					break;

				case "exit":
					doLoop = false;
					break;

				default:
					break;
			}
		}

		// ---- CLEANUP ----
		server?.Close();
		WriteLine ($"Stopped server '{serverName}'");

		if (IsInOwnConsole())
		{
			WriteLine("Press ENTER to exit");
			Console.ReadKey();
		}
	}
}