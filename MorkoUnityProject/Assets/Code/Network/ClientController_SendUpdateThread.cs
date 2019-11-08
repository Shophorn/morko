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
		public int 			sendDelayMs;
		public UdpClient 	udpClient;
		public IPEndPoint 	endPoint;

		public ClientController controller;

		void IThreadRunner.Run()
		{
			Debug.Log($"[CLIENT]: Start SendUpdateThread, endPoint = {endPoint}");
			while(true)
			{
				if (controller.sender != null)
				{
					var updateArgs = new ClientGameUpdateArgs
					{
						playerId = controller.ClientId
					};

					Debug.Log($"[CLIENT SEND UPDATE]: {controller != null} and {controller.sender != null}");

					byte[] updatePackage = controller.sender.GetPackageToSend().ToBinary();
					byte[] data = ProtocolFormat.MakeCommand (updateArgs, updatePackage);

					Debug.Log($"[CLIENT]: Send data to server, position = {controller.sender.GetPackageToSend().position}");
					udpClient.Send(data, data.Length, endPoint);
				}
				Thread.Sleep(sendDelayMs);
			}
		}

		void IThreadRunner.CleanUp() {}
	}
}