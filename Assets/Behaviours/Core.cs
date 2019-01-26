using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class Core : MonoBehaviour
{
    public TextMeshProUGUI countdownText;
    private TruckTrigger truckTrigger;
    Dictionary<PlayerId, int> scores;
    private float beginTimeSecs;

    public static PlayerId winnerPlayerId = PlayerId.CHILD;

    void Start()
    {
        scores = new Dictionary<PlayerId, int>();
        scores[PlayerId.CHILD] = 0;
        scores[PlayerId.DAD] = 0;
        scores[PlayerId.MUM] = 0;

        beginTimeSecs = Time.time;
    }
    
    void Update()
    {
        float passedTimeSecs = (Time.time - beginTimeSecs);
        float remainingTimeSecs = (20 - passedTimeSecs);

        int seconds = (int)(remainingTimeSecs) % 60;
        int minutes = (int)(remainingTimeSecs / 60) % 60;
        string secondsStr = (seconds >= 10 ? "" : "0") + (seconds.ToString());
        string minutesStr = (minutes >= 10 ? "" : "0") + (minutes.ToString());
        countdownText.SetText(minutesStr + ":" + secondsStr);

        if (remainingTimeSecs <= 0)
        {
            int maxScore = 0;
            foreach (KeyValuePair<PlayerId, int> pair in scores)
            {
                if (pair.Value > maxScore)
                {
                    maxScore = pair.Value;
                    winnerPlayerId = pair.Key;
                }
            }

            SceneManager.LoadScene("Win");
        }
    }

    public void AddToScore(PlayerId playerId, int amount)
    {
        int prevScore = scores[playerId];
        scores[playerId] = (prevScore + amount);
        Debug.Log(scores);
    }
}
