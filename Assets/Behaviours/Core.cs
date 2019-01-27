using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class Core : MonoBehaviour
{
    public enum State
    {
        INTRO,
        SHUFFLING,
        SHOW_CONTROLS,
        PLAYING,
        ENDING
    };

    public TextMeshProUGUI remainingPlayTimeText;
    public TruckTrigger truckTrigger;
    public AnimationCurve truckLeaveSpeed;
    Dictionary<PlayerId, int> scores;
    private float beginPlayTimeSecs;
    public float roundTime;

    private State state  = State.INTRO;
    private float shuffleTimeBegin;
    public float maxShuffleTime;
    float shuffleTimeCounter = 0.0f;

    private List<GameObject> objectsInTruck;
    private float showControlsTimeBegin;
    public float maxShowControlsTime;
    private float endingTimeBegin;

    public static Core core;
    public GameObject mum, dad, child;
    public static PlayerId winnerPlayerId = PlayerId.CHILD;
    public static Color dadColor = Color.black;
    public static Color mumColor = Color.black;
    public static Color childColor = Color.black;

    [FMODUnity.EventRef]
    public string FMODGeneralEventRef;
    FMOD.Studio.EventInstance FMODGeneralEvent;

    void Start()
    {
        Core.core = this;

        objectsInTruck = new List<GameObject>();
        state = State.INTRO;
        
        FMODGeneralEvent = FMODUnity.RuntimeManager.CreateInstance(FMODGeneralEventRef);
        // FMODGeneralEvent.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(gameObject)); 
        scores = new Dictionary<PlayerId, int>();
        scores[PlayerId.CHILD] = 0;
        scores[PlayerId.DAD] = 0;
        scores[PlayerId.MUM] = 0;
    }

    void UpdateCountDownText(float remainingTimeSecs)
    {
        int seconds = (int)(remainingTimeSecs) % 60;
        int minutes = (int)(remainingTimeSecs / 60) % 60;
        string secondsStr = (seconds >= 10 ? "" : "0") + (seconds.ToString());
        string minutesStr = (minutes >= 10 ? "" : "0") + (minutes.ToString());
        remainingPlayTimeText.SetText(minutesStr + ":" + secondsStr);

        if (remainingTimeSecs < 10)
        {
            remainingPlayTimeText.faceColor = Color.red;
            remainingPlayTimeText.outlineColor = Color.white;
            remainingPlayTimeText.fontSize = 80;
        }
        else if (remainingTimeSecs < 30)
        {
            remainingPlayTimeText.faceColor = Color.yellow;
            remainingPlayTimeText.fontSize = 60;
        }

    }
    
    void Update()
    {
        if (state == State.INTRO)
        {
            UpdateCountDownText(roundTime);
            FMODGeneralEvent.setParameterValue("Start", 1.0f);
            FMODGeneralEvent.start();
        }
        else if (state == State.PLAYING)
        {
            float passedTimeSecs = (Time.time - beginPlayTimeSecs);
            float remainingTimeSecs = (roundTime - passedTimeSecs);
            UpdateCountDownText(remainingTimeSecs);

            float firstPlayerAudioWeight  = 0.0f;
            float secondPlayerAudioWeight = 0.0f;
            float thirdPlayerAudioWeight  = 0.0f;
            firstPlayerAudioWeight += (scores[PlayerId.DAD] > scores[PlayerId.CHILD]) ? 1.0f : 0.0f;
            firstPlayerAudioWeight += (scores[PlayerId.DAD] > scores[PlayerId.MUM]) ? 1.0f : 0.0f;
            secondPlayerAudioWeight += (scores[PlayerId.MUM] > scores[PlayerId.DAD]) ? 1.0f : 0.0f;
            secondPlayerAudioWeight += (scores[PlayerId.MUM] > scores[PlayerId.CHILD]) ? 1.0f : 0.0f;
            thirdPlayerAudioWeight += (scores[PlayerId.CHILD] > scores[PlayerId.DAD]) ? 1.0f : 0.0f;
            thirdPlayerAudioWeight += (scores[PlayerId.CHILD] > scores[PlayerId.MUM]) ? 1.0f : 0.0f;

            // Debug.Log("DAD: " + scores[PlayerId.DAD]);
            // Debug.Log("MUM: " + scores[PlayerId.MUM]);
            // Debug.Log("CHILD: " + scores[PlayerId.CHILD]);
            // Debug.Log("-----------");

            FMODGeneralEvent.setParameterValue("Player1", firstPlayerAudioWeight);
            FMODGeneralEvent.setParameterValue("Player2", secondPlayerAudioWeight);
            FMODGeneralEvent.setParameterValue("Player3", thirdPlayerAudioWeight);
            FMODGeneralEvent.setParameterValue("Start",   1.0f);

            if (remainingTimeSecs <= 0)
            {
                state = State.ENDING;
                endingTimeBegin = Time.time;
                FindObjectOfType<TruckAnimationController>().GetComponentInChildren<Animation>().Play("OpenRear");
            }
        }
        else if (state == State.SHUFFLING)
        {
            float shuffleTime = (Time.time - shuffleTimeBegin);
            float shuffleTimeNorm = shuffleTime / maxShuffleTime;
            float shuffleTimeLimitNow = Mathf.Clamp((shuffleTimeNorm) * 1.0f, 0.1f, 1.0f);

            if (shuffleTimeNorm < 1.0f)
            {
                shuffleTimeCounter += Time.deltaTime;
                if (shuffleTimeCounter >= shuffleTimeLimitNow)
                {
                    Color[] generatedColors = { Color.black, Color.black, Color.black };
                    GameObject[] characters = { mum, dad, child };
                    for (int i = 0; i < characters.Length; ++i)
                    {
                        GameObject character = characters[i];
                        
                        bool keepDoing;
                        Color characterColor = new Color(0,0,0,0);
                        Color[] characterColors = { Color.blue, Color.red, Color.green, new Color(1.0f, 0.0f, 1.0f),
                                                    new Color(1.0f, 0.5f, 0.0f), Color.black};
                        do
                        {
                            keepDoing = false;
                            characterColor = characterColors[Random.Range(0, characterColors.Length)];
                            foreach (Color otherColor in generatedColors)
                            {
                                if (otherColor == characterColor)
                                {
                                    keepDoing = true;
                                }
                            }
                        }
                        while (keepDoing);

                        generatedColors[i] = characterColor;
                        character.GetComponent<Player>().SetPlayerColor(characterColor);
                    }
                    Core.mumColor = generatedColors[0];
                    Core.dadColor = generatedColors[1];
                    Core.childColor = generatedColors[2];
                    shuffleTimeCounter = 0.0f;
                }
            }
        }
        else if (state == State.SHOW_CONTROLS)
        {
            float showControlsTime = (Time.time - showControlsTimeBegin);
            if (showControlsTime > maxShowControlsTime)
            {
            }
        }
        else if (state == State.ENDING)
        {
            float endingTime = (Time.time - endingTimeBegin);
            if (endingTime > 6.0f)
            {
                int maxScore = 0;
                foreach (KeyValuePair<PlayerId, int> pair in scores)
                {
                    if (pair.Value > maxScore)
                    {
                        maxScore = pair.Value;
                        Core.winnerPlayerId = pair.Key;
                    }
                }
                SceneManager.LoadScene("Win");
            }
        }
    }

    public void AddObjectInTruck(GameObject obj)
    {
        objectsInTruck.Add(obj);
    }

    public void RemoveObjectInTruck(GameObject obj)
    {
        objectsInTruck.Remove(obj);
    }

    public State GetState()
    {
        return state;
    }

    public void AddToScore(PlayerId playerId, int amount)
    {
        int prevScore = scores[playerId];
        scores[playerId] = (prevScore + amount);
    }

    public void StartShuffle()
    {
        state = State.SHUFFLING;
        shuffleTimeBegin = Time.time;
    }

    public void StartShowControls()
    {
        state = State.SHOW_CONTROLS;
        showControlsTimeBegin = Time.time;
    }

    public void StartPlaying()
    {
        state = State.PLAYING;
        beginPlayTimeSecs = Time.time;
    }
}
