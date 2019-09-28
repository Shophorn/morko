using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UInt8 = System.Byte;

namespace Morko.Network
{
	[System.Flags]
	enum AvatarFlags
	{
		Attack = 1 << 0,
		Parry = 1 << 1,
	}

	public class AvatarPackage
	{
		public UInt8 id;
		public Vector3 position;
		public Quaternion rotation;
		public Vector3 velocity;
		AvatarFlags flags;
	}

	public class IPv4AddressFormat
	{
		byte [] parts = new byte[4];

		public static IPv4AddressFormat FromString(string address)
		{
			int length = address.Length;
			if (length > 15)
			{
				throw;
			}

			var parts = address.Split('.');
			if (parts.Length != 4)
			{
				throw;
			}

			IPv4AddressFormat format = new IPv4AddressFormat();
			for (int i = 0; i < 4; i++)
			{
				format.parts[i] = byte.Parse(parts[i]);
			}
			return format;
		}

		string ToString()
		{
			string representation = $"{parts[0]}.{parts[1]}.{parts[2]}.{parts[3]}";
		}

		static IPv4AddressFormat [] GetAdressesInSubnet(IPv4AddressFormat subnetMask)
		{

		}
	}

	public class Client : MonoBehaviour
	{
		LocalController localController = null;
		NetworkController [] networkControllers;

		private static bool HasConnection()
		{
			bool result = NetworkInterface.GetIsNetworkAvailable();
			return result;
		}

		public static Client CreateLanClient()
		{
			Debug.Log($"Host name: {Dns.GetHostName()}");
			var hostName = Dns.GetHostName();
			var hostEntry = Dns.GetHostEntry(hostName);
			
			string ipAddress = null;
			string subnetMask = null;
			/*
			foreach (var address in hostEntry.AddressList)
			{
				if(address.AddressFamily == AddressFamily.InterNetwork)
				// if(address.AddressFamily == AddressFamily.InterNetworkV6)
				{
					ipAddress = address.ToString();

				}

			}
			*/
			List<string> masks = new List<string>();

			foreach(NetworkInterface adapter in NetworkInterface.GetAllNetworkInterfaces())
			{
				if(adapter.OperationalStatus != OperationalStatus.Up)
					continue;
				//print(adapter.Description);
				// print("status: " + adapter.OperationalStatus);
				foreach(UnicastIPAddressInformation unicastInfo in adapter.GetIPProperties().UnicastAddresses)
				{
					if(unicastInfo.Address.AddressFamily == AddressFamily.InterNetwork)
					{
						if(unicastInfo.Address.ToString() != "127.0.0.1")
						{
							subnetMask =unicastInfo.IPv4Mask.ToString();
							ipAddress = unicastInfo.Address.ToString();
						}
						
					}
				}
			}
			
			Debug.Log($"Ip address {ipAddress} Subnet Mask {subnetMask}");

			return null;
		}



		void StartGame()
		{
			// Create characters and controllers
			
		}

		void Update()
		{
			localController.Update();

			bool weHaveNewPackage = false;
			AvatarPackage package = null;

			foreach(var controller in networkControllers)
			{
				if (weHaveNewPackage)
				{
					controller.Update(package);
				}
				else
				{
					controller.Update();
				}

			}
		}
	}
}