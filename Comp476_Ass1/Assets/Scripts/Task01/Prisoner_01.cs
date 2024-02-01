using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Prisoner_01 : NPC
{
    public enum PrisonerStates
    {
        Wait,
        ReachFortress
    }

    // VARIABLES
    public PrisonerStates myState = PrisonerStates.Wait;
    Func<Vector3>[] moveFunctions; // Possible move functions, should follow the indexing of PrisonerStates.



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
    }


    // Built in
    void Start()
    {
        // Register the event listeners.
        OnTargetReached += TargetReached;

        // Furnish the possible move functions.
        moveFunctions = new Func<Vector3>[]
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
