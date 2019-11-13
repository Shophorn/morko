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
		public ClientController controller;

		void IThreadRunner.Run()
		{
			while(true)
			{
				if (controller.sender != null)
				{
					var updateArgs = new ClientGameUpdateArgs
					{
						playerId = controller.ClientId
					};

					byte[] updatePackage = controller.sender.GetPackageToSend().ToBinary();
					byte[] data = ProtocolFormat.MakeCommand (updateArgs, updatePackage);

					controller.udpClient.Send(data, data.Length, controller.joinedServer.endPoint);
				}
				Thread.Sleep(controller.networkSendUpdateInterval);
			}
		}

		void IThreadRunner.CleanUp() {}
	}
}