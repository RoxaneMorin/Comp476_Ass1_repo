using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A base class for all character movements.
public class NPC : MonoBehaviour
{
    // VARIABLES

    // General
    [Header("General NPC Variables")]
    [SerializeField] protected float maxVelocity = 6f;
    [SerializeField] protected float defaultVelocity;
    protected Vector3 currentVelocity;
    public Vector3 CurrentVelocity { get { return currentVelocity; } }

    public GameObject myTarget;
    [SerializeField] protected GameObject previousTarget;
    [SerializeField] protected bool targetReached = false;

    // Obstacle detection.
    [Header("Obstacle Avoidance")]
    [SerializeField] protected bool useFeelers = true;
    [SerializeField] protected List<GameObject> toAvoidInoffensive;
    [SerializeField] protected List<GameObject> toAvoidDanger;
    [SerializeField] protected int feelersCount = 3;
    [SerializeField] protected float feelersAngle = 45f;
    [SerializeField] protected float feelersDistance = 3f;
    private float feelersAngleStep;

    // Rotation
    [Header("Rotation")]
    [SerializeField] protected bool YOnly = true;

    // Seek
    [Header("Seek & Flee")]
    [SerializeField] protected float seekStopRadius = 1f;

    // Arrive
    [Header("Arrive")]
    [SerializeField] protected float arriveStopRadius = 3f;
    [SerializeField] protected float arriveSlowRadius = 5f;

    // Pursue & Evade
    [Header("Pursue & Evade")]
    [SerializeField] protected float aheadDivider = 10f;

    // Wander
    [Header("Wander")]
    [SerializeField] float wanderInterval = 1f;
    [SerializeField] float wanderDegreesDelta = 180f;
    protected float wanderTimer;
    protected Vector3 lastWanderDirection;
    protected Vector3 lastDisplacement;



    // EVENTS
    protected delegate void EventTargetReached(GameObject target);
    protected event EventTargetReached OnTargetReached;



    // METHODS

    // Utilities 

    // TO DO: target passed as a parameter?
    protected Vector3 DesiredVelocityIfTarget(GameObject target)
    {
        return target != null ? target.transform.position - transform.position : Vector3.zero;
    }

    // LookWhereYouAreGoing
    protected Quaternion LookWhereYouAreGoingKinematic()
    {
        if (currentVelocity == Vector3.zero)
            return transform.rotation;

        return Quaternion.LookRotation(currentVelocity);
    }
    protected Quaternion LookWhereYouAreGoingSteer()
    {
        if (YOnly)
        {
            Vector3 from = Vector3.ProjectOnPlane(transform.forward, Vector3.up);
            Vector3 to = LookWhereYouAreGoingKinematic() * Vector3.forward;
            float angleY = Vector3.SignedAngle(from, to, Vector3.up);
            return Quaternion.AngleAxis(angleY, Vector3.up);
        }

        return Quaternion.FromToRotation(transform.forward, LookWhereYouAreGoingKinematic() * Vector3.forward);
    }

    // Seek
    protected Vector3 SeekKinematic(GameObject target)
    {
        Vector3 desiredVelocity = DesiredVelocityIfTarget(target);

        // Notify that the target has been reached. Only used for state change events.
        if (!targetReached & (desiredVelocity.magnitude <= seekStopRadius))
        {
            Debug.Log(string.Format("{0} has reached {1}.", gameObject, target));
            
            targetReached = true;
            OnTargetReached?.Invoke(target);
        }

        desiredVelocity = desiredVelocity.normalized * maxVelocity;
        return desiredVelocity;
    }
    protected Vector3 SeekKinematic(Vector3 target)
    {
        Vector3 desiredVelocity = target - transform.position;
        desiredVelocity = desiredVelocity.normalized * maxVelocity;
        return desiredVelocity;
    }
    protected Vector3 SeekSteer(GameObject target)
    {
        return SeekKinematic(target) - currentVelocity;
    }

    // Flee
    protected Vector3 FleeKinematic(GameObject target)
    {
        Vector3 desiredVelocity = -DesiredVelocityIfTarget(target);
        desiredVelocity = desiredVelocity.normalized * maxVelocity;
        return desiredVelocity;
    }
    protected Vector3 FleeSteer(GameObject target)
    {
        return FleeKinematic(target) - currentVelocity;
    }

    // Arrive
    protected Vector3 ArriveKinematic(GameObject target)
    {
        Vector3 desiredVelocity = DesiredVelocityIfTarget(target);
        float distance = desiredVelocity.magnitude;
        desiredVelocity = desiredVelocity.normalized * maxVelocity;

        if (distance <= arriveStopRadius)
        {
            desiredVelocity = Vector3.zero;

            // Notify that the target has been reached.
            if (!targetReached)
            {
                targetReached = true;
                OnTargetReached?.Invoke(target);
            }
        }
        else if (distance < arriveSlowRadius)
            desiredVelocity *= (distance / arriveSlowRadius);

        return desiredVelocity;
    }
    protected Vector3 ArriveSteer(GameObject target)
    {
        return ArriveKinematic(target) - currentVelocity;
    }

