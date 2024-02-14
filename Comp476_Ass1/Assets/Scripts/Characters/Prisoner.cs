using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Prisoner : NPC
{
    public enum PrisonerStates
    {
        Wait,
        ReachFortress,
        FollowPlayer
    }

    // VARIABLES
    [Space]
    [Header("Prisoner Variables")]
    [SerializeField] protected Hero myFriendHero;

    [Space]

    [SerializeField] private PrisonerStates myState = PrisonerStates.Wait;
    public PrisonerStates MyState { get { return myState; }}
    [SerializeField] private PrisonerStates previousState = PrisonerStates.Wait;
    [SerializeField] private bool isBusy = false;
    public bool IsBusy { get { return isBusy; } }

    Func<GameObject, Vector3>[] moveFunctionsPerState; // Possible move functions, should follow the indexing of PrisonerStates.



    // METHODS

    public void InitRescue(Hero rescuer, GameObject targetFortress)
    {
        previousTarget = myTarget;
        previousState = myState;

        myFriendHero = rescuer;
        myTarget = myFriendHero.gameObject;
        myState = Prisoner.PrisonerStates.ReachFortress;
        isBusy = true;
    }
    public void StopRescue()
    {
        previousTarget = myTarget;
        previousState = myState;

        myFriendHero = null;
        myState = PrisonerStates.Wait;
        isBusy = false;
    }

    public void InitFollowPlayer(GameObject thePlayer)
    {
        previousTarget = myTarget;
        previousState = myState;
        myFriendHero = null;

        isBusy = true;
        myTarget = thePlayer;
        myState = PrisonerStates.FollowPlayer;
    }


    // Movement
    protected void Move()
    {
         //Simple as we don't have much to do beside obstacle avoidance.
        Vector3 desiredVelocity = moveFunctionsPerState[(int)myState](myTarget);

        Move(desiredVelocity);
    }


    // To do: add a check for nearby guardians, which can be consulted by the friend hero.


    // Event receivers.
    override protected void MyTargetReached(GameObject target)
    {
        base.MyTargetReached(target);
    }


    // Built in
    void Start()
    {
        // Dynamic variables.
        defaultVelocity = maxVelocity;

        // Register the event listeners.
        OnTargetReached += MyTargetReached;

        // Furnish the possible move functions.
        moveFunctionsPerState = new Func<GameObject, Vector3>[]
        {
            // They are indexed in accordance with the PrisonerStates enum.
            null,
            ArriveSteer,
            ArriveSteer
        };
    }

    void Update()
    {
        if (myState != PrisonerStates.Wait)
        {
            Move();
        }
    }
}
