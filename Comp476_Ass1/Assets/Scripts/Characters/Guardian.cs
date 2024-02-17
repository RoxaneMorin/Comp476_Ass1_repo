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
    [SerializeField] public FoV myFoV;
    [SerializeField] protected HeroKillZone myCaptureZone;
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
    override protected void MyTargetReached(GameObject target)
    {
        base.MyTargetReached(target);
    }

    protected void FoVEntered(GameObject enterer)
    {
        //Debug.Log(string.Format("{0} has sighted {1}.", this.gameObject, enterer));

        if (enterer.CompareTag("Hero"))
        {
            Debug.Log(string.Format("{0} has sighted a hero, {1}.", this.gameObject, enterer));

            // Notify the hero it has been seen. 
            Hero perceivedHero = enterer.GetComponent<Hero>();
            if (perceivedHero != null)
            {
                perceivedHero.EnterFleeGuardian(this.gameObject);
            }

            // Chase the hero.
            previousTarget = myTarget;
            previousState = myState;

            myTarget = enterer;
            myState = GuardianStates.PursueHero;
        }
    }
    protected void FoVExited(GameObject exiter)
    {
        if (exiter.CompareTag("Hero")) // && !nextWaypoint) // Avoid losing track of the hero between walls.
        {
            Debug.Log(string.Format("{0} lost sight of a hero.", this.gameObject));

            // Notify the hero it is now safe. 
            Hero perceivedHero = exiter.GetComponent<Hero>();
            if (perceivedHero != null)
            {
                perceivedHero.StartCoroutine(perceivedHero.ExitFleeGuardianDelayed(gameObject));
            }

            // Stop chasing the hero.
            previousTarget = myTarget;
            previousState = myState;

            // Clear waypoiint information.
            ClearWaypointInfo();

            myTarget = myPrisoner;
            myState = GuardianStates.ReachPrisoner; // Have it remember its previous state instead?
        }
    }

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
        // Dynamic variables.
        defaultVelocity = maxVelocity;

        // Get components in children.
        myFoV = GetComponentInChildren<FoV>();
        myCaptureZone = GetComponentInChildren<HeroKillZone>();

        // Register the event listeners.
        GuardianKillZone.OnGuardianKillZoneEnter += KillzoneEnter;
        OnTargetReached += MyTargetReached;
        myFoV.OnFoVEnter += FoVEntered;
        myFoV.OnFoVExit += FoVExited;

        // Furnish the possible move functions.
        moveFunctionsPerState = new Func<GameObject, Vector3>[]
        {
            // They are indexed in accordance with the GuardianStates enum.
            WanderSteer, // Wander around the prisoner; the prisoner's location itself is not used by the function.
            PursueSteer, // Pursue the hero. The target is the hero.
            ArriveSteer // Return to the prisoner. The target is the prisoner.
        };


        // Find myPrisoner.
        Prisoner potentialPrisoner = FindClosestPrisoner();
        if (potentialPrisoner)
        {
            myPrisoner = potentialPrisoner.gameObject;
        }
        
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

                    if (!myPrisoner.activeSelf)
                    {
                        Prisoner closestPrisoner = FindClosestPrisoner();
                        if (closestPrisoner)
                        {
                            myPrisoner = FindClosestPrisoner().gameObject;
                        }
                        else myPrisoner = null;
                    }

                    myTarget = myPrisoner;
                    myState = GuardianStates.ReachPrisoner;

                    // Debug.Log(string.Format("{0}'s prisoner ({1}) is too far away!", gameObject, myPrisoner));
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
