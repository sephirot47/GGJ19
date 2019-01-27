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
    Player owner;
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
                gameObject.layer = 0;
                foreach (Transform child in transform)
                {
                    child.gameObject.layer = 0;
                }
                break;

            case State.GRABBED:
                gameObject.layer = 11;
                foreach (Transform child in transform)
                {
                    child.gameObject.layer = 11;
                }
                break;

            case State.BEING_THROWN:
                gameObject.layer = 11;
                foreach (Transform child in transform)
                {
                    child.gameObject.layer = 11;
                }

                if (Time.time - timeStartedToBeThrown >= 1.0f)
                {
                    state = State.IDLE;
                    Physics.IgnoreCollision(GetComponentInChildren<Collider>(),
                                            owner.GetComponentInChildren<Collider>(),
                                            false);

                }
                break;
        }

        switch (state)
        {
            case State.BEING_THROWN:
                gameObject.layer = 0;
                foreach (Transform child in transform)
                {
                    child.gameObject.layer = 0;
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
    }

    public void SetOwner(Player playerOwner)
    {
        owner = playerOwner;
        playerOwnerId = owner.playerId;

        foreach (MeshRenderer mr in GetComponentsInChildren<MeshRenderer>())
        {
            List<Material> materials = new List<Material>();
            mr.GetMaterials(materials);
            foreach (Material mat in materials)
            {
                mat.SetColor("_Color", playerOwner.GetPlayerColor());
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

        if (state == State.BEING_THROWN)
        {
            timeStartedToBeThrown = Time.time;
            Physics.IgnoreCollision(GetComponentInChildren<Collider>(),
                                    owner.GetComponentInChildren<Collider>(),
                                    true);
        }

        rb.isKinematic = grabbed;
        rb.useGravity = !grabbed;
        rb.detectCollisions = !grabbed;
    }

    public static float GetGrabHeuristic(GrabbableObject grabbableObject, Player player)
    {
        float dist = Vector3.Distance(Planar(grabbableObject.transform.position), Planar(player.transform.position));
        float dot = Vector3.Dot(PlanarNorm(player.transform.forward), PlanarNorm(grabbableObject.transform.position - player.transform.position));
        if (dot < -0.3 || dist >= 2.5 || (grabbableObject.state != State.IDLE))
        {
            return -999999.9f;
        }
        
        float closenessHeuristic = (1.0f / dist) * (dot + 0.3f);
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
