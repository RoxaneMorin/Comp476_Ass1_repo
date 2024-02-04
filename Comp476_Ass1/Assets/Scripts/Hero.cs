using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hero : NPC
{
    public enum HeroStates
    {
        ReachPrisoner,
        ReachFortress,
        FleeGuardian,
        TauntGuardian
    }

    // VARIABLES.
    [Header("Hero Variables")]
    [SerializeField] protected GameObject myFortress;
    [SerializeField] protected Prisoner myFriendPrisoner;
    
    [Space]

    [SerializeField] private HeroStates myState = HeroStates.ReachPrisoner;
    [SerializeField] private HeroStates previousState = HeroStates.ReachPrisoner;
    //[SerializeField] private bool isSneaking = false;

    Func<GameObject, Vector3>[] moveFunctionsPerState; // Possible move functions, should follow the indexing of HeroStates.

    [Space]

    [SerializeField] private float stopEscapeTimeBuffer = 1f;
    private float defaultVelocity;



    // METHODS

    public void FleeGuardian(GameObject targetGuardian)
    {
        Debug.Log(string.Format("{0} is now fleeing the guardian {1}.", this, targetGuardian));

        // If we were guiding a prisoner, keep track of it.
        if (myFriendPrisoner != null)
        {
            myFriendPrisoner.StopRescue();
        }
        else
        {
            Prisoner targetPrisoner = myTarget.GetComponent<Prisoner>();
            if (targetPrisoner)
            {
                myFriendPrisoner = targetPrisoner;
            }
        }

        // Update my own stuff.
        previousTarget = myTarget;
        previousState = myState;

        if (!toAvoidDanger.Contains(targetGuardian))
        {
            toAvoidDanger.Add(targetGuardian);
        }

        myTarget = myFortress;
        myState = Hero.HeroStates.FleeGuardian;
    }
    public void EscapedGuardian(GameObject targetGuardian)
    {
        Debug.Log(string.Format("{0} is no longer fleeing.", this));

        previousTarget = myTarget;
        previousState = myState;

        // How to handle them being deleted?
        if (toAvoidDanger.Contains(targetGuardian)) // && !isSneaking)
        {
            toAvoidDanger.Remove(targetGuardian);
        }

        if (!GuardiansInToAvoid())
        {
            myTarget = myFriendPrisoner.gameObject;
            myState = Hero.HeroStates.ReachPrisoner;
        }        
    }

    public IEnumerator EscapedGuardianDelayed(GameObject targetGuardian)
    {
        yield return new WaitForSeconds(stopEscapeTimeBuffer);

        EscapedGuardian(targetGuardian);
    }

    // Handle via feelers?
    //public void SneakPastGuardian(GameObject targetGuardian)
    //{
    //    if (!isSneaking)
    //    {
    //        isSneaking = true;
    //    }

    //    if (!(myState == HeroStates.FleeGuardian && myTarget == targetGuardian)) // No need to avoid the guardian if the hero is already fleering them.
    //    {
    //        Debug.Log(string.Format("{0} is now sneaking past the guardian {1}.", this, targetGuardian));

    //        if (!toAvoidDanger.Contains(targetGuardian))
    //        {
    //            toAvoidDanger.Add(targetGuardian);
    //        }
    //    }
    //    else
    //        Debug.Log(string.Format("{0} is already fleeing {1}.", this, targetGuardian));
    //}
    //public void StopSneak(GameObject targetGuardian)
    //{
    //    Debug.Log(string.Format("{0} is no longer sneaking.", this));

    //    // Only remove the Guardian from the avoid list if we are not actively fleeing it.
    //    if (myState != HeroStates.FleeGuardian && toAvoidDanger.Contains(targetGuardian))
    //    {
    //        toAvoidDanger.Remove(targetGuardian);
    //    }

    //    // Verify that we don't have other guardians to sneak by.
    //    if (isSneaking)
    //    {
    //        isSneaking = false;
    //    }
    //}

    bool GuardiansInToAvoid()
    {
        foreach (GameObject potentialGuardian in toAvoidDanger)
        {
            // Getting the Guardian component would probably be safer, but more expensive.
            if (potentialGuardian.CompareTag("Guardian")) 
                return true;
        }
        return false;
    }


    // Movement

    protected void Move()
    {
        Vector3 desiredVelocity = moveFunctionsPerState[(int)myState](myTarget);

        Move(desiredVelocity);
    }


    // Event receivers.
    override protected void TargetReached(GameObject target)
    {
        base.TargetReached(target);

        // Have we reached a prisoner?
        if (target.CompareTag("Prisoner"))
        {
            Debug.Log(string.Format("{0} is attempting to rescue the prisoner {1}.", this.gameObject, target));

            // Update the prisoner's suff.
            Prisoner targetPrisoner = target.GetComponent<Prisoner>();
            if (targetPrisoner)
            {
                targetPrisoner.InitRescue(this, myFortress);
            }

            // Update the hero's own stuff.
            previousTarget = myTarget;
            previousState = myState;

            targetReached = false;

            myFriendPrisoner = targetPrisoner;
            myTarget = myFortress;
            myState = HeroStates.ReachFortress;
        }

        // Have we guided all attentive guardians to a fortress??
        //else if (target.CompareTag("Fortress"))
        //{
        //    isSneaking = false;
        //    targetReached = false;
        //}
    }

    void GuardianDestroyed(GameObject dyingGardianGO)
    {
        // Check if the guardian was in our list of obstacles to be avoided.
        if (toAvoidDanger.Contains(dyingGardianGO))
        {
            toAvoidDanger.Remove(dyingGardianGO);

            // Verify this works with multiple guardians.
            if (myState == HeroStates.FleeGuardian)
            {
                previousTarget = myTarget;
                previousState = myState;

                myTarget = myFriendPrisoner.gameObject;
                myState = Hero.HeroStates.ReachPrisoner;

                // Was this the last Guardian alive?
                if (!GuardiansInToAvoid())
                {
                    // isSneaking = false;
                    targetReached = false;
                }
            }
        }
    }


    // Built in.

    void Start()
    {
        // Dynamic variables.
        defaultVelocity = maxVelocity;

        // Register the event listeners.
        OnTargetReached += TargetReached;
        FortressKillzone.OnKillZoneEnter += GuardianDestroyed;

        // Furnish the possible move functions.
        moveFunctionsPerState = new Func<GameObject, Vector3>[]
        {
            // They are indexed in accordance with the HeroState enum.
            ArriveSteer,
            SeekSteer,
            SeekSteer,
            ArriveSteer
        };
    }

    void Update()
    {
        Move();
    }
}
