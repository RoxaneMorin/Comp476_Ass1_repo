using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Prisoner : NPC
{
    public enum PrisonerStates
    {
        Wait,
        ReachFortress
    }

    // VARIABLES
    [Space]
    [Header("Prisoner Variables")]
    [SerializeField] protected Hero myFriendHero;

    [Space]

    [SerializeField] private PrisonerStates myState = PrisonerStates.Wait;
    [SerializeField] private PrisonerStates previousState = PrisonerStates.Wait;

    Func<GameObject, Vector3>[] moveFunctionsPerState; // Possible move functions, should follow the indexing of PrisonerStates.



    // METHODS

    public void InitRescue(Hero rescuer, GameObject targetFortress)
    {
        previousTarget = myTarget;
        previousState = myState;

        myTarget = targetFortress;
        myState = Prisoner.PrisonerStates.ReachFortress;
    }
    public void StopRescue()
    {
        previousTarget = myTarget;
        previousState = myState;

        myState = PrisonerStates.Wait;
    }


    // Movement
    protected void Move()
    {
        // Simple as we don't have much to do beside obstacle avoidance.
        Vector3 desiredVelocity = moveFunctionsPerState[(int)myState](myTarget);

        Move(desiredVelocity);
    }


    // Event receivers.
    override protected void TargetReached(GameObject target)
    {
        base.TargetReached(target);
    }


    // Built in
    void Start()
    {
        // Register the event listeners.
        OnTargetReached += TargetReached;

        // Furnish the possible move functions.
        moveFunctionsPerState = new Func<GameObject, Vector3>[]
        {
            // They are indexed in accordance with the PrisonerStates enum.
            null,
            SeekSteer
        };
    }

    void Update()
    {
        if (myState == PrisonerStates.ReachFortress)
        {
            Move();
        }
    }
}
