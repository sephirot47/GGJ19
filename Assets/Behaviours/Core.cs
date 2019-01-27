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

    public Material roofMaterial;
    public MeshRenderer truckMeshRend;

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

        // if (!FMODStarted)
        {
            FMODGeneralEvent = FMODUnity.RuntimeManager.CreateInstance(FMODGeneralEventRef);
            FMODGeneralEvent.setParameterValue("Player1", 0.0f);
            FMODGeneralEvent.setParameterValue("Player2", 0.0f);
            FMODGeneralEvent.setParameterValue("Player3", 0.0f);
            FMODGeneralEvent.setParameterValue("Empat", 1.0f);
            FMODGeneralEvent.setParameterValue("Start", 0.0f);
            FMODGeneralEvent.start();
        }

        roofMaterial.SetColor("_Color", Color.black);

        scores = new Dictionary<PlayerId, int>();
        scores[PlayerId.CHILD] = 0;
        scores[PlayerId.DAD] = 0;
        scores[PlayerId.MUM] = 0;

        remainingPlayTimeText.gameObject.active = false;
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
            remainingPlayTimeText.fontSize = 100;
        }
        else if (remainingTimeSecs < 30)
        {
            remainingPlayTimeText.faceColor = Color.yellow;
            remainingPlayTimeText.fontSize = 80;
        }

    }
    
    void Update()
    {
        if (state == State.INTRO)
        {
            UpdateCountDownText(roundTime);
            FMODGeneralEvent.setParameterValue("Start", 0.0f);
        }
        else if (state == State.PLAYING)
        {
            FMODGeneralEvent.setParameterValue("Start", 1.0f);
            remainingPlayTimeText.gameObject.active = true;

            float passedTimeSecs = (Time.time - beginPlayTimeSecs);
            float remainingTimeSecs = (roundTime - passedTimeSecs);
            UpdateCountDownText(remainingTimeSecs);

            {
                float dadPlayerWinWeight = 0.0f;
                float mumPlayerWinWeight = 0.0f;
                float childPlayerWinWeight = 0.0f;
                dadPlayerWinWeight += (scores[PlayerId.DAD] > scores[PlayerId.CHILD]) ? 1.0f : 0.0f;
                dadPlayerWinWeight += (scores[PlayerId.DAD] > scores[PlayerId.MUM]) ? 1.0f : 0.0f;
                mumPlayerWinWeight += (scores[PlayerId.MUM] > scores[PlayerId.DAD]) ? 1.0f : 0.0f;
                mumPlayerWinWeight += (scores[PlayerId.MUM] > scores[PlayerId.CHILD]) ? 1.0f : 0.0f;
                childPlayerWinWeight += (scores[PlayerId.CHILD] > scores[PlayerId.DAD]) ? 1.0f : 0.0f;
                childPlayerWinWeight += (scores[PlayerId.CHILD] > scores[PlayerId.MUM]) ? 1.0f : 0.0f;

                if (dadPlayerWinWeight == mumPlayerWinWeight &&
                    mumPlayerWinWeight == childPlayerWinWeight)
                {
                    FMODGeneralEvent.setParameterValue("Player1", 0.0f);
                    FMODGeneralEvent.setParameterValue("Player2", 0.0f);
                    FMODGeneralEvent.setParameterValue("Player3", 0.0f);
                    FMODGeneralEvent.setParameterValue("Empat", 1.0f);
                    FMODGeneralEvent.setParameterValue("Start", 1.0f);
                }
                else
                {
                    FMODGeneralEvent.setParameterValue("Player1", dadPlayerWinWeight);
                    FMODGeneralEvent.setParameterValue("Player2", mumPlayerWinWeight);
                    FMODGeneralEvent.setParameterValue("Player3", childPlayerWinWeight);
                    FMODGeneralEvent.setParameterValue("Empat", 0.0f);
                }
                FMODGeneralEvent.setParameterValue("Start", 1.0f);
            }

            {
                Color truckColor = Color.white;
                Color roofColor = Color.black;
                float totalTruckWeight = (scores[PlayerId.DAD] + scores[PlayerId.CHILD] + scores[PlayerId.MUM]);
                if (totalTruckWeight > 0)
                {
                    truckColor = (dad.GetComponent<Player>().GetPlayerColor() * (scores[PlayerId.DAD] / totalTruckWeight));
                    truckColor += (mum.GetComponent<Player>().GetPlayerColor() * (scores[PlayerId.MUM] / totalTruckWeight));
                    truckColor += (child.GetComponent<Player>().GetPlayerColor() * (scores[PlayerId.CHILD] / totalTruckWeight));
                    roofColor = truckColor; 
                }

                List<Material> materials = new List<Material>();
                truckMeshRend.GetMaterials(materials);
                Material truckMat = materials[0];
                truckMat.SetColor("_Color", Color.Lerp(truckMat.GetColor("_Color"), truckColor, Time.deltaTime));
                roofMaterial.SetColor("_Color", Color.Lerp(roofMaterial.GetColor("_Color"), roofColor, Time.deltaTime));
            }

            if (remainingTimeSecs <= 0)
            {
                state = State.ENDING;
                endingTimeBegin = Time.time;
                FMODGeneralEvent.setParameterValue("Start", 0.0f);
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
