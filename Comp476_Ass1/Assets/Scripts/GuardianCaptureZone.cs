using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuardianCaptureZone : MonoBehaviour
{
    // VARIABLES
    [SerializeField] protected Guardian myGuardian; // Probably not necessary as the guardian already receives events.



    // EVENTS
    public delegate void EventCaptureZoneOnEnter(GameObject go);
    public static event EventCaptureZoneOnEnter OnCaptureZoneEnter;



    // METHODS

    // Built in.
    private void Start()
    {
        myGuardian = GetComponentInParent<Guardian>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // Debug.Log(string.Format("{0} entered {1}.", other.gameObject, this));

        Hero caughtHero = other.gameObject.GetComponent<Hero>();
        if (caughtHero)
        {
            Debug.Log(string.Format("A hero has been caught! DUN DUN DUN"));

            OnCaptureZoneEnter?.Invoke(other.gameObject);
        }
    }
}
