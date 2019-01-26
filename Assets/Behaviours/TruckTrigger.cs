using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TruckTrigger : MonoBehaviour
{
    Core core;

    void Start()
    {
        core = FindObjectOfType<Core>();
    }

    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        ProcessTrigger(other, true);
    }

    private void OnTriggerExit(Collider other)
    {
        ProcessTrigger(other, false);
    }

    private void ProcessTrigger(Collider collider, bool enter)
    {
        GrabbableObject grabbableObject = collider.GetComponentInParent<GrabbableObject>();
        if (grabbableObject)
        {
            int objectSize = grabbableObject.GetSize();
            PlayerId ownerId = grabbableObject.GetOwner();
            if (enter)
            {
                core.AddToScore(ownerId, objectSize);
            }
            else
            {
                core.AddToScore(ownerId, -objectSize);
            }
        }
    }
}
