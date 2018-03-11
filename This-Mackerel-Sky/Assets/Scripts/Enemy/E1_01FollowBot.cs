using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MonsterLove.StateMachine; // State-Machine Package.

using Pathfinding;

[RequireComponent(typeof(Seeker))]
public class E1_01FollowBot : EnemyBase
{
    public Transform target;
    public Path path;

    public float speed = 2;
    public float nextWaypointDist = 3;
    public float repathRate = 0.5f;

    private Seeker seeker;
    private Rigidbody2D rigidbody;
    public ForceMode2D fMode; // way to change between force and impulse 

    private int curWaypoint = 0;
    private float lastRepath = float.NegativeInfinity;
    private bool isEnemyActive = true;

    private bool hasReachedEnd = true;

    private void Start()
    {
        seeker = GetComponent<Seeker>();
        rigidbody = GetComponent<Rigidbody2D>();
        StartCoroutine(UpdatePath());
    }

    public void OnPathComplete(Path p)
    {
        Debug.Log("Path Calculated. " + p.error);
        if (!p.error)
        {
            path = p;
            curWaypoint = 0; // Set waypoint counter to beginning of path.
        }
    }

    IEnumerator UpdatePath()
    {
        while (isEnemyActive)
        {
            if (target == null)
            {
                //TODO: search for target.
            }
            else
            {
                seeker.StartPath(transform.position, target.position, OnPathComplete);
            }
            yield return new WaitForSeconds(repathRate);
        }
    }

    public void FixedUpdate()
    {
        if (target == null)
        {
            //TODO: search for target.
            return;
        }
        else if (path == null)
            return;

        else if (curWaypoint > path.vectorPath.Count)
        {
            hasReachedEnd = true;
            return; // Already reached the end of path - do nothing. //hasReachedEnd = true;
        }

        /* Reach the end of path. */
        else if (curWaypoint == path.vectorPath.Count)
        {
            Debug.Log("Path: Reached end of path. ");
            curWaypoint++;
            //rigidbody.velocity = Vector3.zero;
            return;
        }

        Vector3 dir = (path.vectorPath[curWaypoint] - transform.position).normalized;
        Vector3 velocity = dir * speed * Time.fixedDeltaTime;

        //rigidbody.velocity = velocity;
        rigidbody.AddForce(velocity, fMode); // In direction of next waypoint


        float dist = Vector3.Distance(transform.position, path.vectorPath[curWaypoint]);
        if (dist < nextWaypointDist)
        {
            curWaypoint++;
            return;
        } //Things we lost in the fire. //do the dance, the way you move is a mystery
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.LogError("TRIGGEER");
    }
}