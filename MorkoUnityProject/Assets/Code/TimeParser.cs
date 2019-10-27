public static class TimeFormat
{
	public static string ToTimeFormat(int timeInSeconds)
	{
		int minutes = timeInSeconds / 60;
		int seconds = timeInSeconds % 60;
		return $"{minutes.ToString("00")}:{seconds.ToString("00")}";
	}
}