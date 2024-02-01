using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A base class for all character movements.
public class NPC : MonoBehaviour
{
    // VARIABLES

    // General
    [SerializeField] protected float maxVelocity = 6f;
    [SerializeField] protected Vector3 currentVelocity; // Serialize for now to have it visible in editor.

    public GameObject myTarget;
    [SerializeField] protected bool targetReached = false;

    // Seek
    [SerializeField] protected float seekStopRadius = 1f;

    // Arrive
    [SerializeField] protected float arriveStopRadius = 3f;
    [SerializeField] protected float arriveSlowRadius = 5f;



    // EVENTS
    protected delegate void EventTargetReached();
    protected event EventTargetReached OnTargetReached;
    


    // METHODS

    // Seek
    protected Vector3 SeekKinematic()
    {
        Vector3 desiredVelocity = myTarget.transform.position - transform.position;

        // Notify that the target has been reached. Only used for state change events.
        if (!targetReached & (desiredVelocity.magnitude <= seekStopRadius))
        {
            targetReached = true;
            OnTargetReached?.Invoke();
        }

        desiredVelocity = desiredVelocity.normalized * maxVelocity;
        return desiredVelocity;
    }
    protected Vector3 SeekSteer()
    {
        return SeekKinematic() - currentVelocity;
    }

    // Arrive
    protected Vector3 ArriveKinematic()
    {
        Vector3 desiredVelocity = myTarget.transform.position - transform.position;
        float distance = desiredVelocity.magnitude;
        desiredVelocity = desiredVelocity.normalized * maxVelocity;

        if (distance <= arriveStopRadius)
        {
            desiredVelocity = Vector3.zero;

            // Notify that the target has been reached.
            if (!targetReached)
            {
                targetReached = true;
                OnTargetReached?.Invoke();
            }
        }
        else if (distance < arriveSlowRadius)
            desiredVelocity *= (distance / arriveSlowRadius);

        return desiredVelocity;
    }
    protected Vector3 ArriveSteer()
    {
        return ArriveKinematic() - currentVelocity;
    }
    
    
    // Combiners.
    virtual protected void Move() { }


    // Event handlers.
    virtual protected void TargetReached()
    {
        Debug.Log(string.Format("{0} has reached its target, {1}.", this, myTarget));
    }
}
