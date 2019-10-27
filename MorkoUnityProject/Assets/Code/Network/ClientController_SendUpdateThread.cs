using System.Net;
using System.Net.Sockets;
using System.Threading;

using UnityEngine;

using Morko.Network;
using Morko.Threading;

public partial class ClientController
{
	private class SendUpdateThread : IThreadRunner
	{
		public Vector3 		playerPosition;
		public int 			sendDelayMs;
		public int 			clientId;
		public UdpClient 	udpClient;
		public IPEndPoint 	endPoint;

		void IThreadRunner.Run()
		{
			Debug.Log($"[CLIENT]: Start SendUpdateThread, endPoint = {endPoint}");
			while(true)
			{
				var updateArgs = new ClientGameUpdateArgs
				{
					playerId = clientId
				};

				byte [] updatePackage = new PlayerGameUpdatePackage
				{
					playerId = clientId,
					position = playerPosition
				}.ToBinary();

				byte [] data = ProtocolFormat.MakeCommand (updateArgs, updatePackage);

				Debug.Log("[CLIENT]: Send data to server");
				udpClient.Send(data, data.Length, endPoint);

				Thread.Sleep(sendDelayMs);
			}
		}

		void IThreadRunner.CleanUp() {}
	}
}