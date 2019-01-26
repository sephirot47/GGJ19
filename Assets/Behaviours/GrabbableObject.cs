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
    public State state = State.IDLE;
    private Dictionary<Player, float> playersFocus;

    void Awake()
    {
        playersFocus = new Dictionary<Player, float>();
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        switch (state)
        {
            case State.IDLE:
                break;

            case State.GRABBED:
                GetComponentInChildren<Collider>().enabled = false;
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
                GetComponentInChildren<Collider>().enabled = true;
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

        Outline outline = GetComponent<Outline>();
        if (maxGrabPlayer && state == State.IDLE)
        {
            Color outlineColor = (maxGrabPlayer.playerId == PlayerId.CHILD ? Color.blue :
                maxGrabPlayer.playerId == PlayerId.DAD ? new Color(1.0f, 0.4f, 0.0f) : Color.magenta);
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
        switch (size)
        {
            case 0:
                transform.localScale = Vector3.one * 0.3f;
                break;

            case 1:
                transform.localScale = Vector3.one * 0.6f;
                break;

            case 2:
                transform.localScale = Vector3.one * 0.9f;
                break;
        }
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
