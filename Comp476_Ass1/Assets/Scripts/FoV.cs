using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoV : MonoBehaviour
{
    // VARIABLES



    // EVENTS
    public delegate void EventFoVOnEnter(GameObject go);
    public event EventFoVOnEnter OnFoVEnter;
    public delegate void EventFoVOnExit(GameObject go);
    public event EventFoVOnExit OnFoVExit;



    // METHODS

    // Built in.
    private void OnTriggerEnter(Collider other)
    {
        // Debug.Log(string.Format("{0} entered {1}.", other.gameObject, this));
        OnFoVEnter?.Invoke(other.gameObject);
    }
    private void OnTriggerExit(Collider other)
    {
        // Debug.Log(string.Format("{0} exited {1}.", other.gameObject, this));
        OnFoVExit?.Invoke(other.gameObject);
    }
}
