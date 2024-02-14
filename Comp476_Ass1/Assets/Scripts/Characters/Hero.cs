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
        TauntGuardian,
        FleePlayer
    }

    // VARIABLES.
    [Header("Hero Variables")]
    [SerializeField] private bool respawns = false;
    public bool Respawns { get { return respawns; } }
    [SerializeField] private bool huntGuardians = true;
    [SerializeField] private FoV myFoV;
    [SerializeField] private GameObject myFortress;
    [SerializeField] private Prisoner myFriendPrisoner;
    public Prisoner MyFriendPrisoner { get { return myFriendPrisoner; } }
    
    [Space]

    [SerializeField] private HeroStates myState = HeroStates.ReachPrisoner;
    public HeroStates MyState { get { return myState; }}
    [SerializeField] private HeroStates previousState = HeroStates.ReachPrisoner;

    Func<GameObject, Vector3>[] moveFunctionsPerState; // Possible move functions, should follow the indexing of HeroStates.

    [Space]

    [SerializeField] private float stopEscapeTimeBuffer = 1f;
    [SerializeField] private float findNewGuardianTimeBuffer = 10f;
    [SerializeField] private float guardianHuntingVelocity = 3f;

    [Space]

    [SerializeField] private float guardianTrailingWeightIncrease = 0.5f;
    [SerializeField] private float tailFeelerLengthDivider = 3f;
    [SerializeField] private bool guardianOnMyTrail = false;



    // METHODS

    public void InitReachPrisoner()
    {
        if (!myFriendPrisoner || !myFriendPrisoner.isActiveAndEnabled)
        {
            myFriendPrisoner = FindClosestPrisoner(true);
        }

        if (myFriendPrisoner && !MyFriendPrisoner.IsBusy)
        {
            myTarget = myFriendPrisoner.gameObject;
            myState = HeroStates.ReachPrisoner;
        }
    }

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

        ClearWaypointInfo();

        // How to handle them being deleted?
        if (toAvoidActive.Contains(targetGuardian))
        {
            toAvoidActive.Remove(targetGuardian);
        }

        if (!GuardiansInToAvoid())
        {
            Debug.Log("No longer fleeing anyone");

            if ((previousState == HeroStates.ReachPrisoner || previousState == HeroStates.ReachFortress) && myFriendPrisoner.isActiveAndEnabled)
            {
                InitReachPrisoner();
            }
            else if (previousState == HeroStates.FleeGuardian)
            {
                // A closer guardian may be present.
                targetGuardian = null;
                HuntForGuardian(true);
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

    Guardian FindClosestGuardianByFoV()
    {
        GameObject[] potentialGuardianFoVs = GameObject.FindGameObjectsWithTag("FoV");

        if (potentialGuardianFoVs.Length == 1)
        {
            FoV potentialGuardianFoV = potentialGuardianFoVs[0].GetComponent<FoV>();
            Guardian potentialGuardian = potentialGuardianFoV.GetComponentInParent<Guardian>();

            return potentialGuardian;
        }
        else if (potentialGuardianFoVs.Length > 1)
        {
            float closestDistance = Mathf.Infinity;
            GameObject closestPotentialGuardianFoV = null;

            foreach (GameObject potentialGuardianFoV in potentialGuardianFoVs)
            {
                float distance = Vector3.Distance(gameObject.transform.position, potentialGuardianFoV.transform.position);
                if (distance < closestDistance && potentialGuardianFoV.activeInHierarchy == true)
                {
                    closestDistance = distance;
                    closestPotentialGuardianFoV = potentialGuardianFoV;
                }
            }
            return closestPotentialGuardianFoV.GetComponentInParent<Guardian>();
        }
        else
            return null;
    }
    public void ClearFlee(bool ignoreGuardians = false)
    {
        if (ignoreGuardians || !GuardiansInToAvoid())
        {
            targetReached = false;
            ClearWaypointInfo();
            HuntForGuardian(true);
        }
    }

    public void HuntForGuardian(bool retarget = false)
    {
        if (huntGuardians)
        {
            Guardian targetGuardian = null;
            if (myTarget)
            {
                targetGuardian = myTarget.GetComponent<Guardian>();
            }

            if (retarget || targetGuardian == null || !targetGuardian.gameObject.activeInHierarchy)
            {
                // Debug.Log(string.Format("{0} is hunting for a Guardian...", gameObject));
                targetGuardian = FindClosestGuardianByFoV();
            }

            if (targetGuardian)
            {
                Debug.Log(string.Format("{0} is attempting to lure {1} to its doom...", gameObject, targetGuardian.gameObject));

                myState = HeroStates.TauntGuardian;
                myTarget = targetGuardian.myFoV.gameObject;

                Invoke("HuntForGuardianRetry", findNewGuardianTimeBuffer);
            }
            else
            {
                // Move on to rescuing the prisoner.
                InitReachPrisoner();
            }
        }
        else
        {
            // Move on to rescuing the prisoner.
            InitReachPrisoner();
        }
        
    }
    public IEnumerator HuntForGuardianDelayed()
    {
        yield return new WaitForSeconds(findNewGuardianTimeBuffer);

        HuntForGuardian(true);
    }
    public void HuntForGuardianRetry()
    {
        //Debug.Log("In HuntForGuardianRetry.");
        if (myState == HeroStates.TauntGuardian)
        {
            HuntForGuardian(true);
        }
    }

    protected bool CastBackFeeler()
    {
        // To do: try to make these a fan.
        
        float tailFeelerDistance = feelersDistance / tailFeelerLengthDivider;
        Ray ray = new Ray(transform.position, -transform.forward);
        RaycastHit hit;

        // Visualize their little feelers uwu
        Debug.DrawRay(ray.origin, -transform.forward * (tailFeelerDistance), Color.white);

        if (Physics.Raycast(ray, out hit, tailFeelerDistance))
        {
            // Check if the thing hit is a guardian's capture zone.
            if (hit.collider.gameObject.CompareTag("GuardianCaptureZone"))
            {
                // Debug.Log(string.Format("A guardian is closely trailing {0}.", gameObject));
                return true;
            }
        }
        // else,
        return false;
    }
    protected void TryHaste()
    {
        bool isBeingTrailed = CastBackFeeler();

        if (isBeingTrailed && !guardianOnMyTrail)
        {
            guardianOnMyTrail = true;
            toAvoidActiveWeight += guardianTrailingWeightIncrease;
            maxVelocity += guardianTrailingWeightIncrease;
        }
        else if (!isBeingTrailed && guardianOnMyTrail)
        {
            guardianOnMyTrail = false;
            toAvoidActiveWeight -= guardianTrailingWeightIncrease;
            maxVelocity -= guardianTrailingWeightIncrease;
        }
    }


    // Movement

    protected void Move()
    {
        Vector3 desiredVelocity = moveFunctionsPerState[(int)myState](myTarget);
        Move(desiredVelocity, true, (myState != HeroStates.TauntGuardian && myState != HeroStates.FleeGuardian));
    }


    // Event receivers.
    override protected void MyTargetReached(GameObject target)
    {
        // base.MyTargetReached(target);

        Debug.Log(string.Format("The hero reached its target, {0}.", target));

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

            myFriendPrisoner = targetPrisoner;
            myTarget = myFortress;
            myState = HeroStates.ReachFortress;
        }
        if (target.CompareTag("Fortress") && myState == HeroStates.ReachFortress)
        {
            myState = HeroStates.TauntGuardian;
            HuntForGuardian(true);
        }

        targetReached = false;
    }

    protected void FoVEntered(GameObject enterer)
    {
        if (enterer.CompareTag("Player"))
        {
            Debug.Log(string.Format("{0} has sighted a player!", this.gameObject));

            // Save my previous information.
            previousTarget = myTarget;
            previousState = myState;

            // Clear waypoint information.
            ClearWaypointInfo();

            // Update state.
            myTarget = enterer;
            myState = HeroStates.FleePlayer;
        }
    }
    protected void FoVExited(GameObject exiter)
    {
        if (exiter.CompareTag("Player"))
        {
            StartCoroutine(EscapedPlayerDelayed(exiter));
        }
            
    }
    protected void FoVExitedDelayed(GameObject exiter)
    {
        Debug.Log(string.Format("{0} lost sight of the player.", this.gameObject));

        // Save my previous information.
        GameObject tempTarget = myTarget;
        HeroStates tempState = myState;

        // Update state.
        myTarget = previousTarget;
        myState = previousState;

        // Waypoints, next state, etc. handled in ClearFlee.
        ClearFlee();

        previousTarget = tempTarget;
        previousState = tempState;
    }
    public IEnumerator EscapedPlayerDelayed(GameObject player)
    {
        yield return new WaitForSeconds(stopEscapeTimeBuffer);

        FoVExitedDelayed(player);
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

    void HeroKilledByEnv(GameObject killedHeroGO)
    {
        if (killedHeroGO == this.gameObject)
        {
            if (myFriendPrisoner != null)
            {
                myFriendPrisoner.StopRescue();
            }

            if (respawns)
            {
                GameObject deathParticles = Instantiate(myDeathParticleSystem, transform.position, Quaternion.identity);

                doMovement = false;
                Invoke("Respawn", 0.5f);
            }
            else
            {
                this.gameObject.SetActive(false);
            }
        }
    }
    void HeroKilledByPlayer(GameObject killedHeroGO, bool dieForever)
    {
        if (killedHeroGO == this.gameObject)
        {
            if (myFriendPrisoner != null)
            {
                myFriendPrisoner.StopRescue();
            }
            // Might need to handle the prisoner differently.

            if (!dieForever)
            {
                GameObject deathParticles = Instantiate(myDeathParticleSystem, transform.position, Quaternion.identity);

                doMovement = false;
                Invoke("Respawn", 0.5f);
            }
            else
            {
                this.gameObject.SetActive(false);
            }
        }
    }
    private void Respawn()
    {
        gameObject.transform.position = myFortress.transform.position;
        doMovement = true;

        ClearFlee();
        HuntForGuardian(true);
    }

    void PrisonerReachedFortress(GameObject dummy)
    {
        // huntGuardians = true;
        myState = HeroStates.TauntGuardian;
        HuntForGuardian(true);
    }


    // Built in.

    void Start()
    {
        // Dynamic variables.
        defaultVelocity = maxVelocity;

        // Get components in children.
        myFoV = GetComponentInChildren<FoV>();

        // Register the event listeners.
        OnTargetReached += MyTargetReached;
        Player.OnPlayerCaughtHero += HeroKilledByPlayer;
        HeroKillZone.OnHeroKillZoneEnter += HeroKilledByEnv;
        GuardianKillZone.OnGuardianKillZoneEnter += GuardianDestroyed;
        Fortress.OnPrisonerFortressEnter += PrisonerReachedFortress;
        if (myFoV)
        {
            myFoV.OnFoVEnter += FoVEntered;
            myFoV.OnFoVExit += FoVExited;
        }

        // Furnish the possible move functions.
        moveFunctionsPerState = new Func<GameObject, Vector3>[]
        {
            // They are indexed in accordance with the HeroState enum.
            ArriveSteer,
            SeekSteer,
            SeekSteer,
            ArriveSteer,
            FleeSteer
        };

        // Find closest Prisoner.
        myFriendPrisoner = FindClosestPrisoner(true);

        // Find closest Guardian.
        myState = HeroStates.TauntGuardian;
        HuntForGuardian(true);
    }

    void Update()
    {
        // Don't move if my target is null.
        if (myTarget)
        {
            TryHaste();
            Move();
        }
        
    }
}
