using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabbableObject : MonoBehaviour
{
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
    }

    public void PrepareRigidbodyForGrabbing(bool forGrabbing)
    {
        rb.isKinematic = forGrabbing;
        rb.useGravity = !forGrabbing;
        rb.detectCollisions = !forGrabbing;
    }

    public void SetFocused(bool focused, PlayerId playerId)
    {
        Outline outline = GetComponent<Outline>();
        if (focused)
        {
            outline.OutlineColor = Color.blue;
            outline.enabled = true;
        }
        else
        {
            outline.enabled = false;
        }
    }
}
