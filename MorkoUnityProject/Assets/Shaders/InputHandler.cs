using UnityEngine;

public class InputHandler
{
	private struct InputNames
	{
		public string a;
		public string b;
	}

	private static readonly InputNames keyboard = new InputNames
	{
		a = "Hello",
		b = "Nice feature"
	};
}