using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fortress : MonoBehaviour
{
    // VARIABLES
    [SerializeField] FortressKillzone myKillzone;



    // EVENTS
    // To do: add events on enter.
    public delegate void EventFortressOnEnter(GameObject go);
    public static event EventFortressOnEnter OnFortressEnter;


    // METHODS

    // Built in.
    private void Start()
    {
        myKillzone = GetComponentInParent<FortressKillzone>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // Debug.Log(string.Format("{0} entered {1}.", other.gameObject, this));

        if (other.gameObject.CompareTag("Prisoner"))
        {
            Debug.Log(string.Format("{0} has successfully been rescued!", other.gameObject));
            other.gameObject.SetActive(false);

            OnFortressEnter?.Invoke(other.gameObject);
        }
    }
}
