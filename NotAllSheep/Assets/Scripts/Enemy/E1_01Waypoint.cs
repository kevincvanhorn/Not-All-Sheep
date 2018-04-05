using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class E1_01Waypoint : MonoBehaviour{
    public bool waitForPlayer = true; // Should the enemy directly go to the waypoint or wait for the player?
                                      // Use for vertical or odd movements.
    public float speedToWaypoint = -1; // Speed enemy should move approaching this waypoint, -1 if use default.
}
