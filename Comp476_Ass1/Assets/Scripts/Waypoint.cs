using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Waypoint : MonoBehaviour
{
    // VARIABLES
    [SerializeField] private Waypoint nextWaypointIn;
    [SerializeField] private Waypoint nextWaypointOut;

    public Waypoint NextWaypointIn { get { return nextWaypointIn; } }
    public Waypoint NextWaypointOut { get { return nextWaypointOut; } }



    // EVENTS



    // METHODS

    void VisualizeWayToNext()
    {
        if (nextWaypointIn)
        {
            Debug.DrawLine(this.transform.position - transform.right * 0.1f, nextWaypointIn.transform.position - transform.right * 0.1f, Color.red);
        }
        if (nextWaypointOut)
        {
            Debug.DrawLine(this.transform.position + transform.right * 0.1f, nextWaypointOut.transform.position - transform.right * 0.1f, Color.blue);
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
            if (potentialTraveller.previousWaypoint == nextWaypointOut || potentialTraveller.previousWaypoint == null)
            {
                potentialTraveller.nextWaypoint = nextWaypointIn;
                potentialTraveller.previousWaypoint = this;
            }
            // Going out
            if (potentialTraveller.previousWaypoint == nextWaypointIn)
            {
                potentialTraveller.nextWaypoint = nextWaypointOut;
                potentialTraveller.previousWaypoint = this;
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        VisualizeWayToNext();
    }
}
