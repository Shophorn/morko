public interface IAudioUIControllable
{
	void SetMasterVolume(float value);
	void SetMusicVolume(float value);
	void SetCharacterVolume(float value);
	void SetSfxVolume(float value);
}

public interface INetUIControllable
{
	void RequestJoin(JoinInfo joinInfo);
	void OnPlayerReady();
	void CreateRoom(RoomCreateInfo createInfo);
	void LeaveRoom();
	void StartGame();
}

public interface IAppUIControllable
{
	void ExitMatch();
	void Quit();
}