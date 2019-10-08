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

	private static string GetServerName()
	{
		WriteLine("Enter server name to begin broadcasting:");
		string serverName = GetInput();
		return serverName;
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

	public static void Main()
	{
		Server server = null;
		string serverName = null;

		bool doLoop = true;
		while(doLoop)
		{
			WriteLine("Enter action [create, start bc, stop bc, start game, stop game, status, exit]");
			string input = GetInput();

			switch(input)
			{
				case "create":
					serverName = GetServerName();
					server = StartServer(serverName);
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
		server.Close();
		WriteLine ($"Stopped server '{serverName}'");

		if (IsInOwnConsole())
		{
			WriteLine("Press ENTER to exit");
			Console.ReadKey();
		}
	}
}