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

    public TextMeshProUGUI countdownText;
    public TruckTrigger truckTrigger;
    public AnimationCurve truckLeaveSpeed;
    Dictionary<PlayerId, int> scores;
    private float beginPlayTimeSecs;
    public float roundTime;

    private State state  = State.INTRO;
    private float shuffleTimeBegin;
    public float maxShuffleTime;
    float shuffleTimeCounter = 0.0f;

    private float showControlsTimeBegin;
    public float maxShowControlsTime;
    private float endingTimeBegin;

    public static Core core;
    public GameObject mum, dad, child;
    public static PlayerId winnerPlayerId = PlayerId.CHILD;

    void Start()
    {
        Core.core = this;

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
        countdownText.SetText(minutesStr + ":" + secondsStr);
    }
    
    void Update()
    {
        if (state == State.INTRO)
        {
            UpdateCountDownText(roundTime);
        }
        else if (state == State.PLAYING)
        {
            float passedTimeSecs = (Time.time - beginPlayTimeSecs);
            float remainingTimeSecs = (roundTime - passedTimeSecs);
            UpdateCountDownText(remainingTimeSecs);

            if (remainingTimeSecs <= 0)
            {
                state = State.ENDING;
                endingTimeBegin = Time.time;
                FindObjectOfType<TruckAnimationController>().GetComponentInChildren<Animation>().Play("Open");
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

                        int j = 0;
                        float minDistance;
                        Color characterColor;
                        do
                        {
                            minDistance = 99999.9f;
                            characterColor = Random.ColorHSV(0.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f);
                            foreach (Color otherColor in generatedColors)
                            {
                                float distance = Vector3.Distance(new Vector3(otherColor.r, otherColor.g, otherColor.b),
                                    new Vector3(characterColor.r, characterColor.g, characterColor.b));
                                if (distance < minDistance)
                                {
                                    minDistance = distance;
                                }
                            }
                        }
                        while (++j < 50 && minDistance < 1.0);
                        generatedColors[i] = characterColor;

                        foreach (SkinnedMeshRenderer mr in character.GetComponentsInChildren<SkinnedMeshRenderer>())
                        {
                            if (mr.gameObject.name == "Bottoms" || mr.gameObject.name == "Hats" ||
                                mr.gameObject.name == "Shoes" || mr.gameObject.name == "Tops")
                            {
                                List<Material> materials = new List<Material>();
                                mr.GetMaterials(materials);
                                foreach (Material mat in materials)
                                {
                                    mat.SetColor("_Color", characterColor);
                                }
                            }
                        }
                    }
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
                float speed = truckLeaveSpeed.Evaluate((endingTime-6) / 4.0f);
                truckTrigger.transform.parent.Translate(Vector3.right * speed * Time.deltaTime);
            }

            if (endingTime > 12.0f)
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
