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
    [SerializeField] protected Player myFriendPlayer;

    [Space]

    [SerializeField] private PrisonerStates myState = PrisonerStates.Wait;
    public PrisonerStates MyState { get { return myState; }}
    [SerializeField] private PrisonerStates previousState = PrisonerStates.Wait;
    [SerializeField] private bool isBusy = false;
    public bool IsBusy { get { return isBusy; } }

    Func<GameObject, Vector3>[] moveFunctionsPerState; // Possible move functions, should follow the indexing of PrisonerStates.



    // METHODS

    // State changes.
    public void InitRescue(Hero rescuer, GameObject targetFortress)
    {
        if (myFriendPlayer == null)
        {
            Debug.Log(string.Format("{0} is being rescued by {1}.", gameObject, rescuer.gameObject));

            previousTarget = myTarget;
            previousState = myState;

            myFriendHero = rescuer;
            myTarget = myFriendHero.gameObject;
            myState = Prisoner.PrisonerStates.ReachFortress;
            isBusy = true;
        }
    }
    public void StopRescue()
    {
        if (myFriendPlayer == null)
        {
            Debug.Log(string.Format("{0} is no longer being rescue.", gameObject));

            previousTarget = myTarget;
            previousState = myState;

            myFriendHero = null;
            myState = PrisonerStates.Wait;
            isBusy = false;
        }
    }

    public void InitFollowPlayer(GameObject thePlayer)
    {
        previousTarget = myTarget;
        previousState = myState;
        myFriendHero = null;

        isBusy = true;
        myTarget = thePlayer;
        myFriendPlayer = thePlayer.GetComponent<Player>();
        myState = PrisonerStates.FollowPlayer;
    }


    // Movement
    protected void Move()
    {
         //Simple as we don't have much to do beside obstacle avoidance.
        Vector3 desiredVelocity = moveFunctionsPerState[(int)myState](myTarget);

        Move(desiredVelocity);
    }


    // Utility
    public bool GuardiansNearby(float nearbyRadius, GameObject hero)
    {
        // Spherecast around the prisoner.
        Collider[] otherColliders = Physics.OverlapSphere(gameObject.transform.position, nearbyRadius);
        //Debug.DrawWireSphere(transform.position, nearbyRadius);

        foreach (Collider collider in otherColliders)
        {
            if (collider.gameObject.CompareTag("Guardian") || collider.gameObject.CompareTag("Player") || collider.gameObject.CompareTag("FoV"))
            {
                return true;
            }
        }

        // Raycast between hero and prisoner.
        Ray ray = new Ray(transform.position, hero.transform.position - transform.position);
        float maxDistance = Vector3.Distance(transform.position, hero.transform.position);
        RaycastHit[] hits = Physics.RaycastAll(ray, maxDistance);
        Debug.DrawRay(transform.position, hero.transform.position);

        foreach (RaycastHit hit in hits)
        {
            //Debug.Log(hit.collider.gameObject);
            if (hit.collider.gameObject.CompareTag("Guardian") || hit.collider.gameObject.CompareTag("Player") || hit.collider.gameObject.CompareTag("FoV"))
            {
                return true;
            }
        }

        return false;
    }


    // Event receivers.
    override protected void MyTargetReached(GameObject target)
    {
        // base.MyTargetReached(target);
        //targetReached = true;

        if (target.CompareTag("Fortress") && myFriendHero)
        {
            myFriendHero.TargetReached = false;
        }
    }


    // Built in
    void Start()
    {
        // Dynamic variables.
        defaultVelocity = maxVelocity;

        // Register the event listeners.
        this.OnTargetReached += MyTargetReached;

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
        if (myState != PrisonerStates.Wait && !targetReached)
        {
            Move();
        }
    }
}
