using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointEntryExit : Waypoint
{
    // VARIABLES
    [SerializeField] private Vector2 myPos2D;
    [SerializeField] private Vector2 myDir2D;
    [SerializeField] private Waypoint[] surroundingWaypoints;



    // METHODS

    // On NPC entrance, check if their target is on the other side of the guidezone, and direct them to a waypoint if so.
    bool IsBewtweenNPCAndTarget(NPC sourceNPC)
    {
        GameObject NPCsTarget = sourceNPC.myTarget;

        if (NPCsTarget != null)
        {
            //Debug.Log(string.Format("{0} following the target {1}", sourceNPC, NPCsTarget));

            if (sourceNPC.gameObject.CompareTag("Guardian"))
            {
                NPC targetOfNPCsTarget = NPCsTarget.GetComponent<NPC>();
                if (targetOfNPCsTarget && targetOfNPCsTarget.myTarget)
                {
                    NPCsTarget = targetOfNPCsTarget.myTarget;
                    //Debug.Log(string.Format("{0}'s own target is {1}", targetOfNPCsTarget, NPCsTarget));
                }
            }

            // This math did not seem to work with 3D vectors.
            Vector2 npcPos2D = new Vector2(sourceNPC.transform.position.x, sourceNPC.transform.position.z);
            Vector2 targetPos2D = new Vector2(NPCsTarget.transform.position.x, NPCsTarget.transform.position.z);

            // https://stackoverflow.com/questions/1560492/how-to-tell-whether-a-point-is-to-the-right-or-left-side-of-a-line
            // + help from chatGPT.

            Vector2 zoneToNPC = npcPos2D - myPos2D;
            Vector2 zoneToTarget = targetPos2D - myPos2D;

            float crossProductWithNPC = Vector3.Cross(myDir2D, zoneToNPC).z;
            float crossProductWithTarget = Vector3.Cross(myDir2D, zoneToTarget).z;

            return Mathf.Sign(crossProductWithNPC) != Mathf.Sign(crossProductWithTarget);
        }
        else return false;
    }

    // Built in.
    private void OnTriggerEnter(Collider other)
    {
        GameObject otherGameObject = other.gameObject;

        if (otherGameObject.CompareTag("Hero") || otherGameObject.CompareTag("Guardian") || otherGameObject.CompareTag("Prisoner"))
        {
            NPC potentialNPC = otherGameObject.GetComponent<NPC>();
            if (potentialNPC)
            {
                // To do: handle chasing Guardians.
                if (IsBewtweenNPCAndTarget(potentialNPC) && !potentialNPC.isCrossing)
                {
                    //Debug.Log(string.Format("A wall area separates {0} from its target {1}.", otherGameObject, potentialNPC.myTarget));

                    potentialNPC.nextWaypoint = nextWaypointUp ? nextWaypointUp : nextWaypointDown;
                    potentialNPC.previousWaypoint = this;
                    potentialNPC.isCrossing = true;
                }
                else if (potentialNPC.isCrossing && potentialNPC.nextWaypoint == this)
                {
                    // Clear variables as we are done crossing.
                    potentialNPC.ClearWaypointInfo();

                    //Debug.Log(string.Format("{0} is done crossing the wall zone.", otherGameObject));
                }
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        GameObject otherGameObject = other.gameObject;

        if (otherGameObject.CompareTag("Hero") || otherGameObject.CompareTag("Guardian") || otherGameObject.CompareTag("Prisoner"))
        {
            NPC potentialNPC = otherGameObject.GetComponent<NPC>();
            if (potentialNPC)
            {
                // Clear stuck NPCs.
                if (potentialNPC.isCrossing && !IsBewtweenNPCAndTarget(potentialNPC))
                {
                    potentialNPC.ClearWaypointInfo();
                }
            }
        }
    }

    void Start()
    {
        myPos2D = new Vector2(transform.position.x, transform.position.z);
        myDir2D = new Vector2(transform.forward.x, transform.forward.z);

        surroundingWaypoints = new Waypoint[] { nextWaypointUp, nextWaypointDown, null, null, null, null };
        if (nextWaypointUp)
        {
            surroundingWaypoints[2] = nextWaypointUp.NextWaypointUp;

            if (nextWaypointUp.NextWaypointUp)
            {
                surroundingWaypoints[4] = nextWaypointUp.NextWaypointUp.NextWaypointUp;
            }
        }
        
        if (nextWaypointDown)
        {
            surroundingWaypoints[3] = nextWaypointDown.NextWaypointDown;

            if (nextWaypointDown.NextWaypointDown)
            {
                surroundingWaypoints[5] = nextWaypointDown.NextWaypointDown.NextWaypointDown;
            }
        }
    }
}
