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
    private Color playerColor;
    
    private Animator animator;
    private CharacterController cc;
    private GameObject handSocket;
    private GameObject grabSocket;
    private bool isInsideParking;
    private GrabbableObject grabbedObject = null;

    public List<GameObject> grabbableObjectsPrefabsBig;
    public List<GameObject> grabbableObjectsPrefabsMedium;
    public List<GameObject> grabbableObjectsPrefabsSmall;

    void Start()
    {
        cc = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        handSocket = gameObject.transform.FindDeepChild("HandSocket").gameObject;
        grabSocket = gameObject.transform.FindDeepChild("GrabSocket").gameObject;
        SetIsInsideParking(false);

        if (playerId == PlayerId.CHILD) SetPlayerColor(Core.childColor);
        else if (playerId == PlayerId.DAD) SetPlayerColor(Core.dadColor);
        else if (playerId == PlayerId.MUM) SetPlayerColor(Core.mumColor);

        grabbableObjectsPrefabsBig = new List<GameObject>();
        grabbableObjectsPrefabsMedium = new List<GameObject>();
        grabbableObjectsPrefabsSmall = new List<GameObject>();
        grabbableObjectsPrefabsBig.Add(Resources.Load<GameObject>("Objects_L/jar_L"));
        grabbableObjectsPrefabsBig.Add(Resources.Load<GameObject>("Objects_L/carpet_L"));
        grabbableObjectsPrefabsMedium.Add(Resources.Load<GameObject>("Objects_M/suitcase_M"));
        grabbableObjectsPrefabsMedium.Add(Resources.Load<GameObject>("Objects_M/crt_M"));
        grabbableObjectsPrefabsSmall.Add(Resources.Load<GameObject>("Objects_S/clothLamp_S"));
    }

    private void Update()
    {
        if (Core.core.GetState() != Core.State.PLAYING)
        {
            cc.SimpleMove(Vector3.zero);
            animator.SetFloat("Velocity", 0.0f);
            animator.SetBool("Grabbing", false);
            return;
        }

        transform.Find("KeysQuad").gameObject.active = false;

        float axisX = Input.GetAxis("Horizontal" + playerId.ToString());
        float axisZ = Input.GetAxis("Vertical" + playerId.ToString());

        Vector3 velocity = Vector3.zero;
        Vector3 velocityWithoutWeight = Vector3.zero;
        if (CanWalk())
        {
            Vector3 axisVector = new Vector3(axisX, 0, axisZ);
            if (axisVector.magnitude > 0)
            {
                Vector3 velocityDir = axisVector.normalized;
                transform.rotation = Quaternion.Slerp(Quaternion.LookRotation(transform.forward),
                                                      Quaternion.LookRotation(velocityDir),
                                                      rotSpeed * Time.deltaTime);
                
                velocityWithoutWeight = transform.forward * baseSpeed;
                velocity = velocityWithoutWeight * GetWeightSpeedFactor();
                velocity *= speedVsDotCurve.Evaluate(Vector3.Dot(transform.forward, velocityDir));
            }
        }

        cc.SimpleMove(new Vector3(velocity.x, cc.velocity.y, velocity.z));
        animator.SetFloat("Velocity", Planar(velocityWithoutWeight).magnitude / baseSpeed);

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

                int sizeOfObjectToPick = Random.Range(1, 4);
                GameObject prefab = null;
                List<GameObject> prefabList = (sizeOfObjectToPick == 1 ?
                    grabbableObjectsPrefabsSmall : sizeOfObjectToPick == 2 ?
                    grabbableObjectsPrefabsMedium : grabbableObjectsPrefabsBig);
                prefab = prefabList[ Random.Range(0, prefabList.Count) ];
                
                GameObject newGrabbableObjectGo = GameObject.Instantiate<GameObject>(prefab);
                GrabbableObject newGrabbableObject = newGrabbableObjectGo.GetComponent<GrabbableObject>();
                GrabObject(newGrabbableObject);
                newGrabbableObject.SetOwner(playerId, playerColor);
                newGrabbableObject.SetSize(sizeOfObjectToPick);
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

    public void SetPlayerColor(Color color)
    {
        playerColor = color;

        foreach (SkinnedMeshRenderer mr in GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            if (mr.gameObject.name == "Bottoms" || mr.gameObject.name == "Hats" ||
                mr.gameObject.name == "Shoes" || mr.gameObject.name == "Tops")
            {
                List<Material> materials = new List<Material>();
                mr.GetMaterials(materials);
                foreach (Material mat in materials)
                {
                    mat.SetColor("_Color", color);
                }
            }
        }
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

    private static Vector3 Planar(Vector3 v)
    {
        return new Vector3(v.x, 0, v.z);
    }
    private static Vector3 PlanarNorm(Vector3 v)
    {
        return Planar(v).normalized;
    }

    public Color GetPlayerColor()
    {
        return playerColor;
    }
    
    float GetWeightSpeedFactor()
    {
        if (grabbedObject)
        {
            return 1.0f - (grabbedObject.GetSize() * 0.1f);
        }
        return 1.0f;
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

            Rigidbody grabbedObjectRB = grabbedObject.GetComponentInChildren<Rigidbody>();
            grabbedObjectRB.velocity = transform.forward * 10.0f;

            grabbedObject = null;
        }
    }

    public void SetIsInsideParking(bool insideParking)
    {
        isInsideParking = insideParking;

        Outline outline = GetComponent<Outline>();
        if (isInsideParking)
        {
            outline.enabled = true;
            outline.OutlineColor = GetPlayerColor();
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
