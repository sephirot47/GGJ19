using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class WinManager : MonoBehaviour
{
    public TextMeshProUGUI winText;
    public GameObject mum, dad, child;
    public GameObject winnerPos, losePos0, losePos1;

    void Start()
    {
    }
    
    void Update()
    {
        winText.SetText(Core.winnerPlayerId.ToString() + " WINS!");
 
        if (Core.winnerPlayerId == PlayerId.MUM)
        {
            mum.transform.position = winnerPos.transform.position;
            dad.transform.position = losePos0.transform.position;
            child.transform.position = losePos1.transform.position;
        }
        else if (Core.winnerPlayerId == PlayerId.DAD)
        {
            dad.transform.position = winnerPos.transform.position;
            mum.transform.position = losePos0.transform.position;
            child.transform.position = losePos1.transform.position;
        }
        else
        {
            child.transform.position = winnerPos.transform.position;
            dad.transform.position = losePos0.transform.position;
            mum.transform.position = losePos1.transform.position;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            SceneManager.LoadScene("Game");
        }
    }
}
