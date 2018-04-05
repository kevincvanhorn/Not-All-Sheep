using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MonsterLove.StateMachine; // State-Machine Package.

using Pathfinding;

public enum E1_01States {
    Idle,
    Fleeing,
    Interest,
    Follow
};

[RequireComponent(typeof(Seeker))]
public class E1_01FollowBot : EnemyBase
{
    public E1_01States state;

    /* Idle */
    public float idleDist = 30f; // Distance from enemy to player s.t. the enemy is brought out of / into the idle state.
    private float distCheckTime = 0.5f;
    private float distFromPlayer = 0f;


    /* Targets */
    public List<E1_01Waypoint> targets = new List<E1_01Waypoint>();
    private int curTarget = 0;

    public Transform target;
    public Path path;
    public float pathOffsetY = 2;

    public float speed = 2000;
    public float fleeSpeed = 6000;
    public float nextWaypointDist = 3;
    public float repathRate = 0.5f;

    private Seeker seeker;
    private Rigidbody2D rigidbody;
    private ForceMode2D fMode; // way to change between force and impulse 

    private int curWaypoint = 0;
    private float lastRepath = float.NegativeInfinity;
    private bool isEnemyActive = true;

    private bool hasReachedEnd = true;
    private bool inTrigger = false;

    private Transform playerTrans;
    private Vector3 playerDelta;
    private Vector3 playerPrev;

    /* Movement Perlin Noise */
    public float perlinHeightScale = 1.0f;
    public float perlinXScale = 1.0f;
    private float prevPerlinHeight = 0f; // position without perlin noise;
    private float prevPerlinLateral = 0f;

