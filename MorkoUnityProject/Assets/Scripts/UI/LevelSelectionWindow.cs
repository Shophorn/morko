using UnityEngine;
using UnityEngine.UI;

public class LevelSelectionWindow : MonoBehaviour
{
    private Text playerText;
    private Text timerText;

    private Button playerMinus;
    private Button playerPlus;
    private Button timerMinus;
    private Button timerPlus;

    private int maxPlayers, minPlayers;

    private void Start()
    {
        playerText = GameObject.Find("Player Amount Value").GetComponent<Text>();
        timerText = GameObject.Find("Timer Value").GetComponent<Text>();
        playerMinus = GameObject.Find("Player Amount Minus").GetComponent<Button>();
        playerPlus = GameObject.Find("Player Amount Plus").GetComponent<Button>();
        timerPlus = GameObject.Find("Timer Plus").GetComponent<Button>();
        timerMinus = GameObject.Find("Timer Minus").GetComponent<Button>();

        maxPlayers = 4;
        minPlayers = 1;
    }

    public void ChangePlayerAmount(int amount)
    {
        int currentValue = int.Parse(playerText.text);
        if((amount < 0 && currentValue > 1) || (amount > 0 && currentValue < 4))
            currentValue += amount;

        playerText.text = currentValue.ToString();
    }

    public void ChangeTimerAmount(int amount)
    {
        int currentValue = int.Parse(timerText.text);

        if((amount < 0 && currentValue > 1) || (amount > 0 && currentValue < 10))
            currentValue += amount;

        timerText.text = currentValue.ToString();
    }
}
