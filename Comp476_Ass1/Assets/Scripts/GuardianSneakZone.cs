using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuardianSneakZone : MonoBehaviour
{
    // VARIABLES
    [SerializeField] protected Guardian myGuardian; // Probably not necessary as the guardian already receives events.



    // EVENTS
    public delegate void EventSneakZoneOnEnter(GameObject go);
    public event EventSneakZoneOnEnter OnSneakZoneEnter;
    public delegate void EventSneakZoneOnExit(GameObject go);
    public event EventSneakZoneOnExit OnSneakZoneExit;



    // METHODS

    // Built in.
    private void Start()
    {
        myGuardian = GetComponentInParent<Guardian>();
    }

    //private void OnTriggerEnter(Collider other)
    //{
    //    //Debug.Log(string.Format("{0} entered {1}.", other.gameObject, this));
    //    OnSneakZoneEnter?.Invoke(other.gameObject);
    //}
    //private void OnTriggerExit(Collider other)
    //{
    //    //Debug.Log(string.Format("{0} exited {1}.", other.gameObject, this));
    //    OnSneakZoneExit?.Invoke(other.gameObject);
    //}
}
