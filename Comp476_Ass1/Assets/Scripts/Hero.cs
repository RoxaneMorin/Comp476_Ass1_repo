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
    [SerializeField] private float guardianHuntingVelocity = 3f;



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

        if (!toAvoidActive.Contains(targetGuardian))
        {
            toAvoidActive.Add(targetGuardian);
        }

        myTarget = myFortress;
        myState = Hero.HeroStates.FleeGuardian;

        maxVelocity = guardianHuntingVelocity;
    }
    public void EscapedGuardian(GameObject targetGuardian)
    {
        Debug.Log(string.Format("{0} is no longer fleeing {1}.", this, targetGuardian));

        GameObject tempTarget = myTarget;
        HeroStates tempState = myState;

        // How to handle them being deleted?
        if (toAvoidActive.Contains(targetGuardian))
        {
            toAvoidActive.Remove(targetGuardian);
        }

        if (!GuardiansInToAvoid())
        {
            Debug.Log("No longer fleeing anyone");

            if (previousState == HeroStates.ReachPrisoner || previousState == HeroStates.ReachFortress)
            {
                myTarget = myFriendPrisoner.gameObject;
                myState = Hero.HeroStates.ReachPrisoner;
            }
            else if (previousState == HeroStates.FleeGuardian)
            {
                HuntForGuardian();
            }
            else
            {
                myTarget = previousTarget;
                myState = previousState;
            }

            previousTarget = tempTarget;
            previousState = tempState;

            maxVelocity = defaultVelocity;
        }
    }

    public IEnumerator EscapedGuardianDelayed(GameObject targetGuardian)
    {
        yield return new WaitForSeconds(stopEscapeTimeBuffer);

        EscapedGuardian(targetGuardian);
    }

    bool GuardiansInToAvoid()
    {
        foreach (GameObject potentialGuardian in toAvoidActive)
        {
            // Getting the Guardian component would probably be safer, but more expensive.
            if (potentialGuardian.CompareTag("Guardian")) 
                return true;
        }
        return false;
    }

    Guardian FindClosestGuardian()
    {
        GameObject[] potentialGuardians = GameObject.FindGameObjectsWithTag("Guardian");

        if (potentialGuardians.Length == 1)
        {
            return potentialGuardians[0].GetComponent<Guardian>();
        }
        else if (potentialGuardians.Length > 1)
        {
            float closestDistance = Mathf.Infinity;
            GameObject closestPotentialGuardian = null;

            foreach (GameObject potentialGuardian in potentialGuardians)
            {
                float distance = Vector3.Distance(gameObject.transform.position, potentialGuardian.transform.position);
                if (distance < closestDistance && potentialGuardian.activeInHierarchy == true)
                {
                    closestDistance = distance;
                    closestPotentialGuardian = potentialGuardian;
                }
            }
            return closestPotentialGuardian.GetComponent<Guardian>();
        }
        else
            return null;
    }

    public void HuntForGuardian()
    {
        Guardian targetGuardian = myTarget.GetComponent<Guardian>();

        if (!targetGuardian || !targetGuardian.gameObject.activeInHierarchy)
        {
            // Debug.Log(string.Format("{0} is hunting for a Guardian...", gameObject));
            targetGuardian = FindClosestGuardian();
        }

        if (targetGuardian)
        {
            Debug.Log(string.Format("{0} is attempting to lure {1} to its doom...", gameObject, targetGuardian.gameObject));

            myState = HeroStates.TauntGuardian;
            myTarget = targetGuardian.myFoV.gameObject;
        }
        else // Move on to rescuing the prisoner.
        {
            if (!myFriendPrisoner)
            {
                myFriendPrisoner = FindClosestPrisoner();
            }
            myTarget = myFriendPrisoner.gameObject;
            myState = HeroStates.ReachPrisoner;
        }
    }
    public IEnumerator HuntForGuardianDelayed()
    {
        yield return new WaitForSeconds(stopEscapeTimeBuffer);

        HuntForGuardian();
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

        else if (target.CompareTag("Fortress"))
        {
            targetReached = false;
        }
    }

    void GuardianDestroyed(GameObject dyingGardianGO)
    {
        // Check if the guardian was in our list of obstacles to be avoided.
        if (toAvoidActive.Contains(dyingGardianGO))
        {
            toAvoidActive.Remove(dyingGardianGO);

            // Verify this works with multiple guardians.
            if (myState == HeroStates.FleeGuardian || myState == HeroStates.TauntGuardian)
            {
                targetReached = false;
                maxVelocity = defaultVelocity;

                StartCoroutine(HuntForGuardianDelayed());
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

        // Find closest Prisoner.
        myFriendPrisoner = FindClosestPrisoner();

        // Find closest Guardian.
        if (myState == HeroStates.TauntGuardian)
        {
            HuntForGuardian();
        }
    }

    void Update()
    {
        Move();
    }
}
