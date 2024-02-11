using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Can also be reused as a wall killzone.
public class HeroKillZone : MonoBehaviour
{
    // VARIABLES
    [SerializeField] private bool killPlayer = false;



    // EVENTS
    public delegate void EventCaptureZoneOnEnter(GameObject go);
    public static event EventCaptureZoneOnEnter OnHeroKillZoneEnter;
    public static event EventCaptureZoneOnEnter OnPlayerKillZoneEnter;



    // METHODS

    // Built in.
    private void Start()
    {
    }

    private void OnTriggerEnter(Collider other)
    {
        // Debug.Log(string.Format("{0} entered {1}.", other.gameObject, this));

        Hero caughtHero = other.gameObject.GetComponent<Hero>();
        if (caughtHero)
        {
            Debug.Log(string.Format("A hero has been caught! DUN DUN DUN"));

            OnHeroKillZoneEnter?.Invoke(other.gameObject);
            return;
        }

        Player caughtPlayer = other.gameObject.GetComponent<Player>();
        if (killPlayer && caughtPlayer)
        {
            Debug.Log("The player has hit a killzone!");

            OnPlayerKillZoneEnter?.Invoke(other.gameObject);
            return;
        }
    }
}
