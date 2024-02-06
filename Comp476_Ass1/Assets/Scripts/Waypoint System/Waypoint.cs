using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Waypoint : MonoBehaviour
{
    // VARIABLES
    [SerializeField] protected Waypoint nextWaypointUp;
    [SerializeField] protected Waypoint nextWaypointDown;



    // EVENTS



    // METHODS

    void VisualizeWayToNext()
    {
        if (nextWaypointUp)
        {
            Debug.DrawLine(this.transform.position - transform.right * 0.1f, nextWaypointUp.transform.position - transform.right * 0.1f, Color.red);
        }
        if (nextWaypointDown)
        {
            Debug.DrawLine(this.transform.position + transform.right * 0.1f, nextWaypointDown.transform.position - transform.right * 0.1f, Color.blue);
        }
    }

    // Built in.
    private void OnTriggerEnter(Collider other)
    {
        // If the colliding object is an NPC travelling between waypoints,
        NPC potentialTraveller = other.gameObject.GetComponent<NPC>();

        if (potentialTraveller && potentialTraveller.nextWaypoint == this) // and their desired target was this one,
        {
            // Going in
            if (potentialTraveller.previousWaypoint == nextWaypointDown)
            {
                potentialTraveller.nextWaypoint = nextWaypointUp;
                potentialTraveller.previousWaypoint = this;
            }
            // Going out
            if (potentialTraveller.previousWaypoint == nextWaypointUp)
            {
                potentialTraveller.nextWaypoint = nextWaypointDown;
                potentialTraveller.previousWaypoint = this;
            }
        }
    }

    void Update()
    {
        VisualizeWayToNext();
    }
}
