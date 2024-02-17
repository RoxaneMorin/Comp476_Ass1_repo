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

    public enum HeroRelationshipToGuardians
    {
        NeverHunt,
        HuntIfClustered,
        AlwaysHunt
    }

    // VARIABLES.
    [Header("Hero Variables")]
    [SerializeField] private bool respawns = false;
    public bool Respawns { get { return respawns; } }
    [SerializeField] private HeroRelationshipToGuardians guardianHuntingBehaviour = HeroRelationshipToGuardians.AlwaysHunt;
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
    [SerializeField] private float guardiansNearPrisonerRadius = 5f;

    [Space]

    [SerializeField] private float guardianTrailingWeightIncrease = 0.5f;
    [SerializeField] private float tailFeelerLengthDivider = 3f;
    [SerializeField] private bool guardianOnMyTrail = false;

    [Space]

    [SerializeField] private float maintenanceIntervals = 20f;



    // METHODS

    // State changes.
    public void EnterReachPrisoner(bool retarget = false)
    {
        if (retarget || !myFriendPrisoner || !(myFriendPrisoner.isActiveAndEnabled) || myFriendPrisoner.IsBusy)
        {
            myFriendPrisoner = FindClosestPrisoner(true);
        }

        if (myFriendPrisoner && !MyFriendPrisoner.IsBusy)
        {
            myTarget = myFriendPrisoner.gameObject;
            myState = HeroStates.ReachPrisoner;
        }
        else // Else, no prisoners are available. Enter taunt guardian.
        {
            Guardian potentialClosestGuardian = FindClosestGuardianByFoV();
            if (potentialClosestGuardian)
            {
                EnterTauntGuardianDelayed();
            }
        }
    }

    public void EnterFleeGuardian(GameObject targetGuardian)
    {
        Debug.Log(string.Format("{0} is now fleeing the guardian {1}.", this, targetGuardian));

        // If we were guiding a prisoner, notify it.
        if (myFriendPrisoner != null && myFriendPrisoner.isActiveAndEnabled && myFriendPrisoner.MyState == Prisoner.PrisonerStates.ReachFortress)
        {

            myFriendPrisoner.StopRescue();
            myFriendPrisoner = null;
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
    public void ExitFleeGuardian(GameObject targetGuardian)
    {
        Debug.Log(string.Format("{0} is no longer fleeing {1}.", this, targetGuardian));

        GameObject tempTarget = myTarget;
        HeroStates tempState = myState;

        //ClearWaypointInfo();

        // How to handle them being deleted?
        if (toAvoidActive.Contains(targetGuardian))
        {
            toAvoidActive.Remove(targetGuardian);
        }

        if (!GuardiansInToAvoid())
        {
            Debug.Log(string.Format("{0} is no longer fleeing anyone.", this.gameObject));

            previousTarget = tempTarget;
            previousState = tempState;

            maxVelocity = defaultVelocity;

            AssignHuntOrRescue();
        }
        else
        {
            Debug.Log(string.Format("{0} is still being pursued.", this.gameObject));
        }
    }

    public IEnumerator ExitFleeGuardianDelayed(GameObject targetGuardian)
    {
        yield return new WaitForSeconds(stopEscapeTimeBuffer);

        ExitFleeGuardian(targetGuardian);
    }

    public void AssignHuntOrRescue(bool retarget = false)
    {
        switch (guardianHuntingBehaviour)
        {
            case HeroRelationshipToGuardians.NeverHunt:
                EnterReachPrisoner();
                break;

            case HeroRelationshipToGuardians.HuntIfClustered:
                myFriendPrisoner = FindClosestPrisoner(true);
                if (FriendPrisonerSurrounded())
                {
                    Debug.Log(string.Format("{0}'s target, {1}, is surrounded by guardians. They will first try to clean those away...", this.gameObject, myTarget));
                    EnterTauntGuardian(retarget);
                }
                else
                {
                    Debug.Log(string.Format("{0} is making a beeline for {1}.", this.gameObject, myFriendPrisoner));
                    EnterReachPrisoner(false);
                }
                break;

            case HeroRelationshipToGuardians.AlwaysHunt:
                EnterTauntGuardian(retarget);
                break;
        }
    }
    public IEnumerator AssignHuntOrRescueDelayed(bool retarget = false)
    {
        yield return new WaitForSeconds(findNewGuardianTimeBuffer);

        AssignHuntOrRescue(retarget);
    }
    public void RetryAssignHuntOrRescue()
    {
        if (myState == HeroStates.TauntGuardian && myTarget != myFortress)
        {
            AssignHuntOrRescue(true);
            // EnterTauntGuardian(true);
        }
    }

    public void EnterTauntGuardian(bool retarget = false)
    {
        Guardian targetGuardian = null;
        if (myTarget)
        {
            targetGuardian = myTarget.GetComponent<Guardian>();
        }

        if (retarget || !targetGuardian || !targetGuardian.gameObject.activeInHierarchy)
        {
            targetGuardian = FindClosestGuardianByFoV();
        }

        if (targetGuardian != null && targetGuardian.isActiveAndEnabled)
        {
            Debug.Log(string.Format("{0} is attempting to lure {1} to its doom...", gameObject, targetGuardian.gameObject));

            myState = HeroStates.TauntGuardian;
            myTarget = targetGuardian.myFoV.gameObject;

            Invoke("RetryAssignHuntOrRescue", findNewGuardianTimeBuffer);
        }
        else // No guardians are left. Move on to rescuing the prisoner regardless of state.
        {
            Prisoner potentialPrisoner = FindClosestPrisoner();   
            if (potentialPrisoner)
            {
                EnterReachPrisoner();
            }
        }
    }
    public IEnumerator EnterTauntGuardianDelayed()
    {
        yield return new WaitForSeconds(findNewGuardianTimeBuffer);

        EnterTauntGuardian(true);
    }


    // Utility.
    bool FriendPrisonerSurrounded()
    {
        if (myFriendPrisoner != null && myFriendPrisoner.isActiveAndEnabled)
        {
            return myFriendPrisoner.GuardiansNearby(guardiansNearPrisonerRadius, gameObject);
        }
        else return false;
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
            AssignHuntOrRescue(true);
        }
    }

    protected bool CastBackFeeler()
    {
        float tailFeelerDistance = feelersDistance / tailFeelerLengthDivider;

        for (int i = 0; i < feelersCount; i++)
        {
            Vector3 currentDirection = Quaternion.Euler(0, i * 45, 0) * transform.forward;
            Ray ray = new Ray(transform.position, currentDirection);
            RaycastHit hit;

            // Visualize their little feelers uwu
            Debug.DrawRay(ray.origin, currentDirection * (tailFeelerDistance), Color.blue);

            if (Physics.Raycast(ray, out hit, tailFeelerDistance))
            {
                // Check if the thing hit is a guardian's capture zone.
                if (hit.collider.gameObject.CompareTag("GuardianCaptureZone"))
                {
                    // Debug.Log(string.Format("A guardian is closely trailing {0}.", gameObject));
                    return true;
                }
            }
        }
        // else,
        return false;
    }


    // Movement
    protected void Move()
    {
        Vector3 desiredVelocity = moveFunctionsPerState[(int)myState](myTarget);
        Move(desiredVelocity, true, (myState != HeroStates.TauntGuardian && myState != HeroStates.FleeGuardian));
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

    IEnumerator DoMaintenance()
    {
        while (true)
        {
            //Debug.Log(string.Format("{0} doing maintenance.", gameObject));

            // In FoV, but not detected?
            if (myState == HeroStates.TauntGuardian)
            {
                Collider[] colliders = Physics.OverlapSphere(transform.position, 0.5f);
                foreach (Collider collider in colliders)
                {
                    if (collider.gameObject.CompareTag("FoV"))
                    {
                        FoV potentialFoV = collider.gameObject.GetComponent<FoV>();
                        if (potentialFoV && potentialFoV.MyOwner.gameObject.CompareTag("Guardian"))
                        {
                            EnterFleeGuardian(potentialFoV.MyOwner.gameObject);
                            break;
                        }
                    }
                }
            }

            // Clear stuck waypoints.
            if (isCrossing && currentVelocity.magnitude < 0.5f) // Find a better check.
            {
                ClearWaypointInfo();
            }

            // Clear stuck at base.
            if (myState == HeroStates.ReachFortress && myTarget == myFortress && currentVelocity.magnitude < 0.5f)
            {
                AssignHuntOrRescue();
            }

            yield return new WaitForSeconds(maintenanceIntervals);
        }
    }


    // Event receivers.
    override protected void MyTargetReached(GameObject target)
    {
        // base.MyTargetReached(target);

        //Debug.Log(string.Format("{0} reached its target, {1}.", gameObject, target));

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
            AssignHuntOrRescueDelayed(true);
        }
        else
        {
            targetReached = false;
        }
    }

    protected void PlayerFoVEntered(GameObject enterer)
    {
        if (enterer.CompareTag("Player"))
        {
            Debug.Log(string.Format("{0} has sighted a player!", this.gameObject));

            // If we were guiding a prisoner, notify it.
            if (myFriendPrisoner != null && myFriendPrisoner.isActiveAndEnabled && myFriendPrisoner.MyState == Prisoner.PrisonerStates.ReachFortress)
            {

                myFriendPrisoner.StopRescue();
                myFriendPrisoner = null;
            }

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
    protected void PlayerFoVExited(GameObject exiter)
    {
        if (exiter.CompareTag("Player"))
        {
            StartCoroutine(EscapedPlayerDelayed(exiter));
        }
            
    }
    protected void PlayerFoVExitedDelayed(GameObject exiter)
    {
        Debug.Log(string.Format("{0} lost sight of the player.", this.gameObject));

        // Save my previous information.
        GameObject tempTarget = myTarget;
        HeroStates tempState = myState;

        // Update state.
        myTarget = previousTarget;
        myState = previousState;

        // Waypoints, next state, etc. handled in ClearFlee.
        ClearFlee(false);

        previousTarget = tempTarget;
        previousState = tempState;
    }
    public IEnumerator EscapedPlayerDelayed(GameObject player)
    {
        yield return new WaitForSeconds(stopEscapeTimeBuffer * 2);

        PlayerFoVExitedDelayed(player);
    }

    void GuardianDestroyed(GameObject dyingGardianGO)
    {
        // Check if the guardian was in our list of obstacles to be avoided.
        if (toAvoidActive.Contains(dyingGardianGO))
        {
            toAvoidActive.Remove(dyingGardianGO);

            if (!GuardiansInToAvoid() && (myState == HeroStates.FleeGuardian || myState == HeroStates.ReachFortress))
            {
                targetReached = false;
                maxVelocity = defaultVelocity;

                AssignHuntOrRescueDelayed(true);
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
                myFriendPrisoner = null;
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
                myFriendPrisoner = null;
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
        AssignHuntOrRescue(true);
    }

    void PrisonerReachedFortress(GameObject prisonerGO)
    {
        Prisoner potentialPrisoner = prisonerGO.GetComponent<Prisoner>();
        if (potentialPrisoner == myFriendPrisoner && myState == HeroStates.ReachFortress)
        {
            AssignHuntOrRescue(true);
        }
    }


    // Built in.
    void Start()
    {
        // Dynamic variables.
        defaultVelocity = maxVelocity;

        // Get components in children.
        myFoV = GetComponentInChildren<FoV>();

        // Register the event listeners.
        this.OnTargetReached += MyTargetReached;
        Player.OnPlayerCaughtHero += HeroKilledByPlayer;
        HeroKillZone.OnHeroKillZoneEnter += HeroKilledByEnv;
        GuardianKillZone.OnGuardianKillZoneEnter += GuardianDestroyed;
        Fortress.OnPrisonerFortressEnter += PrisonerReachedFortress;
        if (myFoV)
        {
            myFoV.OnFoVEnter += PlayerFoVEntered;
            myFoV.OnFoVExit += PlayerFoVExited;
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

        // Find target prisoner, see if we need to hunt guardians before rescuing it.
        myFriendPrisoner = FindClosestPrisoner(true);
        AssignHuntOrRescue(true);

        StartCoroutine(DoMaintenance());
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
