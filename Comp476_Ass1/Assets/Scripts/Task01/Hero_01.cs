using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hero_01 : NPC
{
    public enum HeroStates
    {
        ReachPrisoner,
        ReachFortress
    }

    // VARIABLES.
    [SerializeField] protected GameObject myFortress;

    public HeroStates myState = HeroStates.ReachPrisoner;
    Func<Vector3>[] moveFunctions; // Possible move functions, should follow the indexing of HeroStates.



    // METHODS

    // Movement
    override protected void Move()
    {
        Vector3 desiredVelocity = moveFunctions[(int)myState]();
        currentVelocity = Vector3.Lerp(currentVelocity, desiredVelocity, Time.deltaTime);

        transform.position += currentVelocity * Time.deltaTime;
    }


    // Event receivers.
    override protected void TargetReached()
    {
        base.TargetReached();

        if (myState == HeroStates.ReachPrisoner)
        {
            myState = HeroStates.ReachFortress;

            // Update the prisoner's suff.
            Prisoner_01 targetPrisoner = myTarget.GetComponent<Prisoner_01>();
            targetPrisoner.myTarget = myFortress;
            targetPrisoner.myState = Prisoner_01.PrisonerStates.ReachFortress;

            // Update hero's own stuff.
            myTarget = myFortress;
            targetReached = false;
        }
    }


    // Built in.

    void Start()
    {
        // Register the event listeners.
        OnTargetReached += TargetReached;

        // Furnish the possible move functions.
        moveFunctions = new Func<Vector3>[]
        {
            // They are indexed in accordance with the HeroState enum.
            ArriveSteer,
            SeekSteer
        };
    }

    void Update()
    {
        Move();
    }
}
