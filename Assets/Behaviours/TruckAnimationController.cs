using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TruckAnimationController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OpenTruck()
    {
        Animation anim = GetComponentInChildren<Animation>();
        anim.Play("Close");
    }

    public void CloseTruck()
    {
        Animation anim = GetComponentInChildren<Animation>();
        anim.Play("Open");
    }
}
