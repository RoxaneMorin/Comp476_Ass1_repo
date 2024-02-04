using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FortressKillzone : MonoBehaviour
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

            Debug.Log(string.Format("A guardian has been eliminated!"));

            other.gameObject.SetActive(false);

            //Destroy(other.gameObject, 1f); // Leave time for cleanup.
        }
    }
}
