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
}
