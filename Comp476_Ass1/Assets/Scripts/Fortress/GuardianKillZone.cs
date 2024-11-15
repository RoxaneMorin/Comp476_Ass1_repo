using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuardianKillZone : MonoBehaviour
{
    // VARIABLES
    [SerializeField] protected Fortress myFortress; // Probably not necessary.



    // EVENTS
    public delegate void EventKillZoneOnEnter(GameObject go);
    public static event EventKillZoneOnEnter OnGuardianKillZoneEnter;
    public static event EventKillZoneOnEnter OnPlayerKillZoneEnter;



    // METHODS

    // Built in.
    private void Start()
    {
        myFortress = GetComponentInParent<Fortress>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // Debug.Log(string.Format("{0} entered {1}.", other.gameObject, this));

        Guardian caughtGuardian = other.gameObject.GetComponent<Guardian>();
        if (caughtGuardian)
        {
            OnGuardianKillZoneEnter?.Invoke(other.gameObject);

            Hero guardiansTarget = caughtGuardian.myTarget.GetComponent<Hero>();

            other.gameObject.SetActive(false);
            Debug.Log(string.Format("A guardian has been eliminated!"));

            if (guardiansTarget)
            {
                guardiansTarget.ClearFlee(true);
            }

            return;
        }

        Player caughtPlayer = other.gameObject.GetComponent<Player>();
        if (caughtPlayer)
        {
            Debug.Log("The player has hit a killzone!");

            OnPlayerKillZoneEnter?.Invoke(other.gameObject);

            return;
        }

        Hero enteringHero = other.gameObject.GetComponent<Hero>();
        if (enteringHero)
        {
            enteringHero.TargetReached = true;
        }

        Prisoner enteringPrisoner = other.gameObject.GetComponent<Prisoner>();
        if (enteringPrisoner)
        {
            enteringPrisoner.myTarget = myFortress.gameObject;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        Hero exitingHero = other.gameObject.GetComponent<Hero>();
        if (exitingHero)
        {
            // Debug.Log("A hero is exiting the base's surroundings.");
            exitingHero.TargetReached = false;
        }
    }
}
