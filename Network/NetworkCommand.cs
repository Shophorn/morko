/*
Leo Tamminen
shophorn@protonmail.com
*/

namespace Morko.Network
{
	public enum NetworkCommand : byte
	{
		/* Note(Leo): When no other values between 1 and Undefined are specified
		Undefined will have biggest and we can easily check if any value might
		be a proper NetworkCommand. */
		None = 0,

		ServerIntroduce = 1,
		ServerMulticastTest,

		PlayerJoin,

		Undefined,
	}
}