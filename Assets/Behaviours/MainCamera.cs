using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class MainCamera : MonoBehaviour
{
    public Core core;
    public GameObject truck;
    public Animation titleAnimation;
    float beginTitleAnimationTime;
    bool firstAnimationPlayed = false;

    // Start is called before the first frame update
    void Start()
    {
        beginTitleAnimationTime = Time.time;
        titleAnimation["Title_Enter"].speed = 0.0f;
        titleAnimation.Play("Title_Enter");
    }

    // Update is called once per frame
    void Update()
    {   
        if (Time.time - beginTitleAnimationTime > 5)
        {
            titleAnimation["Title_Enter"].speed = 1.5f;
            titleAnimation.Play("Title_Enter");

            if (Time.time - beginTitleAnimationTime > 10)
            {
                if (!firstAnimationPlayed && Input.GetKeyDown(KeyCode.Space))
                {
                    firstAnimationPlayed = true;
                    GetComponent<PlayableDirector>().Play();
                }
            }
        }
    }

    public void StartShuffling()
    {
        core.StartShuffle();
        truck.SetActive(true);
    }
    public void StartShowControls()
    {
        core.StartShowControls();
    }
    public void StartPlaying()
    {
        core.StartPlaying();
    }

}
