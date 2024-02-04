using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Guardian : NPC
{
    public enum GuardianStates
    {
        Wander,
        PursueHero,
        ReachPrisoner,
        Stop
    }

    // VARIABLES
    [Header("Guardian Variables")]
    [SerializeField] protected GuardianFoV myFoV;
    //[SerializeField] protected GuardianSneakZone mySneakZone;
    [SerializeField] protected GuardianCaptureZone myCaptureZone;
    [SerializeField] protected GameObject myPrisoner;

    [Space]

    [SerializeField] protected float maxDistanceFromMyPrisoner = 10f;

    [Space]

    [SerializeField] private GuardianStates myState = GuardianStates.Wander;
    [SerializeField] private GuardianStates previousState = GuardianStates.Wander;

    Func<GameObject, Vector3>[] moveFunctionsPerState; // Possible move functions, should follow the indexing of GuardianStates.



    // METHODS

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

    protected void FoVEntered(GameObject enterer)
    {
        if (enterer.CompareTag("Hero"))
        {
            Debug.Log(string.Format("{0} has sighted a hero!", this.gameObject));

            // Notify the hero it has been seen. 
            Hero perceivedHero = enterer.GetComponent<Hero>();
            if (perceivedHero != null)
                perceivedHero.FleeGuardian(this.gameObject);

            // Chase the hero.
            previousTarget = myTarget;
            previousState = myState;

            myTarget = enterer;
            myState = GuardianStates.PursueHero;
        }
    }
    protected void FoVExited(GameObject exiter)
    {
        if (exiter.CompareTag("Hero"))
        {
            Debug.Log(string.Format("{0} lost sight of a hero.", this.gameObject));

            // Notify the hero it is now safe. 
            Hero perceivedHero = exiter.GetComponent<Hero>();
            if (perceivedHero != null)
            {
                perceivedHero.StartCoroutine(perceivedHero.EscapedGuardianDelayed(gameObject));
            }

            // Stop chasing the hero.
            previousTarget = myTarget;
            previousState = myState;

            myTarget = myPrisoner;
            myState = GuardianStates.ReachPrisoner; // Have it remember its previous state instead?
        }
    }

    //protected void SneakZoneEntered(GameObject enterer)
    //{
    //    if (enterer.CompareTag("Hero"))
    //    {
    //        Debug.Log(string.Format("A hero is attempting to sneak past {0}.", this.gameObject));

    //        // Notify the hero it should sneak past the guard.
    //        Hero perceivedHero = enterer.GetComponent<Hero>();
    //        if (perceivedHero != null)
    //        {
    //            perceivedHero.SneakPastGuardian(gameObject);
    //        }
    //    }
    //}
    //protected void SneakZoneExited(GameObject exiter)
    //{
    //    if (exiter.CompareTag("Hero"))
    //    {
    //        Debug.Log(string.Format("{0} is no longer sneaking past {1}.", exiter, this.gameObject));

    //        // Notify the hero to stop sneaking.
    //        Hero perceivedHero = exiter.GetComponent<Hero>();
    //        if (perceivedHero != null)
    //        {
    //            perceivedHero.StopSneak(gameObject);
    //        }
    //    }
    //}

    protected void KillzoneEnter(GameObject enterer)
    {
        if (enterer == gameObject)
        {
            myState = GuardianStates.Stop;
        }
    }



    // Built in.
    void Start()
    {
        // Get components in children.
        myFoV = GetComponentInChildren<GuardianFoV>();
        //mySneakZone = GetComponentInChildren<GuardianSneakZone>();
        myCaptureZone = GetComponentInChildren<GuardianCaptureZone>();

        // Register the event listeners.
        FortressKillzone.OnKillZoneEnter += KillzoneEnter;
        OnTargetReached += TargetReached;
        myFoV.OnFoVEnter += FoVEntered;
        myFoV.OnFoVExit += FoVExited;
        //mySneakZone.OnSneakZoneEnter += SneakZoneEntered;
        //mySneakZone.OnSneakZoneExit += SneakZoneExited;


        // Furnish the possible move functions.
        moveFunctionsPerState = new Func<GameObject, Vector3>[]
        {
            // They are indexed in accordance with the GuardianStates enum.
            WanderSteer, // Wander around the prisoner; the prisoner's location itself is not used by the function.
            PursueSteer, // Pursue the hero. The target is the hero.
            ArriveSteer // Return to the prisoner. The target is the prisoner.
        };
    }

    void Update()
    {
        if (myState != GuardianStates.Stop)
        {
            // Return to the prisoner if we get too far away.
            if (myPrisoner != null)
            {
                if (myState == GuardianStates.Wander && Vector3.Distance(transform.position, myPrisoner.transform.position) > maxDistanceFromMyPrisoner)
                {
                    previousTarget = myTarget;

                    myTarget = myPrisoner;
                    myState = GuardianStates.ReachPrisoner;

                    Debug.Log("Prisoner too far!");
                }
                else if (myState == GuardianStates.ReachPrisoner && Vector3.Distance(transform.position, myPrisoner.transform.position) < maxDistanceFromMyPrisoner / 2)
                {
                    previousTarget = myTarget;

                    myState = GuardianStates.Wander;
                }
            }

            Move();
        }
    }
}
