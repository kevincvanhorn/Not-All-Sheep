using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MonsterLove.StateMachine; // State-Machine Package.

using Pathfinding;

[RequireComponent (typeof (Rigidbody2D))]
[RequireComponent(typeof(Seeker))]
public class E1_01FollowBot : EnemyBase {

    public Transform target;      // Target of enemy.
    public float updateTime = 2f; // rate/sec path is updated.

    private Seeker seeker;
    private Rigidbody2D rb;

    public Path path; // The calculated path. 

    public float speed = 300f; // Enemy speed per second.
    public ForceMode2D fMode; // way to change between force and impulse 

    [HideInInspector]
    public bool pathHasEnded = false;

    public float nextWaypointDist = 3; // how close to waypoint before reached.

    private int curWaypoint = 0; // The waypoint being moved towards (index). 

    private StateMachine<E1_01FollowBotStates> fsm;

    private enum E1_01FollowBotStates
    {
        Idle,
        Fleeing,
        Interest
    }

    private void Start()
    {
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();

        /* If no target is defined. */
        if(target == null)
        {
            Debug.LogError("ERROR: No Target Found.");
        }

        // Start a new path to target position and store result in OnPathComplete method.
        StartCoroutine(UpdatePath());
    }

    IEnumerator UpdatePath()
    {
        if(target == null)
        {
            //TODO: search for target.
            yield return null;
        }
        seeker.StartPath(transform.position, target.position, OnPathComplete);
        yield return new WaitForSeconds(updateTime);
        StartCoroutine(UpdatePath());
    }

    public void OnPathComplete(Path p)
    {
        Debug.Log("Path Created: " + p.error);
        if (!p.error)
        {
            path = p;
            curWaypoint = 0; // the waypoint where to start on the path. 
        }
    }

    private void FixedUpdate()
    {
        if (target == null)
        {
            //TODO: search for target.
            return;
        }

        // TODO: Always Look at Player. 

        if (path == null)
            return;

        if(curWaypoint >= path.vectorPath.Count)
        {
            if (pathHasEnded)
                return;

            Debug.Log("End of path reached. ");
            pathHasEnded = true;
            return;
        }

        pathHasEnded = false;

        // Direction to the next waypoint.
        Vector3 dir = (path.vectorPath[curWaypoint] - transform.position).normalized;
        dir *= speed * Time.fixedDeltaTime;

        // Move the Enemy.
        rb.AddForce(dir, fMode); // In direction of next waypoint
        float dist = Vector3.Distance(transform.position, path.vectorPath[curWaypoint]);
        if (dist < nextWaypointDist)
        {
            curWaypoint++;
            return;
        }
    }

    // Update is called once per frame
    void Update () {
		
	}

    void Idle_OnEnter()
    {

    }

    void Idle_Update()
    {

    }
}
