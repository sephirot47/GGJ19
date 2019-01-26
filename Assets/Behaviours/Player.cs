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

public class Player : MonoBehaviour
{
    public float baseSpeed = 3.0f;
    public float rotSpeed = 100.0f;
    public AnimationCurve speedVsDotCurve;

    private Animator animator;
    private Rigidbody rb;
    private GameObject handSocket;
    private GrabbableObject grabbedObject = null;

    private GrabbableObject lastGrabbedObject = null;
    private float lastGrabbedObjectReleaseTime = 0.0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        handSocket = gameObject.transform.FindDeepChild("HandSocket").gameObject;
    }

    private Vector3 Planar(Vector3 v)
    {
        return new Vector3(v.x, 0, v.z);
    }
    private Vector3 PlanarNorm(Vector3 v)
    {
        return Planar(v).normalized;
    }

    private void Update()
    {
        bool justGrabbed = false;

        List<GrabbableObject> grabbableObjects = new List<GrabbableObject>(FindObjectsOfType<GrabbableObject>());
        GrabbableObject closestGrabbableObject = null;
        if (!grabbedObject)
        {
            float closestGrabbableObjectDist = -999999.9f;
            foreach (GrabbableObject grabbableObject in grabbableObjects)
            {
                float dist = Vector3.Distance(Planar(grabbableObject.transform.position), Planar(transform.position));
                float dot = Vector3.Dot(PlanarNorm(transform.forward), PlanarNorm(grabbableObject.transform.position - transform.position));
                if (dot < 0 || dist >= 1 || grabbableObject == lastGrabbedObject)
                {
                    continue;
                }

                float closenessHeuristic = (1.0f / dist) * dot;
                if (closenessHeuristic >= closestGrabbableObjectDist)
                {
                    closestGrabbableObjectDist = closenessHeuristic;
                    closestGrabbableObject = grabbableObject;
                }
            }

            if (Input.GetKeyDown(KeyCode.LeftControl))
            {
                if (closestGrabbableObject)
                {
                    grabbedObject = closestGrabbableObject;
                    grabbedObject.PrepareRigidbodyForGrabbing(true);

                    grabbedObject.transform.parent = handSocket.transform;
                    grabbedObject.transform.localPosition = Vector3.zero;
                    grabbedObject.transform.localRotation = Quaternion.identity;

                    justGrabbed = true;
                }
            }
        }

        if (grabbedObject)
        {
            foreach (GrabbableObject grabbableObject in grabbableObjects)
            {
                grabbableObject.SetFocused(false, 0);
            }
        }
        else
        {
            foreach (GrabbableObject grabbableObject in grabbableObjects)
            {
                if (closestGrabbableObject == grabbableObject)
                {
                    grabbableObject.SetFocused(true, 0);
                }
                else
                {
                    grabbableObject.SetFocused(false, 0);
                }
            }
        }

        if (grabbedObject)
        {
            if (!justGrabbed && Input.GetKeyDown(KeyCode.LeftControl))
            {
                animator.SetTrigger("Throw");
            }
        }

        if (lastGrabbedObject)
        {
            if (Time.time - lastGrabbedObjectReleaseTime >= 1.0f)
            {
                Physics.IgnoreCollision(lastGrabbedObject.GetComponent<Collider>(),
                                        GetComponent<Collider>(),
                                        false);
                lastGrabbedObject = null;
            }
            else
            {
                Physics.IgnoreCollision(lastGrabbedObject.GetComponent<Collider>(),
                                        GetComponent<Collider>(),
                                        true);
            }
        }
    }

    void FixedUpdate()
    {
        float axisX = Input.GetAxis("Horizontal");
        float axisZ = Input.GetAxis("Vertical");

        Vector3 axisVector = new Vector3(axisX, 0, axisZ);
        if (axisVector.magnitude > 0)
        {
            Vector3 velocityDir = axisVector.normalized;
            transform.rotation = Quaternion.Slerp(Quaternion.LookRotation(transform.forward),
                                                  Quaternion.LookRotation(velocityDir),
                                                  rotSpeed * /*rb.velocity.magnitude **/ Time.deltaTime);

            Vector3 velocity = (transform.forward * baseSpeed);
            velocity *= speedVsDotCurve.Evaluate(Vector3.Dot(transform.forward, velocityDir)); //  * 0.5f + 0.5f;
            rb.velocity = new Vector3(velocity.x, rb.velocity.y, velocity.z);
        }
        animator.SetFloat("Velocity", rb.velocity.magnitude / baseSpeed);
    }

    void ReleaseGrabbedObject()
    {
        if (grabbedObject)
        {
            lastGrabbedObject = grabbedObject;
            lastGrabbedObjectReleaseTime = Time.time;

            grabbedObject.transform.parent = null;
            grabbedObject.PrepareRigidbodyForGrabbing(false);

            Rigidbody grabbedObjectRB = grabbedObject.GetComponent<Rigidbody>();
            grabbedObjectRB.velocity = transform.forward * 10.0f;

            grabbedObject = null;
        }
    }

    public void ThrowGrabbedObject()
    {
        ReleaseGrabbedObject();
    }
}