    // Pursue
    protected Vector3 PursueSteer(GameObject target)
    {
        try // There might be a more efficient way to do this.
        {
            NPC targetNPC = target.GetComponent<NPC>();

            float distance = Vector3.Distance(target.transform.position, transform.position);
            float ahead = distance / aheadDivider;
            Vector3 futurePosition = target.transform.position + targetNPC.CurrentVelocity * ahead;

            return SeekKinematic(futurePosition) - currentVelocity;
        }
        catch
        {
            Debug.Log(string.Format("{0}'s pursue target ({1}) is not of a valid NPC type.", this.gameObject, target));
        }

        return transform.position; // Return the object's current position instead.
    }

    // Wander
    protected Vector3 WanderKinematic(GameObject dummy)
    {
        wanderTimer += Time.deltaTime;

        // Edit this to take into account the previous target?
        if (lastWanderDirection == Vector3.zero)
            lastWanderDirection = transform.forward.normalized * maxVelocity;

        if (lastDisplacement == Vector3.zero)
            lastDisplacement = transform.forward;

        Vector3 desiredVelocity = lastDisplacement;
        if (wanderTimer > wanderInterval)
        {
            float angle = (UnityEngine.Random.value - UnityEngine.Random.value) * wanderDegreesDelta;
            Vector3 direction = Quaternion.AngleAxis(angle, Vector3.up) * lastWanderDirection.normalized;
            Vector3 circleCenter = transform.position + lastDisplacement;
            Vector3 destination = circleCenter + direction.normalized;
            desiredVelocity = destination - transform.position;
            desiredVelocity = desiredVelocity.normalized * maxVelocity;

            lastDisplacement = desiredVelocity;
            lastWanderDirection = direction;
            wanderTimer = 0;
        }

        return desiredVelocity;
    }
    protected Vector3 WanderSteer(GameObject dummy)
    {
        return WanderKinematic(dummy) - currentVelocity;
    }
    
    
    // Base Move function.
    virtual protected void Move(Vector3 desiredVelocity, bool avoidObstacles = true) 
    {
        // Add weights to the different categories?
        if (avoidObstacles)
        {
            if (useFeelers)
            {
                CastFeelers();
                desiredVelocity += AvoidObstacles(toAvoidInoffensive) * 0.5f;
            }
            desiredVelocity += AvoidObstacles(toAvoidDanger) * 1.5f;
        }

        desiredVelocity = Vector3.ClampMagnitude(desiredVelocity, maxVelocity);
        currentVelocity = Vector3.Lerp(currentVelocity, desiredVelocity, Time.deltaTime);

        // Try and avoid sinking into the ground.
        currentVelocity.y = 0;

        transform.position += currentVelocity * Time.deltaTime;

        // Just face where you are going.
        transform.rotation = Quaternion.Slerp(transform.rotation, transform.rotation * LookWhereYouAreGoingSteer(), Time.deltaTime);
    }

    // Obstacle avoidance.
    protected void CastFeelers() // Replace feelers by colliders if they are too greedy.
    {
        // Clear out the previous list of obstacles.
        toAvoidInoffensive.Clear();

        // Prepare
        float angleStep = feelersAngle / (feelersCount - 1);
        float halfAngle = feelersAngle / 2;

        Vector3 startDirection = Quaternion.Euler(0, -halfAngle, 0) * transform.forward;

        for (int i = 0; i < feelersCount; i++)
        {
            Vector3 currentDirection = Quaternion.Euler(0, i * angleStep, 0) * startDirection;
            Ray ray = new Ray(transform.position, currentDirection);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, feelersDistance))
            {
                // TO DO: only register the relevant objects.
                toAvoidInoffensive.Add(hit.collider.gameObject);
            }

            // Visualize their little feelers uwu
            Debug.DrawRay(ray.origin, ray.direction * feelersDistance, Color.white);
        }
    }
    protected Vector3 AvoidObstacles(List<GameObject> obstacles)
    {
        Vector3 desiredVelocity = Vector3.zero;

        foreach (GameObject obstacle in obstacles)
        {
            desiredVelocity += FleeSteer(obstacle);
        }

        return desiredVelocity;
    }


    // Misc
    protected Prisoner FindClosestPrisoner()
    {
        GameObject[] potentialPrisoners = GameObject.FindGameObjectsWithTag("Prisoner");

        if (potentialPrisoners.Length == 1)
        {
            return potentialPrisoners[0].GetComponent<Prisoner>();
        }
        else if (potentialPrisoners.Length > 0)
        {
            float closestDistance = Mathf.Infinity;
            GameObject closestPotentialPrisoner = null;

            foreach (GameObject potentialPrisoner in potentialPrisoners)
            {
                float distance = Vector3.Distance(gameObject.transform.position, potentialPrisoner.transform.position);
                if (distance < closestDistance && potentialPrisoner.activeSelf == true)
                {
                    closestDistance = distance;
                    closestPotentialPrisoner = potentialPrisoner;
                }
            }
            return closestPotentialPrisoner.GetComponent<Prisoner>();
        }
        else // Will probably need to handle state changes.
            return null;
    }


    // Event handlers.
    virtual protected void TargetReached(GameObject target)
    {
        // Debug.Log(string.Format("{0} has reached its target, {1}.", this, target));
    }
}
