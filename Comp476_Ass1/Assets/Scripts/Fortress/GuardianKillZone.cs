using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuardianKillZone : MonoBehaviour
{
    // VARIABLES
    [SerializeField] protected Fortress myFortress; // Probably not necessary.



    // EVENTS
    public delegate void EventKillZoneOnEnter(GameObject go);
    public static event EventKillZoneOnEnter OnKillZoneEnter;



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
            OnKillZoneEnter?.Invoke(other.gameObject);

            Hero guardiansTarget = caughtGuardian.myTarget.GetComponent<Hero>();

            other.gameObject.SetActive(false);
            Debug.Log(string.Format("A guardian has been eliminated!"));

            if (guardiansTarget)
            {
                guardiansTarget.ClearFlee(true);
            }

            return;
        }

        Hero enteringHero = other.gameObject.GetComponent<Hero>();
        if (enteringHero)
        {
            enteringHero.TargetReached = true;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        Hero exitingHero = other.gameObject.GetComponent<Hero>();
        if (exitingHero)
        {
            Debug.Log("A hero is exiting the base's surroundings.");
            exitingHero.TargetReached = false;
        }
    }
}