    private void Start()
    {
        seeker = GetComponent<Seeker>();
        rigidbody = GetComponent<Rigidbody2D>();

        playerTrans = GameObject.Find("Player").transform;
        playerPrev = playerTrans.position;

        state = E1_01States.Idle;
        StartCoroutine(UpdateDistToPlayer()); // Start in the idle state, checking if close to player. Switches if so.
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
                Vector3 posY = new Vector3(target.position.x, target.position.y + pathOffsetY, target.position.z); //x+13
                seeker.StartPath(transform.position, posY, OnPathComplete);
            }
            yield return new WaitForSeconds(repathRate);
        }
    }

    public void FixedUpdate()
    {
        if(state == E1_01States.Follow)
        {
            UpdateFollow();
        }
        else if(state == E1_01States.Idle)
        {
            UpdateIdle();
        }
        else if(state == E1_01States.Interest)
        {
            UpdateInterest();
        }
        else if (state == E1_01States.Fleeing)
        {
            UpdateFleeing();
        }
    }

    private void UpdateIdle()
    {
        if (distFromPlayer < idleDist)
        {
            SwitchState(E1_01States.Fleeing);
        }
    }

    IEnumerator UpdateDistToPlayer()
    {
        while (gameObject.activeSelf)
        {
            distFromPlayer = Vector3.Distance(transform.position, playerTrans.position);
            yield return new WaitForSeconds(distCheckTime);
        }
    }

    private void UpdateFollow()
    {
        //rigidbody.velocity = velocity;
        if (!inTrigger)
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
            rigidbody.AddForce(velocity, fMode); // In direction of next waypoint


            float dist = Vector3.Distance(transform.position, path.vectorPath[curWaypoint]);
            if (dist < nextWaypointDist)
            {
                curWaypoint++;
                return;
            } //Things we lost in the fire. //do the dance, the way you move is a mystery

        }
        else if (inTrigger)
        {
            rigidbody.velocity = Vector2.zero;
            playerDelta = playerTrans.position - playerPrev;
            playerDelta = new Vector3(playerDelta.x, 0, playerDelta.z);
            transform.Translate(playerDelta);
        }

        playerPrev = playerTrans.position;
    }

    private void UpdateInterest()
    {
        //rigidbody.velocity = velocity;
        if (!inTrigger)
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
            rigidbody.AddForce(velocity, fMode); // In direction of next waypoint


            float dist = Vector3.Distance(transform.position, path.vectorPath[curWaypoint]);
            if (dist < nextWaypointDist)
            {
                curWaypoint++;
                return;
            } //Things we lost in the fire. //do the dance, the way you move is a mystery

        }
        else if (inTrigger)
        {
            rigidbody.velocity = Vector2.zero;
            playerDelta = playerTrans.position - playerPrev;
            playerDelta = new Vector3(playerDelta.x, 0, playerDelta.z);
            transform.Translate(playerDelta);
        }

        playerPrev = playerTrans.position;
    }

    private void UpdateFleeing()
    {
        bool shouldWaitForPlayer = false;
        if (targets[curTarget].waitForPlayer && distFromPlayer > idleDist)
        {
            shouldWaitForPlayer = true;
        }

        if (!inTrigger && !shouldWaitForPlayer)
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
                curTarget++;

                if (curTarget < targets.Count)
                    target = targets[curTarget].transform;
                return;
            }

            /* Get Waypoint Speed if exists: (not == -1)*/
            if (targets[curTarget].speedToWaypoint != -1)
            {
                speed = targets[curTarget].speedToWaypoint;
            }

            Vector3 dir = (path.vectorPath[curWaypoint] - transform.position).normalized;
            Vector3 velocity = dir * speed * Time.fixedDeltaTime;
            rigidbody.AddForce(velocity, fMode); // In direction of next waypoint


            float dist = Vector3.Distance(transform.position, path.vectorPath[curWaypoint]);
            if (dist < nextWaypointDist)
            {
                curWaypoint++;
                return;
            } //Things we lost in the fire. //do the dance, the way you move is a mystery

        }
        else if (inTrigger)
        {
            rigidbody.velocity = Vector2.zero;
            playerDelta = playerTrans.position - playerPrev;
            playerDelta = new Vector3(playerDelta.x, 0, playerDelta.z);
            transform.Translate(playerDelta);
        }

        playerPrev = playerTrans.position;
    }

    private void LateUpdate()
    {
        float height = perlinHeightScale * Mathf.PerlinNoise(Time.time * perlinXScale, 0.0F);
        float lateral = (0.5f*perlinHeightScale) * Mathf.PerlinNoise((Time.time+1) * perlinXScale, 0.0F);
        float deltaHeight = height - prevPerlinHeight;
        float deltaWidth = lateral - prevPerlinLateral;

        transform.Translate(new Vector3(deltaWidth,deltaHeight,0));

        //Vector3 pos = transform.position;
        //pos.y = height;
        //pos.x = height / 2;
        prevPerlinHeight = height;
        prevPerlinLateral = lateral;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<PlayerBounds>() != null)
        {
            inTrigger = true;
            //Vector3 dir = (transform.position-path.vectorPath[curWaypoint]).normalized *rigidbody.velocity.magnitude*-1;
            rigidbody.velocity = Vector3.zero;
            //rigidbody.AddForce(dir, fMode);
            Debug.LogError("TRIGGEER");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.GetComponent<PlayerBounds>() != null)
        {
            inTrigger = false;
        }
    }

    /* State Switching Behaviour */
    private void SwitchState(E1_01States newState)
    {
        /* Call current state End methods. */
        EndState();
        
        /* Call new state Start methods: */
        if(newState == E1_01States.Interest || newState == E1_01States.Follow)
        {
            StartCoroutine(UpdatePath());
        }
        else if(newState == E1_01States.Fleeing)
        {
            speed = fleeSpeed;

            if (curTarget < targets.Count)
                target = targets[curTarget].transform;

            StartCoroutine(UpdatePath());
        }

        // Switch State.
        state = newState;
    }

    /* Calls End methods for the appropriate states. */
    private void EndState()
    {
        /* State End methods: */
        if (state == E1_01States.Interest || state == E1_01States.Follow || state == E1_01States.Fleeing)
        {
            StopCoroutine(UpdatePath());
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }
}