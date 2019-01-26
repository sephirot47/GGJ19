using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public static class TransformDeepChildExtension
{
    //Breadth-first search
    public static Transform FindDeepChild(this Transform aParent, string aName)
    {
        var result = aParent.Find(aName);
        if (result != null)
            return result;
        foreach (Transform child in aParent)
        {
            result = child.FindDeepChild(aName);
            if (result != null) return result;
        }
        return null;
    }
}

public enum PlayerId
{
    CHILD = 0,
    MUM,
    DAD
};

public class Player : MonoBehaviour
{
    public float baseSpeed = 3.0f;
    public float rotSpeed = 100.0f;
    public AnimationCurve speedVsDotCurve;
    public PlayerId playerId;
    public GameObject grabbableObjectPrefab;

    private int nextSizeOfObjectToPick = 0;
    private Animator animator;
    private Rigidbody rb;
    private GameObject handSocket;
    private GameObject grabSocket;
    private bool isInsideParking;
    private GrabbableObject grabbedObject = null;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        handSocket = gameObject.transform.FindDeepChild("HandSocket").gameObject;
        grabSocket = gameObject.transform.FindDeepChild("GrabSocket").gameObject;
        SetIsInsideParking(false);
    }

    private void Update()
    {
        bool justGrabbed = false;

        List<GrabbableObject> grabbableObjects = new List<GrabbableObject>(FindObjectsOfType<GrabbableObject>());
        foreach (GrabbableObject grabbableObject in grabbableObjects)
        {
            grabbableObject.SetFocused(false, this);
        }

        if (isInsideParking)
        {
            if (Input.GetButtonDown("Action" + playerId.ToString()))
            {
                if (grabbedObject)
                {
                    grabbedObject.gameObject.SetActive(false);
                    GameObject.Destroy(grabbedObject.gameObject);
                    ReleaseGrabbedObject();
                }

                GameObject newGrabbableObjectGo = GameObject.Instantiate<GameObject>(grabbableObjectPrefab);
                GrabbableObject newGrabbableObject = newGrabbableObjectGo.GetComponent<GrabbableObject>();
                GrabObject(newGrabbableObject);
                newGrabbableObject.SetSize(nextSizeOfObjectToPick);

                nextSizeOfObjectToPick = (nextSizeOfObjectToPick + 1) % 3;
            }
        }
        else
        {
            GrabbableObject closestGrabbableObject = null;
            if (!grabbedObject)
            {
                float closestGrabbableObjectDist = 0.0f;
                foreach (GrabbableObject grabbableObject in grabbableObjects)
                {
                    float closenessHeuristic = GrabbableObject.GetGrabHeuristic(grabbableObject, this);
                    if (closenessHeuristic >= closestGrabbableObjectDist)
                    {
                        closestGrabbableObjectDist = closenessHeuristic;
                        closestGrabbableObject = grabbableObject;
                    }
                }

                if (Input.GetButtonDown("Action" + playerId.ToString()))
                {
                    if (closestGrabbableObject)
                    {
                        GrabObject(closestGrabbableObject);
                        justGrabbed = true;
                    }
                }
            }

            if (!grabbedObject && closestGrabbableObject)
            {
                closestGrabbableObject.SetFocused(true, this);
            }

            if (grabbedObject)
            {
                if (!justGrabbed && Input.GetButtonDown("Action" + playerId.ToString()))
                {
                    grabbedObject.transform.parent = handSocket.transform;
                    animator.SetTrigger("Throw");
                }
            }
        }

        animator.SetBool("Grabbing", (grabbedObject != null));
    }

    bool CanWalk()
    {
        bool canWalk = true;
        if (playerId == PlayerId.DAD)
        {
            canWalk = !animator.GetCurrentAnimatorStateInfo(2).IsName("Throw");
        }
        else if (playerId == PlayerId.MUM)
        {
            canWalk = !animator.GetCurrentAnimatorStateInfo(2).IsName("Throw");
        }
        return canWalk;
    }

    void FixedUpdate()
    {
        float axisX = Input.GetAxis("Horizontal" + playerId.ToString());
        float axisZ = Input.GetAxis("Vertical" + playerId.ToString());
        
        if (CanWalk())
        {
            Vector3 axisVector = new Vector3(axisX, 0, axisZ);
            if (axisVector.magnitude > 0)
            {
                Vector3 velocityDir = axisVector.normalized;
                transform.rotation = Quaternion.Slerp(Quaternion.LookRotation(transform.forward),
                                                      Quaternion.LookRotation(velocityDir),
                                                      rotSpeed * /*rb.velocity.magnitude **/ Time.deltaTime);

                Vector3 velocity = (transform.forward * baseSpeed);
                velocity *= speedVsDotCurve.Evaluate(Vector3.Dot(transform.forward, velocityDir));
                rb.velocity = new Vector3(velocity.x, rb.velocity.y, velocity.z);
            }
        }
        else
        {
            rb.velocity = new Vector3(0, rb.velocity.y, 0);
        }

        animator.SetFloat("Velocity", rb.velocity.magnitude / baseSpeed);
    }

    void GrabObject(GrabbableObject grabbableObject)
    {
        grabbedObject = grabbableObject;
        grabbedObject.SetGrabbed(true);

        grabbedObject.transform.SetParent(grabSocket.transform);
        grabbedObject.transform.localPosition = Vector3.zero;
        grabbedObject.transform.localRotation = Quaternion.identity;
    }

    void ReleaseGrabbedObject()
    {
        if (grabbedObject)
        {
            grabbedObject.transform.parent = null;
            grabbedObject.SetGrabbed(false);

            Rigidbody grabbedObjectRB = grabbedObject.GetComponent<Rigidbody>();
            grabbedObjectRB.velocity = transform.forward * 10.0f;

            grabbedObject = null;
        }
    }

    public void SetIsInsideParking(bool insideParking)
    {
        bool hasChanged = (isInsideParking != insideParking);

        isInsideParking = insideParking;

        Outline outline = GetComponent<Outline>();
        if (isInsideParking)
        {
            outline.enabled = true;
            if (hasChanged)
            {
                nextSizeOfObjectToPick = 0;
            }
        }
        else
        {
            outline.enabled = false;
        }
    }

    public void ThrowGrabbedObject()
    {
        ReleaseGrabbedObject();
    }
}
