using System;

public partial class GameManager
{
	public static event Action OnLoadingStartLocal; 
	public static event Action OnGameStartLocal;
	public static event Action OnGameEndLocal;
	public static event Action OnReturnToMenuLocal;
}