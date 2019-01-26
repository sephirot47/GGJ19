using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCamera : MonoBehaviour
{
    public Core core;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartShuffling()
    {
        core.StartShuffle();
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
