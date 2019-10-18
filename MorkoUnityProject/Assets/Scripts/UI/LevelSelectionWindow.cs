using UnityEngine;
using UnityEngine.UI;

namespace Morko
{
	public class LevelSelectionWindow : MonoBehaviour
	{
		private Text playerText;
		private Text timerText;

		private Button playerMinus;
		private Button playerPlus;
		private Button timerMinus;
		private Button timerPlus;

		private int maxPlayers, minPlayers;
		private int maxMinutes, minMinutes;
		private int maxSeconds, minSeconds;

		private void Start()
		{
			playerText = GameObject.Find("Player Amount").GetComponent<Text>();
			timerText = GameObject.Find("Timer Value").GetComponent<Text>();
			playerMinus = GameObject.Find("Player Minus").GetComponent<Button>();
			playerPlus = GameObject.Find("Player Plus").GetComponent<Button>();
			timerPlus = GameObject.Find("Timer Plus").GetComponent<Button>();
			timerMinus = GameObject.Find("Timer Minus").GetComponent<Button>();

			maxPlayers = 10;
			minPlayers = 1;

			maxMinutes = 15;
			minMinutes = 1;
			maxSeconds = 59;
			minSeconds = 0;
		}

		public void ChangePlayerAmount(int amount)
		{
			int currentValue = int.Parse(playerText.text);

			if ((amount < 0 && currentValue > minPlayers) || (amount > 0 && currentValue < maxPlayers))
				currentValue += amount;

			playerText.text = currentValue.ToString();
		}

		public void ChangeTimerAmount(int amount)
		{
			string[] timer = timerText.text.Split(':');
			int minutes = int.Parse(timer[0]);
			int seconds = int.Parse(timer[1]);

			seconds += amount;

			if (seconds < minSeconds)
			{
				minutes--;
				seconds = 60 + seconds;
			}
			else if (seconds > maxSeconds)
			{
				minutes++;
				seconds = 60 - seconds;
				if (seconds < 0)
					seconds *= -1;
			}

			if (minutes >= maxMinutes && seconds > 0)
			{
				minutes = maxMinutes;
				seconds = 0;
			}
			else if (minutes < minMinutes)
			{
				minutes = minMinutes;
				seconds = 0;
			}
			timerText.text = minutes.ToString("00") + ":" + seconds.ToString("00");
		}
	}
}