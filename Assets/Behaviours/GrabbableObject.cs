using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabbableObject : MonoBehaviour
{
    public enum State
    {
        IDLE, GRABBED, BEING_THROWN
    };

    private Rigidbody rb;
    private float timeStartedToBeThrown = 0.0f;
    private int objectSize = 0;
    private PlayerId playerOwnerId = PlayerId.CHILD;
    public State state = State.IDLE;
    private Dictionary<Player, float> playersFocus;

    void Awake()
    {
        playersFocus = new Dictionary<Player, float>();
        rb = GetComponentInChildren<Rigidbody>();
        GetComponentInChildren<Outline>().enabled = false;
    }

    void Update()
    {
        switch (state)
        {
            case State.IDLE:
                break;

            case State.GRABBED:
                if (GetComponentInChildren<Collider>())
                {
                    GetComponentInChildren<Collider>().enabled = false;
                }
                break;

            case State.BEING_THROWN:
                if (Time.time - timeStartedToBeThrown >= 1.0f)
                {
                    state = State.IDLE;
                }
                break;
        }

        switch (state)
        {
            case State.IDLE:
            case State.BEING_THROWN:
                if (GetComponentInChildren<Collider>())
                {
                    GetComponentInChildren<Collider>().enabled = true;
                }
                break;
        }

        Player maxGrabPlayer = null;
        float maxGrabHeuristic = 0.0f;
        foreach (KeyValuePair<Player,float> pair in playersFocus)
        {
            Player player = pair.Key;
            float grabHeuristic = pair.Value;
            if (grabHeuristic > maxGrabHeuristic)
            {
                maxGrabPlayer = player;
                maxGrabHeuristic = grabHeuristic;
            }
        }

        Outline outline = GetComponentInChildren<Outline>();
        if (maxGrabPlayer && state == State.IDLE)
        {
            Color outlineColor = maxGrabPlayer.GetPlayerColor();
            outline.OutlineColor = outlineColor;
            outline.enabled = true;
        }
        else
        {
            outline.enabled = false;
        }
    }

    private static Vector3 Planar(Vector3 v)
    {
        return new Vector3(v.x, 0, v.z);
    }
    private static Vector3 PlanarNorm(Vector3 v)
    {
        return Planar(v).normalized;
    }
    
    public void SetSize(int size)
    {
        objectSize = size;
        /*
        float scaleFactorBasedOnPlayer = (playerOwnerId == PlayerId.CHILD ? 1.0f :
            playerOwnerId == PlayerId.DAD ? 2.0f : 2.0f);
        switch (size)
        {
            case 1:
                transform.localScale = Vector3.one * 0.3f;
                break;

            case 2:
                transform.localScale = Vector3.one * 0.3f + Vector3.up * 0.5f;
                break;

            case 3:
                transform.localScale = Vector3.one * 0.3f + Vector3.up * 1.0f;
                break;
        }
        transform.localScale = transform.localScale * scaleFactorBasedOnPlayer;
        */
    }

    public void SetOwner(PlayerId playerOwnerId_, Color ownerColor)
    {
        playerOwnerId = playerOwnerId_;

        foreach (MeshRenderer mr in GetComponentsInChildren<MeshRenderer>())
        {
            List<Material> materials = new List<Material>();
            mr.GetMaterials(materials);
            foreach (Material mat in materials)
            {
                mat.SetColor("_Color", ownerColor);
            }
        }
    }

    public PlayerId GetOwner()
    {
        return playerOwnerId;
    }

    public int GetSize()
    {
        return objectSize;
    }

    public void SetGrabbed(bool grabbed)
    {
        state = (grabbed ? State.GRABBED : State.BEING_THROWN);
        if (!grabbed)
        {
            timeStartedToBeThrown = Time.time;
        }

        rb.isKinematic = grabbed;
        rb.useGravity = !grabbed;
        rb.detectCollisions = !grabbed;
    }

    public static float GetGrabHeuristic(GrabbableObject grabbableObject, Player player)
    {
        float dist = Vector3.Distance(Planar(grabbableObject.transform.position), Planar(player.transform.position));
        float dot = Vector3.Dot(PlanarNorm(player.transform.forward), PlanarNorm(grabbableObject.transform.position - player.transform.position));
        if (dot < 0 || dist >= 1.5 || (grabbableObject.state != State.IDLE))
        {
            return -999999.9f;
        }


        float closenessHeuristic = (1.0f / dist) * dot;
        return closenessHeuristic;
    }

    public void SetFocused(bool focused, Player player)
    {
        if (focused)
        {
            float grabHeuristic = GetGrabHeuristic(this, player);
            playersFocus.Add(player, grabHeuristic);
        }
        else
        {
            playersFocus.Remove(player);
        }
    }
}
