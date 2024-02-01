using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Base : MonoBehaviour
{
    // VARIABLES



    // METHODS

    // Built in.
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(string.Format("{0} entered {1}.", other.gameObject, this));

        if (other.gameObject.CompareTag("Prisoner"))
        {
            Debug.Log(string.Format("{0} will now be destroyed.", other.gameObject));
            Destroy(other.gameObject);
        }
    }
}
