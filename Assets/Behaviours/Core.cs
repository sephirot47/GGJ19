using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Core : MonoBehaviour
{
    private TruckTrigger truckTrigger;
    Dictionary<PlayerId, int> scores;

    void Start()
    {
        scores = new Dictionary<PlayerId, int>();
        scores.Add(PlayerId.CHILD, 0);
        scores.Add(PlayerId.DAD, 0);
        scores.Add(PlayerId.MUM, 0);
    }
    
    void Update()
    { 
    }

    public void AddToScore(PlayerId playerId, int amount)
    {
        int prevScore = scores[playerId];
        scores.Add(playerId, prevScore + amount);
        Debug.Log(scores);
    }
}
