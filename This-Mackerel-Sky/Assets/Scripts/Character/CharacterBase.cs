using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MonsterLove.StateMachine; // State-Machine Package.

// Update -> OnTrigger -> OnCollision

[RequireComponent(typeof(CInputManager))]
[RequireComponent(typeof(CCollisionState))]
[RequireComponent(typeof(CActionsBase))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class CharacterBase : MonoBehaviour
{
    Rigidbody2D rigidBody; // Not Kinematic: moves not by transform, but by physics
    public Collider2D collider; 

    /* Collisions Vars */
    private float slideFactor = 1;

    /* Colliders */
    HashSet<Vector2> contacts = new HashSet<Vector2>();

    /* Movement Variables */
    public float moveSpeed = 10;    // Horizontal speed.
    public float moveSpeedMin = 5;
    public float sprintSpeed = 20;
    public float activeSpeed;
    public Vector3 velocity;

    public Vector2 wallHitSpeed;
    public float directionFacing = 1;
    public float directionMoving = 1;

    /* Jump Variables */
    public float lateralAccelAirborne = 60;
    public float lateralAccelGrounded = 100;

    public float jumpHeightMax = 5;
    public float jumpHeightMin = .9f;
    public float timeToJumpApex = .4f;

    public float gravity;
    float jumpVelocityMax;
    float jumpVelocityMin;

    /* Slope Variables */
    public float slopeDir;
    public float slopeAngle = 0;
    public float maxAngle = 80;
    public Vector2 topSlopeSpeedCur;
    private Vector2 climbSlopeHitSpeed;
    public float steepSlopeMinEnterSpeed = 20;

    public Vector3 debugSlopeHitLoc;

    private CInputManager inputManager;
    private CActionsBase cActionsBase;

    /* Define States */
    public enum CStatesBase
    {
        FindState,
        Action,
        Idle,
        Airborne,
        OnWall,
        Running,
        Dashing,
        ClimbingSlope,
        TopSlope,
        SteepSlope,
        Simulate
    }

    public CCollisionState collisionState;

    public HashSet<CollisionType> enterCollisionTypes = new HashSet<CollisionType>(); // For use in that frame. // Should be virtual
    //HashSet<CollisionType> collisionTypes; // For use in that frame.

    public StateMachine<CStatesBase> fsm;

    /* Private State-Specific Vars */
    // Steep Slopes
    private float steepSlopeSpeed;
    private Vector2 steepSlopeHitNormal;
    private float wallFrictionDown;


    private Vector3 debugWallHitLoc;
    private Vector2 wallHitNormal;
    public float wallFallSpeed;
    public float wallStickTime;
    public bool isWallSticking;

    public bool hasLateralInput; // For camera smoothing.

    public void Awake()
    {
        // Initialize State Machine Engine		
        fsm = StateMachine<CStatesBase>.Initialize(this, CStatesBase.Airborne);
        collider = GetComponent<Collider2D>(); // For CameraFollow's Start Method.
    }

    public void Start()
    {
        /* Set collision defaults. */
        activeSpeed = moveSpeed;
        wallHitSpeed.x = activeSpeed;
        rigidBody = GetComponent<Rigidbody2D>();

        inputManager = GetComponent<CInputManager>();
        collisionState = GetComponent<CCollisionState>();
        cActionsBase = GetComponent<CActionsBase>();

        /* Calc constants in terms of Jump time and apex height. */
        gravity = -(2 * jumpHeightMax) / Mathf.Pow(timeToJumpApex, 2);
        jumpVelocityMax = Mathf.Abs(gravity * timeToJumpApex);
        jumpVelocityMin = Mathf.Sqrt(2 * Mathf.Abs(gravity) * jumpHeightMin);

        wallFrictionDown = 1;
        wallStickTime = 0;
        wallStickTime = 0;
    }

    void PreStateUpdate()
    {
        /* Update directionFacing ------------------------------------------ */
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            directionFacing = 1;
        }
        // When Left is first input.
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            directionFacing = -1;
        }
        else if (Input.GetKey(KeyCode.LeftArrow) && !Input.GetKey(KeyCode.RightArrow))
        {
            directionFacing = -1;
        }
        else if (Input.GetKey(KeyCode.RightArrow) && !Input.GetKey(KeyCode.LeftArrow))
        {
            directionFacing = 1;
        }

        if (!Input.GetKey(KeyCode.LeftArrow) && !Input.GetKey(KeyCode.RightArrow))
        {
            hasLateralInput = false;
        }
        else
        {
            hasLateralInput = true;
        }

        directionMoving = (velocity.x >= 0) ? 1 : -1;
        slopeDir = collisionState.slopeDir;

        //Debug.Log("PRESTATE VEL " + velocity);
    }

    void Update()
    {
        Debug.Log("MAIN - Update "+ velocity);
        rigidBody.velocity = velocity;
        //Debug.Log(velocity);
    }

    /** Called on Player collision with a new object. **/
    public void BaseCollisionEnter2D(Collision2D collision)
    { // ~ Could convert Collision2D to Collider2D
        //Debug.Log("BaseCollisionEnter2D" + velocity);
        PreStateUpdate();
        enterCollisionTypes.Clear();
        collisionState.CheckOverlaps();

        ContactPoint2D[] contactsIn = new ContactPoint2D[4]; // 2 when side collides (each corner) || 1 when on slope
        contactsIn = collision.contacts; //  NETBOOK - Uncomment this.
        //collision.GetContacts(contactsIn); //  NETBOOK - Comment this.
        /*Debug.LogError("BASECOLLISIONENTER");
        foreach (ContactPoint2D e in contactsIn) {
            Debug.LogError(e.normal);
        }
        Debug.LogError("----------");*/

        /* Call Collider Enter Functions */
        for (int i = 0; i < contactsIn.Length; i++)
        {
            /* If contact exists (entries are zero in larger alocated ContactPoint2D[])*/
            if (contactsIn[i].normal != Vector2.zero)
            {
                //Debug.DrawLine(contactsIn[i].point, contactsIn[i].point + Vector2.up, Color.blue, 20);
                //Debug.DrawLine(contactsIn[i].point, contactsIn[i].point+contactsIn[i].normal, Color.red, 20);
                slopeAngle = Vector2.Angle(contactsIn[i].normal, Vector2.up);

                /* Flat Ground */
                if (slopeAngle == CStats.botAngle)
                { // contactsIn[i].normal.y == -1
                    enterCollisionTypes.Add(CollisionType.Bot);
                    collisionState.bot = true;
                    collisionState.none = false;
                }
                /* Wall Collision */
                if (slopeAngle <= CStats.wallAngleMax && slopeAngle >= CStats.wallAngleMin)
                {
                    if (contactsIn[i].normal.x > 0)
                    {
                        enterCollisionTypes.Add(CollisionType.Left);
                        collisionState.left = true;
                        collisionState.none = false;
                    }
                    else if (contactsIn[i].normal.x < 0)
                    {
                        enterCollisionTypes.Add(CollisionType.Right);
                        collisionState.none = false;
                        collisionState.right = true;
                    }
                    else
                    {
                        Debug.Log("ERROR: Invalid Angle.");
                    }
                    wallHitNormal = contactsIn[i].normal;
                    debugWallHitLoc = contactsIn[i].point;
                }
                /* Top Collision*/
                else if (slopeAngle >= CStats.topAngleMin && slopeAngle <= CStats.topAngleMax)
                {
                    enterCollisionTypes.Add(CollisionType.Top);
                    collisionState.top = true;
                    collisionState.none = false;
                }
                /* Top Slope COllision*/
                else if (slopeAngle > CStats.wallAngleMax && slopeAngle < CStats.topAngleMin)
                {
                    enterCollisionTypes.Add(CollisionType.TopSlope);
                    collisionState.topSlope = true;
                    collisionState.none = false;
                }
                /* Steep Slope Collision. */
                else if (slopeAngle > CStats.slopeAngleMax && slopeAngle < CStats.topAngleMin)
                {
                    slopeDir = (contactsIn[i].normal.x < 0) ? 1 : -1; // 1 = right, -1 = left
                    enterCollisionTypes.Add(CollisionType.SteepSlope);
                    collisionState.steepSlope = true;
                    collisionState.none = false;

                    debugSlopeHitLoc = contactsIn[i].point;
                    steepSlopeHitNormal = contactsIn[i].normal;
                }
                /* Slope Collision */
                else
                { // This is now bot.
                    //Debug.LogError(slopeAngle);
                    slopeDir = (contactsIn[i].normal.x < 0) ? 1 : -1; // 1 = right, -1 = left
                    //slopeAngle = Vector2.Angle(contactsIn[i].normal, Vector2.up);
                    enterCollisionTypes.Add(CollisionType.Slope);
                    collisionState.slope = true;
                    collisionState.none = false;
                }
            }
        }

        /*if (contactsIn.Length > 1) {
            Debug.LogError("ERROR: Multiple Collision Points.");
            foreach(var e in contactsIn) {
                Debug.LogError(e.normal);
            }
            Debug.LogError("Slope " + collisionState.Slope + " Right " + collisionState.Right);
            Debug.LogError("-----------");
        }*/

        //Debug.Log("BaseCollisionEnter2D END" + velocity);
    } // Should be virtual

    /* Collision Methods: Custom ---------------------------------------------*/
    // EXECUTION ORDER:
    // Enter - Called immediately when changeState is called (before Main Update).
    // Exit
    // Update - Called after Main Update
    // Collision Enter/Exit
    // Input Events
    // Update
    // LateUpdate
    // Finally

    /* Should be a buffer state active when no input is pressed. */
    void Idle_Enter()
    {
        // velocity.x = 0
        Debug.Log("IDLE - Enter");
    }

    void Idle_Update()
    {
        Debug.Log("IDLE - Update");
        PreStateUpdate();

        //collisionState.printStatesError();

        /* Vertical JUMP Calc ------------------------------------------ */
        // Jump if pressed or held && not touchingTop (ex: sandwiched between two platforms).
        if (Input.GetKey(KeyCode.UpArrow) && !collisionState.Top && !collisionState.TopSlope)
        {
            velocity.y = jumpVelocityMax;
            fsm.ChangeState(CStatesBase.Simulate, StateTransition.Safe);
        }

        /*if (!collisionState.Bot || !collisionState.Slope) {
            fsm.ChangeState(States.Simulate, StateTransition.Safe);
        }*/

        /* Lateral Calc -------------------------------------------------- */
        else if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow))
        {
            if (collisionState.Slope)
            {
                if (slopeAngle > CStats.slopeAngleMin && slopeAngle <= CStats.slopeAngleMax)
                {
                    fsm.ChangeState(CStatesBase.ClimbingSlope, StateTransition.Safe);
                }
                else
                {
                    Debug.LogError("TopCollision - Invalid Angle");
                }
            }
            else if (collisionState.Bot)
            {
                fsm.ChangeState(CStatesBase.Running, StateTransition.Safe);
            }
            else if (collisionState.SteepSlope)
            {
                fsm.ChangeState(CStatesBase.SteepSlope, StateTransition.Safe);
            }
            else
            {
                Debug.LogError("ERROR: Invalid Idle Transition.");
                collisionState.printStatesError();
            }
        }

        if (inputManager.ActionKeyPressed())
        {
            fsm.ChangeState(CStatesBase.Action);
        }
    }

    void Idle_OnCollisionEnter2D(Collision2D collision)
    {
        BaseCollisionEnter2D(collision);
    }

    void Airborne_Enter()
    {
        Debug.Log("AIRBORNE - Enter");
    }

    void Airborne_Update()
    {
        Debug.Log("AIRBORNE -  Update");
        PreStateUpdate();

        /* Vertical Calc ----------------------------------------- */
        if (Input.GetKeyUp(KeyCode.UpArrow))
        {  // Variable jump - When Up is released in this frame.
            if (velocity.y > jumpVelocityMin)
            {
                velocity.y = jumpVelocityMin;
            }
        }

        /* Lateral Calc -------------------------------------------*/
        if (Input.GetKey(KeyCode.RightArrow) && velocity.x < activeSpeed)
        { // in-air lateral move right
            velocity.x += lateralAccelAirborne * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.LeftArrow) && velocity.x > -activeSpeed)
        { // in-air lateral move left
            velocity.x -= lateralAccelAirborne * Time.deltaTime;
        }

        if (collisionState.Top && velocity.y > 0)
        {
            velocity.y = 0;
        }

        velocity.y += gravity * Time.deltaTime; // Apply Gravity until grounded

        // Jumping While Against Wall.
        if (collisionState.Right || collisionState.Left)
        {
            /*print("CollisionState -------------");
            collisionState.printStates();
            print("----------------------------");*/
            if (velocity.x != 0) wallHitSpeed = velocity;
            //Debug.LogError("wallhitspeed " + wallHitSpeed);
            velocity.x = 0;
            fsm.ChangeState(CStatesBase.OnWall);
        }

        // Trigger Action.
        if (inputManager.ActionKeyPressed())
        {
            fsm.ChangeState(CStatesBase.Action);
        }
    }

    void Airborne_OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("AIRBORNE - OnCollisionEnter " + velocity);
        BaseCollisionEnter2D(collision);

        /* These are the new collisions this frame from this specific collision. */
        // ? Iterate for all combinations not needed with contains.
        if (enterCollisionTypes.Count > 0)
        {
            /* Bot Collision. */
            if (enterCollisionTypes.Contains(CollisionType.Bot))
            {
                velocity.y = 0;
                enterCollisionTypes.Remove(CollisionType.Bot); // Addressed this collision so delete.
                if (velocity.x == 0) { fsm.ChangeState(CStatesBase.Idle, StateTransition.Safe); }
                else { fsm.ChangeState(CStatesBase.Running, StateTransition.Safe); }
                // Continues execution from here after NextState.Enter() before Update() next frame.
            }

            /* Steep Slope Collision. */
            else if (enterCollisionTypes.Contains(CollisionType.SteepSlope))
            {
                if (slopeAngle > CStats.slopeAngleMax && slopeAngle < CStats.topAngleMin)
                {
                    fsm.ChangeState(CStatesBase.SteepSlope, StateTransition.Safe);
                }
                else { Debug.LogError("SteepSlope Collision - Invalid Angle"); }
                enterCollisionTypes.Remove(CollisionType.SteepSlope);
            }

            /* Slope Collision. */
            else if (enterCollisionTypes.Contains(CollisionType.Slope))
            {
                if (slopeAngle > CStats.slopeAngleMin && slopeAngle <= CStats.slopeAngleMax)
                {
                    //velocity.y = 0; // TODO Address this.
                    //velocity.x = 0;
                    fsm.ChangeState(CStatesBase.ClimbingSlope, StateTransition.Safe);
                }
                else { Debug.LogError("Slope Collision - Invalid Angle"); }
                enterCollisionTypes.Remove(CollisionType.Slope);
            }

            /* Top Slope Collision. */
            else if (enterCollisionTypes.Contains(CollisionType.TopSlope))
            {
                if (slopeAngle > CStats.wallAngleMax && slopeAngle < CStats.topAngleMin)
                {
                    topSlopeSpeedCur = velocity;
                    fsm.ChangeState(CStatesBase.TopSlope);
                }
                else { Debug.LogError("TopCollision - Invalid Angle"); }
                enterCollisionTypes.Remove(CollisionType.TopSlope);
                //Debug.DrawLine(debugSlopeHitLoc, debugSlopeHitLoc + velocity, Color.yellow, 20);
            }

            /* Wall Collision (Including Wall Slope). */
            else if (enterCollisionTypes.Contains(CollisionType.Left))
            {
                wallHitSpeed = velocity;
                //Debug.LogError("--Wallhitspeed" + wallHitSpeed);
                velocity.x = 0; // Commented Out 1.5.18
                velocity.y = 0; // added 1.16.18
                enterCollisionTypes.Remove(CollisionType.Left);
                if (!collisionState.Bot)
                {
                    fsm.ChangeState(CStatesBase.OnWall, StateTransition.Safe);
                }
                else
                {
                    velocity.x = 0;
                    velocity.y = 0; // added 1.16.18
                    Debug.LogWarning("AIRBORNE: This state should be inaccessible - grounded & touchingWall");
                }
            }
            else if (enterCollisionTypes.Contains(CollisionType.Right))
            {
                wallHitSpeed = velocity;
                //Debug.LogError("--Wallhitspeed" + wallHitSpeed);
                velocity.x = 0; // Commented Out 1.5.18
                velocity.y = 0; // added 1.16.18
                enterCollisionTypes.Remove(CollisionType.Right);
                if (!collisionState.Bot)
                {
                    fsm.ChangeState(CStatesBase.OnWall, StateTransition.Safe);
                }
                else
                {
                    velocity.x = 0;
                    velocity.y = 0; // added 1.16.18
                    Debug.LogWarning("AIRBORNE: This state should be inaccessible - grounded & touchingWall");
                }
            }

            /* Top Collision. */
            else if (enterCollisionTypes.Contains(CollisionType.Top))
            {
                enterCollisionTypes.Remove(CollisionType.Top);
                velocity.y = 0;
            }
            else { fsm.ChangeState(CStatesBase.FindState, StateTransition.Safe); }
        }
    }

    void Running_Enter()
    {
        Debug.Log("RUNNING - Enter");
    }

    void Running_Update()
    {
        Debug.Log("RUNNING - Update");
        PreStateUpdate();

        //Debug.Log("Pre-Velocity" + velocity.x);

        // check contacts and set velocity.x = 0 should be touching the ground still

        /* Sprint Calc ------------------------------------------------- */
        if (Input.GetKey(KeyCode.LeftShift))
        {
            activeSpeed = sprintSpeed;
        }
        else
        {
            activeSpeed = moveSpeed;
        }

        Debug.Log("Accel-Velocity" + velocity.x);

        /* Lateral Calc -------------------------------------------------- */
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow))
        {
            velocity.x = activeSpeed * directionFacing;
        }
        /* X Acceleration ---------------------------------------------- */
        else if (velocity.x != 0 && !Input.GetKey(KeyCode.RightArrow) && !Input.GetKey(KeyCode.LeftArrow))
        { // On-release of Lateral Movement controls - Deccelerate
            //velocity.x = 0; //1.3.18
            Debug.Log(directionMoving + " Pre --" + velocity.x);
            if (directionMoving == 1) { // Decceleration Right
                   if(velocity.x >= 0)
                {
                    velocity.x -= lateralAccelGrounded * Time.deltaTime;
                }
                if(velocity.x < 0) { velocity.x = 0; }
            }
            else if (directionMoving == -1) { // Decceleration Left
                if(velocity.x < 0)
                {
                    velocity.x += lateralAccelGrounded * Time.deltaTime;
                }
                if(velocity.x >= 0) { velocity.x = 0; }  
            }
            Debug.Log(directionMoving + " Post --" + velocity.x);
        }

        /* Run/deccelerate into wall - Applied here once instead of conditionals above. */
        if (velocity.x > 0 && collisionState.Right)
        {
            velocity.x = 0;
        }
        else if (velocity.x < 0 && collisionState.Left)
        {
            velocity.x = 0;
        }

        /* Steep Slope Min Velocity. */
        if (collisionState.SteepSlope)
        {
            if (velocity.x > -steepSlopeMinEnterSpeed && velocity.x < 0 && slopeDir == -1)
            {
                if (Input.GetKey(KeyCode.LeftArrow))
                {
                    velocity.x = 0;
                }
                   
            }
            else if (velocity.x < steepSlopeMinEnterSpeed && velocity.x > 0 && slopeDir == 1)
            {
                if (Input.GetKey(KeyCode.RightArrow))
                {
                    velocity.x = 0;
                }
                    
            }
        }

        /* Priority Cases*/
        if (inputManager.ActionKeyPressed())
        { // Trigger Action.
            print("Running Transition 3");
            fsm.ChangeState(CStatesBase.Action);
        }

        else if (collisionState.None)
        { // Case - slide off edge
            Debug.LogError("NONE");
            collisionState.printStatesError();
            fsm.ChangeState(CStatesBase.Simulate, StateTransition.Safe);
        }

        /* Vertical JUMP Calc ------------------------------------------ */
        // Jump if pressed or held && not touchingTop (ex: sandwiched between two platforms).
        else if (Input.GetKey(KeyCode.UpArrow) && !collisionState.Top && !collisionState.TopSlope)
        {
            velocity.y = jumpVelocityMax;
            print("Running Transition 1");
            fsm.ChangeState(CStatesBase.Simulate, StateTransition.Safe);
        }
        else if (velocity.x == 0 && !Input.GetKey(KeyCode.RightArrow) && !Input.GetKey(KeyCode.LeftArrow))
        {
            Debug.Log("Running Transition 2");
            fsm.ChangeState(CStatesBase.Idle, StateTransition.Safe);
        }
        /*(else if (Mathf.Abs(velocity.x) >= steepSlopeMinEnterSpeed && collisionState.SteepSlope)
        {
            fsm.ChangeState(States.SteepSlope, StateTransition.Safe);
        }*/
        else if (collisionState.SteepSlope && Mathf.Abs(velocity.x) >= steepSlopeMinEnterSpeed)
        {
            if (velocity.x < 0 && slopeDir == -1)
            {
                fsm.ChangeState(CStatesBase.SteepSlope, StateTransition.Safe);
            }
            else if (velocity.x > 0 && slopeDir == 1)
            {
                fsm.ChangeState(CStatesBase.SteepSlope, StateTransition.Safe);
            }
        }
        //Debug.Log("Post-Velocity" + velocity.x);
    }

    void Running_OnCollisionEnter2D(Collision2D collision)
    {
        BaseCollisionEnter2D(collision);
        Debug.Log("RUNNING - OnCollisionEnter");

        if (enterCollisionTypes.Count > 0)
        {
            /* Wall Collision (Including Wall Slopes). */
            if (enterCollisionTypes.Contains(CollisionType.Right)) // TouchingWall.
            {
                wallHitSpeed = velocity;
                enterCollisionTypes.Remove(CollisionType.Right);
                velocity.x = 0;
            }
            else if (enterCollisionTypes.Contains(CollisionType.Left)) // TouchingWall.
            {
                wallHitSpeed = velocity;
                enterCollisionTypes.Remove(CollisionType.Left);
                velocity.x = 0;
            }

            /* Top Collision. */
            else if (enterCollisionTypes.Contains(CollisionType.Top))
            {
                velocity.y = 0; // Redundancy case - addressed in this.Update.
                enterCollisionTypes.Remove(CollisionType.Top);
            }

            /* Steep Slope Collision. */
            else if (enterCollisionTypes.Contains(CollisionType.SteepSlope))
            {
                Debug.Log("Steep Slope Collision from Running. ");
                if (slopeAngle > CStats.slopeAngleMax && slopeAngle < CStats.topAngleMin)
                {
                    if (Mathf.Abs(velocity.x) >= 20)
                    {
                        fsm.ChangeState(CStatesBase.SteepSlope, StateTransition.Safe);
                    }
                    else
                    {
                        velocity.x = 0;
                    }
                }
                else { Debug.LogError("SteepSlope Collision - Invalid Angle"); }
                enterCollisionTypes.Remove(CollisionType.SteepSlope);
            }

            /* Slope Collision. */
            else if (enterCollisionTypes.Contains(CollisionType.Slope))
            {
                if (slopeAngle > CStats.slopeAngleMin && slopeAngle <= CStats.slopeAngleMax)
                {
                    fsm.ChangeState(CStatesBase.ClimbingSlope, StateTransition.Safe);
                }
                else { Debug.LogError("TopCollision - Invalid Angle"); }
                enterCollisionTypes.Remove(CollisionType.Slope);
            }

            /* Top Slope Collision. */
            else if (enterCollisionTypes.Contains(CollisionType.TopSlope))
            {
                if (slopeAngle > CStats.wallAngleMax && slopeAngle < CStats.topAngleMin)
                {
                    velocity.y = 0; // Top Slope Collisions
                }
                else { Debug.LogError("TopCollision - Invalid Angle"); }
                enterCollisionTypes.Remove(CollisionType.TopSlope);
            }

            /* Invalid State. */
            else
            {
                fsm.ChangeState(CStatesBase.FindState, StateTransition.Safe);
            }
        }
    }

    IEnumerator OnWall_Enter()
    {
        yield return new WaitForEndOfFrame();// WaitforEndofFrame();
        Debug.Log("ONWALL - Enter " + velocity + " " + wallHitSpeed);
        velocity = wallHitSpeed;
        wallStickTime = Mathf.Abs(wallHitSpeed.x); // 1.16.18
        float wallNormalAngle = Vector2.Angle(velocity * -1, wallHitNormal);

        Debug.DrawLine(debugWallHitLoc, debugWallHitLoc + (Vector3)wallHitNormal * 5, Color.blue, 10f);
        Debug.DrawLine(debugWallHitLoc, debugWallHitLoc + velocity * -1, Color.yellow, 10f);

        /* Enable / Disable Wallsticking 1.16.17 */
        //if ((collisionState.Left && Input.GetKey(KeyCode.LeftArrow)) || (collisionState.Right && Input.GetKey(KeyCode.RightArrow)))
        //{
        if (wallNormalAngle <= 40) // Impact Angle
            {
                isWallSticking = true;
                //velocity = Vector2.zero;
            }
        //}
        else
        {
            isWallSticking = false;
        }

        if (!collisionState.Bot && !collisionState.Slope && !collisionState.TopSlope) // 1.13.18 - had vel > 0?
        {
            
            wallFallSpeed = Mathf.Sin(wallNormalAngle * Mathf.Deg2Rad) * velocity.magnitude * Mathf.Sign(velocity.y);
            //Debug.LogError("Velocity   " + velocity.magnitude);
            Debug.LogError("WallHitAngle      " + wallNormalAngle);
            //Debug.LogError("Speed Calc " + wallFallSpeed);
        }
        else
        {
            wallFallSpeed = velocity.magnitude;
            collisionState.printStatesError();
            Debug.LogWarning("ERROR: Probably should not be here");
        }
    }

    /* OnWall method for jumping player toward a wall */
    // @ param dir: 1 = left, -1 = right
    private void jumpTowardWall(int dir) 
    {
        velocity.y = jumpVelocityMax;
        velocity.x = dir * activeSpeed / 2;
        fsm.ChangeState(CStatesBase.Simulate, StateTransition.Safe);
    }

    /* OnWall method for jumping player away from a wall */
    // @ param dir: 1 = left, -1 = right
    private void jumpAwayFromWall(int dir)
    {
        velocity.y = jumpVelocityMax;
        velocity.x = dir *activeSpeed;
        fsm.ChangeState(CStatesBase.Simulate, StateTransition.Safe);
    }

    void OnWall_Update()
    {
        Debug.Log("ONWALL - Update");
        PreStateUpdate();

        slopeAngle = collisionState.curWallAngle; // CollisionState update is before this.

        //Debug.Log(velocity);
        
        if (collisionState.Slope)
        {
            //Commented out 1.5.18 - Untested
            velocity.y = 0;
            velocity.x = 0;
            
        }
        else if (!collisionState.Right && !collisionState.Left)
        { 
            fsm.ChangeState(CStatesBase.Simulate, StateTransition.Safe);
        }
        else
        {
            // Wall Stick Waiting
            if (isWallSticking)
            {
                if (wallStickTime > 0 && !(collisionState.Left && Input.GetKey(KeyCode.RightArrow) || collisionState.Right && Input.GetKey(KeyCode.LeftArrow)))
                {
                    wallStickTime += gravity * Time.deltaTime;
                }
                else{
                    //velocity = Vector2.zero;
                }
                
            }

            // Apply gravity to dummy variable.
            wallFallSpeed += gravity * Time.deltaTime; // Slide down Slope 1.16.18

            velocity.x = wallFallSpeed * Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * collisionState.slopeDir; // steepSlopeSpeed
            velocity.y = wallFallSpeed * Mathf.Sin(slopeAngle * Mathf.Deg2Rad);

            // velocity.y += gravity * Time.deltaTime; // Apply Gravity until grounded

            // If only touching one side.
            if (!(collisionState.Left && collisionState.Right))
            {
                // When Up is released in this frame.
                if (Input.GetKeyUp(KeyCode.UpArrow))
                {
                    if (wallFallSpeed > jumpVelocityMin)
                    { // Keep applying velocity up while key is pressed - variable jump
                        wallFallSpeed = jumpVelocityMin;
                    }
                }

                // When Up is first input.
                if (Input.GetKeyDown(KeyCode.UpArrow))
                {
                    if (collisionState.Left && Input.GetKey(KeyCode.LeftArrow)) // Jump toward left wall.
                    {
                        jumpTowardWall(1);
                    }
                    else if(collisionState.Left && Input.GetKey(KeyCode.RightArrow)) // Jump away from left wall
                    {
                        jumpAwayFromWall(1);
                    }
                    else if (collisionState.Right && Input.GetKey(KeyCode.RightArrow)) // Jump toward right wall.
                    {
                        jumpTowardWall(-1);
                    }
                    else if (collisionState.Right && Input.GetKey(KeyCode.LeftArrow)) // Jump away from right wall.
                    {
                        jumpAwayFromWall(-1);
                    }
                }
                // When Right is first input.
                else if (Input.GetKeyDown(KeyCode.RightArrow))
                { // on L/R input - setting conditions.
                    directionFacing = 1;
                    if (collisionState.Right && Input.GetKey(KeyCode.UpArrow)) // Jumping toward right wall.
                    {
                        jumpTowardWall(-1);
                    }
                    else if(collisionState.Left && Input.GetKey(KeyCode.UpArrow)) // Jump away from left wall.
                    {
                        jumpAwayFromWall(1);
                    }
                }
                // When Left is first input.
                else if (Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    print("WALL - State Change 1");
                    directionFacing = -1;
                    if (collisionState.Left && Input.GetKey(KeyCode.UpArrow)) // Jumping toward left wall.
                    {
                        jumpTowardWall(1);
                    }
                    else if(collisionState.Right && Input.GetKey(KeyCode.UpArrow)) // Jump away from right wall.
                    {
                        jumpAwayFromWall(-1);
                    }
                }

                // When Right or Left is held down.
                else if (Input.GetKey(KeyCode.RightArrow))
                {
                    if (collisionState.Left)
                    {
                        if (Input.GetKey(KeyCode.UpArrow)) // Jump away from left wall.
                        {
                            jumpAwayFromWall(1);
                        }
                        else
                        { // Fall away from left wall
                            if (velocity.y <= 0)
                            {
                                if (velocity.x < 0) velocity.x = 0;  // Needed for falling from wall but sticking bc velocity is negative into wall.
                                velocity.x += lateralAccelAirborne * Time.deltaTime;
                                fsm.ChangeState(CStatesBase.Simulate, StateTransition.Safe);
                            }
                            // Doesn't allow falling away from wall when wallRising
                        }
                    }
                    else if (collisionState.Right && Input.GetKey(KeyCode.UpArrow)) // Jumping toward right wall.
                    {
                        // When coming from a non-grounded state, immediately jump when hit wall
                        if (velocity.y < 0)
                        {
                            jumpTowardWall(-1);
                        }
                    }

                }
                else if (Input.GetKey(KeyCode.LeftArrow))
                {
                    if (collisionState.Right)
                    {
                        print("WALL - State Change 2");
                        if (Input.GetKey(KeyCode.UpArrow)) // Jump away from right wall.
                        {
                            print("WALL - State Change 3");
                            jumpAwayFromWall(-1);
                        }
                        else
                        { // Fall away from wall
                            if(velocity.y <= 0)
                            {
                                if (velocity.x > 0) velocity.x = 0;// Needed for falling from wall but sticking bc velocity is negative into wall.
                                velocity.x -= lateralAccelAirborne * Time.deltaTime;
                                fsm.ChangeState(CStatesBase.Simulate, StateTransition.Safe);
                            }
                        }
                        // Doesn't allow falling away from wall when wallRising
                    }
                    else if (collisionState.Left && Input.GetKey(KeyCode.UpArrow)) // Jumping toward left wall.
                    {
                        // When coming from a non-grounded state, immediately jump when hit wall
                        if (velocity.y < 0)
                        {
                            jumpTowardWall(1);
                        }
                    }
                }
            }
        }
        //collisionState.printStatesShort();
        //print("ONWALL - End of Update. Vel " + velocity);

        //Debug.DrawLine(debugWallHitLoc, debugWallHitLoc + velocity * .25f, Color.yellow, 10f);
    }
   

    /*   void OnWall_Update()
       {
           Debug.Log("ONWALL - Update");
           PreStateUpdate();

           bool isTouchingLeft = collisionState.Left;
           bool isTouchingRight = collisionState.Right;


           //Debug.Log(velocity);

           else if (!collisionState.Right && !collisionState.Left)
           { // Case - slide off edge
               fsm.ChangeState(CStatesBase.Simulate, StateTransition.Safe);
           }
           else
           {
               velocity.y += gravity * Time.deltaTime; // Apply Gravity until grounded

               // Only Touching one side.
               if (!(isTouchingLeft && isTouchingRight))
               {
                   // When Up is released in this frame.
                   if (Input.GetKeyUp(KeyCode.UpArrow))
                   {
                       if (velocity.y > jumpVelocityMin)
                       { // Keep applying velocity up while key is pressed - variable jump
                           velocity.y = jumpVelocityMin;
                       }
                   }
                   // When Up is first input.
                   if (Input.GetKeyDown(KeyCode.UpArrow))
                   {
                       if (isTouchingLeft && Input.GetKey(KeyCode.LeftArrow)) // Jump toward left wall.
                       {
                           velocity.y = jumpVelocityMax;
                           velocity.x = activeSpeed / 2;
                           fsm.ChangeState(CStatesBase.Simulate, StateTransition.Safe);
                       }
                       else if (isTouchingRight && Input.GetKey(KeyCode.RightArrow)) // Jump toward right wall.
                       {
                           velocity.y = jumpVelocityMax;
                           velocity.x = -1 * activeSpeed / 2;
                           fsm.ChangeState(CStatesBase.Simulate, StateTransition.Safe);
                       }
                   }
                   // When Right is first input.
                   else if (Input.GetKeyDown(KeyCode.RightArrow))
                   { // on L/R input - setting conditions.
                       directionFacing = 1;
                       if (isTouchingRight && Input.GetKey(KeyCode.UpArrow)) // Jumping toward right wall.
                       {
                           velocity.y = jumpVelocityMax;
                           velocity.x = -1 * activeSpeed / 2;
                           fsm.ChangeState(CStatesBase.Simulate, StateTransition.Safe);
                       }
                   }
                   // When Left is first input.
                   else if (Input.GetKeyDown(KeyCode.LeftArrow))
                   {
                       print("WALL - State Change 1");
                       directionFacing = -1;
                       if (isTouchingLeft && Input.GetKey(KeyCode.UpArrow)) // Jumping toward left wall.
                       {
                           velocity.y = jumpVelocityMax;
                           velocity.x = activeSpeed / 2;
                           fsm.ChangeState(CStatesBase.Simulate, StateTransition.Safe);
                       }
                   }

                   // When Right or Left is held down.
                   else if (Input.GetKey(KeyCode.RightArrow))
                   {
                       if (isTouchingLeft)
                       {
                           if (Input.GetKey(KeyCode.UpArrow)) // Jump away from left wall.
                           {
                               velocity.y = jumpVelocityMax;
                               velocity.x = activeSpeed;
                               fsm.ChangeState(CStatesBase.Simulate, StateTransition.Safe);
                           }
                           else
                           { // Fall away from left wall
                               if (velocity.y <= 0)
                               {
                                   if (velocity.x < 0) velocity.x = 0;  // Needed for falling from wall but sticking bc velocity is negative into wall.
                                   velocity.x += lateralAccelAirborne * Time.deltaTime;
                                   fsm.ChangeState(CStatesBase.Simulate, StateTransition.Safe);
                               }
                               // Doesn't allow falling away from wall when wallRising
                           }
                       }
                       else if (isTouchingRight && Input.GetKey(KeyCode.UpArrow)) // Jumping toward right wall.
                       {
                           // When coming from a non-grounded state, immediately jump when hit wall
                           if (velocity.y < 0)
                           {
                               velocity.y = jumpVelocityMax;
                               velocity.x = -1 * activeSpeed / 2;
                               fsm.ChangeState(CStatesBase.Simulate, StateTransition.Safe);
                           }
                       }

                   }
                   else if (Input.GetKey(KeyCode.LeftArrow))
                   {
                       if (isTouchingRight)
                       {
                           print("WALL - State Change 2");
                           if (Input.GetKey(KeyCode.UpArrow)) // Jump away from right wall.
                           {
                               print("WALL - State Change 3");
                               velocity.y = jumpVelocityMax;
                               velocity.x = -1 * activeSpeed / 2;
                               fsm.ChangeState(CStatesBase.Simulate, StateTransition.Safe);
                           }
                           else
                           { // Fall away from wall
                               if (velocity.y <= 0)
                               {
                                   if (velocity.x > 0) velocity.x = 0;// Needed for falling from wall but sticking bc velocity is negative into wall.
                                   velocity.x -= lateralAccelAirborne * Time.deltaTime;
                                   fsm.ChangeState(CStatesBase.Simulate, StateTransition.Safe);
                               }
                           }
                           // Doesn't allow falling away from wall when wallRising
                       }
                       else if (isTouchingLeft && Input.GetKey(KeyCode.UpArrow)) // Jumping toward left wall.
                       {
                           // When coming from a non-grounded state, immediately jump when hit wall
                           if (velocity.y < 0)
                           {
                               velocity.y = jumpVelocityMax;
                               velocity.x = activeSpeed / 2;
                               fsm.ChangeState(CStatesBase.Simulate, StateTransition.Safe);
                           }
                       }
                   }
               }
           }
       }
       */

    void OnWall_OnCollisionEnter2D(Collision2D collision)
    {
        BaseCollisionEnter2D(collision);
        Debug.Log("ONWALL - OnCollisionEnter");

        if (enterCollisionTypes.Count > 0)
        {
            /* Bot Collision. */
            if (enterCollisionTypes.Contains(CollisionType.Bot))
            {
                velocity.y = 0;
                if (velocity.x == 0) { fsm.ChangeState(CStatesBase.Idle, StateTransition.Safe); }
                else { fsm.ChangeState(CStatesBase.Running, StateTransition.Safe); }
                enterCollisionTypes.Remove(CollisionType.Bot); // Addressed this collision so delete.
                // Continues execution from here after NextState.Enter() before Update() next frame.
            }

            /* Steep Slope Collision. */
            else if (enterCollisionTypes.Contains(CollisionType.SteepSlope))
            {
                if (slopeAngle > CStats.slopeAngleMax && slopeAngle < CStats.topAngleMin)
                {
                    fsm.ChangeState(CStatesBase.SteepSlope, StateTransition.Safe);
                }
                else { Debug.LogError("SteepSlope Collision - Invalid Angle"); }
                enterCollisionTypes.Remove(CollisionType.SteepSlope);
            }

            /* Slope Collision. */
            else if (enterCollisionTypes.Contains(CollisionType.Slope))
            {
                if (slopeAngle > CStats.slopeAngleMin && slopeAngle <= CStats.slopeAngleMax)
                {
                    velocity.x = 0;
                    velocity.y = 0;
                    fsm.ChangeState(CStatesBase.ClimbingSlope, StateTransition.Safe);
                }
                else { Debug.LogError("TopCollision - Invalid Angle"); }
                enterCollisionTypes.Remove(CollisionType.Slope);
            }

            /* Top Slope Collision. */
            else if (enterCollisionTypes.Contains(CollisionType.TopSlope))
            {
                if (slopeAngle > CStats.wallAngleMax && CStats.topAngleMin < 175)
                {
                    velocity.y = 0;
                }
                else { Debug.LogError("TopCollision - Invalid Angle"); }
                enterCollisionTypes.Remove(CollisionType.TopSlope);
            }

            /* Top Collision. */
            else if (enterCollisionTypes.Contains(CollisionType.Top))
            {
                velocity.y = 0;
                enterCollisionTypes.Remove(CollisionType.Top);
            }

            /* Wall Collision (Including Wall Slopes). */
            else if (enterCollisionTypes.Contains(CollisionType.Left))
            {
                wallHitSpeed = velocity;
                velocity.x = 0; //1.5.18

                enterCollisionTypes.Remove(CollisionType.Left);
                Debug.LogWarning("This should not usually occur. Addressed in Update.");
            }
            else if (enterCollisionTypes.Contains(CollisionType.Right))
            {
                wallHitSpeed = velocity;
                velocity.x = 0; // 1.5.18

                enterCollisionTypes.Remove(CollisionType.Right);
                Debug.LogWarning("This should not usually occur. Addressed in Update.");
            }

            /* Invalid State - Unfound. */
            else
            {
                fsm.ChangeState(CStatesBase.FindState, StateTransition.Safe);
            }
        }
    }

    void ClimbingSlope_Enter()
    {
        Debug.Log("SLOPE - Enter");
        //collisionState.printStatesError();
        //Debug.Log("Slope E Pre: " + velocity);

        climbSlopeHitSpeed = velocity;
        velocity.x = climbSlopeHitSpeed.x * Mathf.Cos(slopeAngle * Mathf.Deg2Rad); // * directionMoving if not just .x
        velocity.y = Mathf.Abs(climbSlopeHitSpeed.x) * Mathf.Sign(velocity.y) * Mathf.Sin(slopeAngle * Mathf.Deg2Rad);
        // What if Enter -> velocity change in a collision or before end of current frame update -> First update

        //Debug.Log("Slope Enter: " + velocity);
    }

    void ClimbingSlope_Update()
    {
        PreStateUpdate();
        Debug.Log("SLOPE - Update ");
        //Debug.Log("dir: " + directionFacing + " R/L:" + Input.GetKey(KeyCode.RightArrow) +" "+Input.GetKey(KeyCode.LeftArrow));
        //collisionState.printStatesShort();

        slopeAngle = collisionState.curSlopeAngle;

        /* Sprint Calc ------------------------------------------------- */
        if (Input.GetKey(KeyCode.LeftShift)) { activeSpeed = sprintSpeed; }
        else { activeSpeed = moveSpeed; }

        /* Lateral Calc -------------------------------------------------- */
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow))
        {
            if (collisionState.Right && Input.GetKey(KeyCode.RightArrow))
            {
                velocity.x = 0;
                velocity.y = 0;
            }
            else if (collisionState.Left && Input.GetKey(KeyCode.LeftArrow))
            {
                velocity.x = 0;
                velocity.y = 0;
            }
            else
            {
                Debug.Log("SLOPE - Lateral Calc. ");
                velocity.x = activeSpeed * Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * directionFacing;
                velocity.y = activeSpeed * Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * slopeDir * directionFacing;
            }

        }


        /* X Acceleration ---------------------------------------------- */
        else if (!Input.GetKey(KeyCode.RightArrow) && !Input.GetKey(KeyCode.LeftArrow))
        { // On-release of Lateral Movement controls - Deccelerate
            //velocity.x = 0;
            //velocity.y = 0;
            Debug.Log(directionMoving + " Pre: " + velocity);
            //Debug.LogError("Slope Angle " + slopeAngle);
            if (directionMoving == 1)
            { // Decceleration Right
                if (velocity.x >= 0)
                {
                    //Debug.LogError("Case 1.");
                    velocity.x -= lateralAccelGrounded * Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * Time.deltaTime;
                    velocity.y += slopeDir * -1 * lateralAccelGrounded * Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * Time.deltaTime;
                }
                if (velocity.x < 0) {
                    //Debug.Log("Halt 1.");
                    velocity.y = 0;
                    velocity.x = 0;
                }
            }
            else if (directionMoving == -1)
            { // Decceleration Left
                if (velocity.x < 0)
                {
                    //Debug.LogError("Case 2.");
                    velocity.x += lateralAccelGrounded * Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * Time.deltaTime;
                    velocity.y += slopeDir * lateralAccelGrounded * Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * Time.deltaTime;
                }
                if (velocity.x >= 0) {
                    Debug.Log("Halt 2.");
                    velocity.y = 0;
                    velocity.x = 0;
                }
            }
            Debug.Log(directionMoving + " Post: " + velocity);
        }

        /* Run/deccelerate into wall - Applied here once instead of conditionals above. */
        /*if (velocity.x > 0 && collisionState.Right) {
            Debug.LogError("ERROR1.");
            velocity.x = 0;
            velocity.y = 0;
        }
        else if (velocity.x < 0 && collisionState.Left) {
            Debug.LogError("ERROR2.");
            velocity.x = 0;
            velocity.y = 0;
        }*/

        if (collisionState.Top || collisionState.TopSlope)
        { // NOTE: Do not else with above, uses calculated velocity.
            if (velocity.y > 0)
            {
                velocity.x = 0;
                velocity.y = 0;
            }
        }

        /* Steep Slope Min Velocity. */
        if (collisionState.SteepSlope && activeSpeed < steepSlopeMinEnterSpeed)
        {
            if (velocity.x < 0 && collisionState.steepSlopeDir == -1)
            {
                velocity.x = 0;
                velocity.y = 0;
            }
            else if (velocity.x > 0 && collisionState.steepSlopeDir == 1)
            {
                velocity.x = 0;
                velocity.y = 0;
            }
        }


        /* Priority Cases (Change the actual state). */
        if (inputManager.ActionKeyPressed())
        { // Trigger Action.
            fsm.ChangeState(CStatesBase.Action);
        }
        else if (collisionState.None)
        { // Case - slide off edge
            fsm.ChangeState(CStatesBase.Simulate, StateTransition.Safe);
        }
        else if ((collisionState.Left || collisionState.Right) && !collisionState.Slope)
        { // Ran up slope and skid up wall
            // Case1.03: not on slope - just above at corner of slope and wall. 
            if (Input.GetKey(KeyCode.UpArrow) && !collisionState.Top && !collisionState.TopSlope)
            {
                Debug.Log("Slope - Transition 4");
                //Debug.LogError("Case1.03");
                velocity.y = jumpVelocityMax;
                fsm.ChangeState(CStatesBase.OnWall, StateTransition.Safe);
            }
        }

        /* Vertical JUMP Calc ------------------------------------------ */
        // Jump if pressed or held && not touchingTop (ex: sandwiched between two platforms).
        else if (Input.GetKey(KeyCode.UpArrow) && !collisionState.Top && !collisionState.TopSlope)
        {
            Debug.Log("Slope - Transition 3");
            //NOTE! Remember to copy this jump behavior to the Case1.03 Above
            velocity.y = jumpVelocityMax;
            /*(if (collisionState.Left || collisionState.Right)
            {
                fsm.ChangeState(States.OnWall, StateTransition.Safe);
            }
            else
            {*/
            fsm.ChangeState(CStatesBase.Simulate, StateTransition.Safe);
            //}

        }
        /*else if (velocity.x == 0 && !Input.GetKey(KeyCode.RightArrow) && !Input.GetKey(KeyCode.LeftArrow))
        {
            // TODO: This is where idle would go. 
            //velocity.y = 0;
            if (collisionState.Slope)
            {
                Debug.Log("Slope - Transition 1");
            }
            else
            {
                Debug.Log("Slope - Transition 2");
            }

            //fsm.ChangeState(States.Idle, StateTransition.Safe);
        }*/

        /* Steep Slope Min Velocity. */
        else if (collisionState.SteepSlope && activeSpeed >= steepSlopeMinEnterSpeed)
        {
            if (velocity.x < 0 && collisionState.steepSlopeDir == -1)
            {
                fsm.ChangeState(CStatesBase.SteepSlope, StateTransition.Safe);
            }
            else if (velocity.x > 0 && collisionState.steepSlopeDir == 1)
            {
                fsm.ChangeState(CStatesBase.SteepSlope, StateTransition.Safe);
            }
        }

        Debug.Log(velocity);
    }

    void ClimbingSlope_OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("SLOPE - OnCollisionEnter2D");
        BaseCollisionEnter2D(collision);

        if (enterCollisionTypes.Count > 0)
        {
            /* Wall Collision (Including Wall Slopes). */
            if (enterCollisionTypes.Contains(CollisionType.Right))
            {
                wallHitSpeed = velocity;
                // TouchingWall.
                velocity.x = 0;
                velocity.y = 0;
                enterCollisionTypes.Remove(CollisionType.Right);
            }
            else if (enterCollisionTypes.Contains(CollisionType.Left))
            {
                wallHitSpeed = velocity;
                // TouchingWall.
                velocity.x = 0;
                velocity.y = 0;
                enterCollisionTypes.Remove(CollisionType.Left);
            }

            /* Top Collision. */
            else if (enterCollisionTypes.Contains(CollisionType.Top))
            {
                velocity.x = 0;
                velocity.y = 0; // Redundancy case - addressed in this.Update.
                enterCollisionTypes.Remove(CollisionType.Top);
            }

            /* Top Slope Collision. */
            else if (enterCollisionTypes.Contains(CollisionType.TopSlope))
            {
                // 01.01.18b01 ?
                if (slopeAngle > CStats.wallAngleMax && slopeAngle < CStats.topAngleMin)
                { // TODO: Address this. Should stop moving if hits topSlope.
                    velocity.x = 0;
                    velocity.y = 0; // Redundancy case - addressed in this.Update.
                }
                else { Debug.LogError("TopCollision - Invalid Angle"); }

                enterCollisionTypes.Remove(CollisionType.TopSlope);
            }

            /* Bot Collision. */
            else if (enterCollisionTypes.Contains(CollisionType.Bot))
            {
                fsm.ChangeState(CStatesBase.Running, StateTransition.Safe);
                enterCollisionTypes.Remove(CollisionType.Bot);
            }

            /* Steep Slope Collision. */
            else if (enterCollisionTypes.Contains(CollisionType.SteepSlope))
            {
                Debug.Log("Steep Slope Collision from Running. ");
                if (slopeAngle > CStats.slopeAngleMax && slopeAngle < CStats.topAngleMin)
                {
                    if (activeSpeed >= 20)//Mathf.Abs(velocity.x) >= 20)
                    {
                        fsm.ChangeState(CStatesBase.SteepSlope, StateTransition.Safe);
                    }
                    else
                    {
                        velocity.x = 0;
                        velocity.y = 0;
                    }
                }
                else { Debug.LogError("SteepSlope Collision - Invalid Angle"); }
                enterCollisionTypes.Remove(CollisionType.SteepSlope);
            }

            /* Slope Collision. */
            else if (enterCollisionTypes.Contains(CollisionType.Slope))
            {
                //fsm.ChangeState(States.ClimbingSlope, StateTransition.Safe);
                if (slopeAngle > CStats.slopeAngleMin && slopeAngle <= CStats.slopeAngleMax)
                {
                    //Stay
                }
                else { Debug.LogError("TopCollision - Invalid Angle " + slopeAngle); }
                enterCollisionTypes.Remove(CollisionType.Slope);
            }

            /* Undefined State. */
            else
            {
                fsm.ChangeState(CStatesBase.FindState, StateTransition.Safe);
            }
        }
    }

    void TopSlope_Enter()
    {
        velocity = topSlopeSpeedCur;
        Debug.Log("TOPSLOPE - Enter");
    }

    void TopSlope_Update()
    {
        PreStateUpdate();
        Debug.Log("TOPSLOPE - Update");

        float platformSlippiness = 20;
        slideFactor = 1 / (Mathf.Abs(velocity.x)) * 100 + platformSlippiness;
        if (slideFactor < 1)
        {
            slideFactor = 1;
        }

        velocity = topSlopeSpeedCur;

        Vector2 slopeVector = (Vector2)(Quaternion.Euler(0, 0, 180 - slopeAngle) * Vector2.right); //PFEF
        float hitAngle = Vector2.Angle(velocity, slopeVector);
        //velocity.x = slopeHitSpeed.x * Mathf.Cos((180 - slopeAngle) * Mathf.Deg2Rad); // - velocit?
        velocity.y = Mathf.Abs(topSlopeSpeedCur.x) * Mathf.Sin((180 - slopeAngle) * Mathf.Deg2Rad);

        Debug.Log("SlideFactor " + slideFactor);
        Debug.Log("Slope Angle: " + (180 - slopeAngle));
        //Debug.LogError("Slope Vector: " + slopeVector);
        Debug.Log("Hit Angle : " + hitAngle);
        //Debug.DrawLine(new Vector3(0,0,0), slopeVector, Color.yellow, 20);
        //Debug.DrawLine(new Vector3(0, 0, 0), velocity, Color.red, 20);
        Debug.DrawLine(debugSlopeHitLoc, debugSlopeHitLoc + (Vector3)slopeVector, Color.green, 20);
        Debug.DrawLine(debugSlopeHitLoc, debugSlopeHitLoc + velocity, Color.red, 20);

        if (velocity.x == 0)
        {
            topSlopeSpeedCur.y = 0;
        }
        else
        {
            topSlopeSpeedCur.y += gravity * slideFactor * Time.deltaTime; // Apply Gravity until grounded
        }

        if (topSlopeSpeedCur.y <= 0)
        {
            fsm.ChangeState(CStatesBase.Simulate, StateTransition.Safe);
        }

        Debug.Log("slopeHitSpeed" + topSlopeSpeedCur);
        Debug.Log("velocity" + velocity);

        if (!collisionState.Slope)
        {
            fsm.ChangeState(CStatesBase.Simulate, StateTransition.Safe);
        }
    }

    void TopSlope_OnCollisionEnter2D(Collision2D collision)
    {
        Debug.LogError("TOPSLOPE - OnCollisionEnter2D - TODO Address this.");
        BaseCollisionEnter2D(collision);

        /* Wall Collision (Including Wall Slopes). */
        if (enterCollisionTypes.Contains(CollisionType.Right)) // 1.13.18 Added - untested
        {
            wallHitSpeed = velocity;
            // TouchingWall.
            velocity.x = 0;
            velocity.y = 0;
            enterCollisionTypes.Remove(CollisionType.Right);
        }
        else if (enterCollisionTypes.Contains(CollisionType.Left))
        {
            wallHitSpeed = velocity;
            // TouchingWall.
            velocity.x = 0;
            velocity.y = 0;
            enterCollisionTypes.Remove(CollisionType.Left);
        }

    }

    void SteepSlope_Enter()
    {
        Debug.Log("STEEPSLOPE - Enter");

        /* Sprint Calc ------------------------------------------------- */
        if (Input.GetKey(KeyCode.LeftShift))
        {
            activeSpeed = sprintSpeed;
        }
        else
        {
            activeSpeed = moveSpeed;
        }

        if (velocity.y < 0 && !collisionState.Bot && !collisionState.Slope)
        {
            //Debug.DrawLine(debugSlopeHitLoc, debugSlopeHitLoc + (Vector3)steepSlopeHitNormal * 5, Color.blue, 10f);
            //Debug.DrawLine(debugSlopeHitLoc, debugSlopeHitLoc + velocity * -1, Color.yellow, 10f);
            float steepNormalAngle = Vector2.Angle(velocity * -1, steepSlopeHitNormal);
            steepSlopeSpeed = Mathf.Sin(steepNormalAngle * Mathf.Deg2Rad) * velocity.magnitude * Mathf.Sign(velocity.y);

            //Debug.LogError("Velocity   " + velocity.magnitude);
            //Debug.LogError("Angle      " + steepNormalAngle);
            //Debug.LogError("Speed Calc " + steepSlopeSpeed);
        }
        else
        {
            steepSlopeSpeed = velocity.magnitude;
        }
    }

    void SteepSlope_Update()
    {
        PreStateUpdate();
        Debug.Log("STEEPSLOPE - Update");

        slopeAngle = collisionState.curSteepSlopeAngle; // CollisionState update is before this.

        /* Lateral Calc -------------------------------------------------- */
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow))
        {
            if (collisionState.Right && Input.GetKey(KeyCode.RightArrow))
            {
                velocity.x = 0;
                velocity.y = 0;
            }
            else if (collisionState.Left && Input.GetKey(KeyCode.LeftArrow))
            {
                velocity.x = 0;
                velocity.y = 0;
            }
        }

        steepSlopeSpeed += gravity * Time.deltaTime; // Slide down Slope

        velocity.x = steepSlopeSpeed * Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * collisionState.steepSlopeDir; // steepSlopeSpeed
        velocity.y = steepSlopeSpeed * Mathf.Sin(slopeAngle * Mathf.Deg2Rad);

        if (collisionState.Top || collisionState.TopSlope)
        { // NOTE: Do not else with above, uses calculated velocity.
            if (velocity.y > 0)
            {
                velocity.x = 0;
                velocity.y = 0;
            }
        }

        /* Priority Cases (Change the actual state). */
        if (inputManager.ActionKeyPressed())
        { // Trigger Action.
            fsm.ChangeState(CStatesBase.Action);
        }
        else if (collisionState.None)
        { // Case - slide off edge
            Debug.Log("STEEPSLOPE - Transition 5");
            fsm.ChangeState(CStatesBase.Simulate, StateTransition.Safe);
        }
        else if ((collisionState.Left || collisionState.Right) && !collisionState.steepSlope) // TODO: Test this. 
        { // Ran up slope and skid up wall
            // Case1.03: not on slope - just above at corner of slope and wall. 
            if (!collisionState.Top && !collisionState.TopSlope)
            {
                if (Input.GetKeyDown(KeyCode.UpArrow))
                {
                    Debug.Log("STEEPSLOPE - Transition 4");
                    //NOTE! Remember to copy this jump behavior to the Case1.03 Above
                    velocity.y = jumpVelocityMax;
                    fsm.ChangeState(CStatesBase.Simulate, StateTransition.Safe);
                }
                else if (velocity.y >= 0 && (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow)))
                {
                    Debug.Log("STEEPSLOPE - Transition 7");
                    velocity.y = jumpVelocityMax;
                    fsm.ChangeState(CStatesBase.Simulate, StateTransition.Safe);
                }

            }
        }

        /* Vertical JUMP Calc ------------------------------------------ */
        // Jump if pressed && not touchingTop (ex: sandwiched between two platforms).
        else if (!collisionState.Top && !collisionState.TopSlope)
        {
            if(Input.GetKeyDown(KeyCode.UpArrow))
            {
                Debug.Log("STEEPSLOPE - Transition 3");
                //NOTE! Remember to copy this jump behavior to the Case1.03 Above
                velocity.y = jumpVelocityMax;
                fsm.ChangeState(CStatesBase.Simulate, StateTransition.Safe);
            }
            else if(velocity.y >= 0 && (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow)))
            {
                Debug.Log("STEEPSLOPE - Transition 6");
                velocity.y = jumpVelocityMax;
                fsm.ChangeState(CStatesBase.Simulate, StateTransition.Safe);
            }
            
        }
        Debug.Log(velocity);
    }

    void SteepSlope_OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("STEEPSLOPE - OnCollisionEnter2D");
        BaseCollisionEnter2D(collision);

        if (enterCollisionTypes.Count > 0)
        {
            /* Wall Collision (Including Wall Slopes). */
            if (enterCollisionTypes.Contains(CollisionType.Right))
            {
                wallHitSpeed = velocity;
                // TouchingWall.
                velocity.x = 0;
                velocity.y = 0;
                enterCollisionTypes.Remove(CollisionType.Right);
            }
            else if (enterCollisionTypes.Contains(CollisionType.Left))
            {
                wallHitSpeed = velocity;
                // TouchingWall.
                velocity.x = 0;
                velocity.y = 0;
                enterCollisionTypes.Remove(CollisionType.Left);
            }

            /* Top Collision. */
            else if (enterCollisionTypes.Contains(CollisionType.Top))
            {
                velocity.x = 0;
                velocity.y = 0; // Redundancy case - addressed in this.Update.
                enterCollisionTypes.Remove(CollisionType.Top);
            }

            /* Top Slope Collision. */
            else if (enterCollisionTypes.Contains(CollisionType.TopSlope))
            {
                // 01.01.18b01 ?
                if (slopeAngle > CStats.wallAngleMax && slopeAngle < CStats.topAngleMin)
                { // TODO: Address this. Should stop moving if hits topSlope.
                    velocity.x = 0;
                    velocity.y = 0; // Redundancy case - addressed in this.Update.
                }
                else { Debug.LogError("TopCollision - Invalid Angle"); }

                enterCollisionTypes.Remove(CollisionType.TopSlope);
            }

            /* Bot Collision. */
            else if (enterCollisionTypes.Contains(CollisionType.Bot))
            {
                //velocity.x = activeSpeed; //1.3.18
                velocity.y = 0;
                fsm.ChangeState(CStatesBase.Running, StateTransition.Safe);
                enterCollisionTypes.Remove(CollisionType.Bot);
            }

            /* Steep Slope Collision. */
            else if (enterCollisionTypes.Contains(CollisionType.SteepSlope))
            {
                // TODO: Add the if statements and else to be sure of angle.
            }

            /* Slope Collision. */
            else if (enterCollisionTypes.Contains(CollisionType.Slope))
            {
                //fsm.ChangeState(States.ClimbingSlope, StateTransition.Safe);
                if (slopeAngle > CStats.slopeAngleMin && slopeAngle <= CStats.slopeAngleMax)
                {
                    fsm.ChangeState(CStatesBase.ClimbingSlope, StateTransition.Safe);
                }
                else { Debug.LogError("TopCollision - Invalid Angle " + slopeAngle); }
                enterCollisionTypes.Remove(CollisionType.Slope);
            }


            /* Undefined State. */
            else
            {
                fsm.ChangeState(CStatesBase.FindState, StateTransition.Safe);
            }
        }
    }

    /* Simulate is for the 4-5 frames after a jump/transition away from an object into empty space occurs.
     Needed for the collision state to catch up so that actions like airborne checking if it's touching the floor
     in an update does not occur immediately at the first frame of up pressed out of the grounded state while the object is 
     still "grounded" by the bounding check.*/
    void Simulate_Enter()
    {
        Debug.Log("SIMULATE - Enter from" + fsm.LastState);
        collisionState.printStatesError();
    }

    void Simulate_Update()
    {
        Debug.Log("Simulate_Update");
        PreStateUpdate();

        /* Lateral Calc -------------------------------------------*/
        if (Input.GetKey(KeyCode.RightArrow) && velocity.x < activeSpeed)
        { // in-air lateral move right
            velocity.x += lateralAccelAirborne * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.LeftArrow) && velocity.x > -activeSpeed)
        { // in-air lateral move left
            velocity.x -= lateralAccelAirborne * Time.deltaTime;
        }

        velocity.y += gravity * Time.deltaTime; // Apply Gravity until grounded

        // Trigger Action.
        if (inputManager.ActionKeyPressed())
        {
            fsm.ChangeState(CStatesBase.Action);
        }
        /* From Running State */
        else if (fsm.LastState == CStatesBase.Running)
        {
            if (!collisionState.Bot)
            {
                fsm.ChangeState(CStatesBase.Airborne);
            }
        }

        /* From Idle State. */
        else if (fsm.LastState == CStatesBase.Idle)
        {
            if (!collisionState.Bot)
            {
                fsm.ChangeState(CStatesBase.Airborne);
            }
            else if (!collisionState.Slope)
            {
                fsm.ChangeState(CStatesBase.Airborne);
            }
        }

        /* From Wall State. */
        else if (fsm.LastState == CStatesBase.OnWall)
        {
            if (collisionState.top)
            {
                //velocity.y = 0;
            }
            if (!collisionState.Left && !collisionState.Right)
            {
                fsm.ChangeState(CStatesBase.Airborne);
            }
            else if (collisionState.Bot)
            {
                fsm.ChangeState(CStatesBase.Idle);
            }
            else if (collisionState.TopSlope)
            {
                if (slopeAngle > CStats.wallAngleMax && slopeAngle < CStats.topAngleMin)
                {
                    topSlopeSpeedCur = velocity;
                    fsm.ChangeState(CStatesBase.TopSlope);
                }
                else { Debug.LogError("TopCollision - Invalid Angle"); }
            }
            else if (collisionState.Slope)
            {
                if (slopeAngle > CStats.slopeAngleMin && slopeAngle <= CStats.slopeAngleMax)
                {
                    fsm.ChangeState(CStatesBase.ClimbingSlope, StateTransition.Safe);
                }
                else { Debug.LogError("TopCollision - Invalid Angle"); }
            }
        }

        /* From Slope State. */
        else if (fsm.LastState == CStatesBase.ClimbingSlope)
        {
            if (!collisionState.Slope)
            { // TODO: Fix this b/c topslopes by adding slope angle aspect
                fsm.ChangeState(CStatesBase.Airborne);
            }
        }

        /* From Top Slope State. */
        else if (fsm.LastState == CStatesBase.TopSlope)
        {
            if (!collisionState.TopSlope)
            { //TODO - above ex. what if comeout of stop slope into a bot slope = stuck.
                if (collisionState.None)
                {
                    fsm.ChangeState(CStatesBase.Airborne);
                }
                else
                {
                    Debug.LogError("ERROR: Simulate - Top Slope - Can't transition to airborne. ");
                }
            }
            else if (collisionState.Bot || collisionState.Slope)
            {
                fsm.ChangeState(CStatesBase.Idle);
            }
        }

        else if(fsm.LastState == CStatesBase.SteepSlope)
        {
            if (!collisionState.SteepSlope)
            {
                if (collisionState.None)
                {
                    fsm.ChangeState(CStatesBase.Airborne);
                }
                else
                {
                    Debug.LogError("ERROR: Simulate - Top Slope - Can't transition to airborne. ");
                }
            }
        }

        /* From Undefined State. */
        else
        {
            Debug.LogWarning("Simulate_Update: State Simulate not defined from " + fsm.LastState);
        }
    }

    void Simulate_OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("Simulate - OnCollisionEnter from " + fsm.LastState);
        BaseCollisionEnter2D(collision);

        /*collisionState.printStatesError();
        Debug.LogError("TOPSLOPE " + enterCollisionTypes.Contains(CollisionType.TopSlope) + " SLOPE " + enterCollisionTypes.Contains(CollisionType.Slope));
        foreach (CollisionType e in enterCollisionTypes) {
            Debug.LogError("---- " + e);
        }
        foreach (ContactPoint2D c in collision.contacts) {
            Debug.LogError("THIS - " + c.normal);
        }*/


        /* These are the new collisions this frame from this specific collision. */
        // ? Iterate for all combinations not needed with contains.
        if (enterCollisionTypes.Count > 0)
        {
            /* Bot Collision. */
            if (enterCollisionTypes.Contains(CollisionType.Bot))
            {
                velocity.y = 0;
                enterCollisionTypes.Remove(CollisionType.Bot); // Addressed this collision so delete.
                if (velocity.x == 0)
                {
                    fsm.ChangeState(CStatesBase.Idle, StateTransition.Safe);
                }
                else
                {
                    fsm.ChangeState(CStatesBase.Running, StateTransition.Safe);
                }
                // Continues execution from here after NextState.Enter() before Update() next frame.
            }

            /* Wall Collision (Including SLoped Wall). */
            else if (enterCollisionTypes.Contains(CollisionType.Left))
            {
                wallHitSpeed = velocity;
                velocity.x = 0; // Added 1.13.18
                velocity.y = 0; // added 1.16.18
                // OnWall.
                enterCollisionTypes.Remove(CollisionType.Left);
                if (!collisionState.Bot)
                {
                    fsm.ChangeState(CStatesBase.OnWall, StateTransition.Safe);
                }
                else
                {
                    velocity.x = 0;
                    velocity.y = 0; // added 1.16.18
                    Debug.LogWarning("SIMULATE: This state should be inaccessible - grounded & touchingWall");
                }
            }
            else if (enterCollisionTypes.Contains(CollisionType.Right))
            {
                velocity.x = 0; // Added 1.13.18
                enterCollisionTypes.Remove(CollisionType.Right);
                if (!collisionState.Bot)
                {
                    fsm.ChangeState(CStatesBase.OnWall, StateTransition.Safe);
                }
                else
                {
                    velocity.x = 0;
                    Debug.LogWarning("SIMULATE: This state should be inaccessible - grounded & touchingWall");
                }
            }

            /* Top Collision. */
            else if (enterCollisionTypes.Contains(CollisionType.Top))
            {
                velocity.y = 0;
                if (!collisionState.Slope && !collisionState.Left && !collisionState.Right && !collisionState.Bot)
                {
                    fsm.ChangeState(CStatesBase.Airborne);
                }
                // Case1.04: climbing slope topcollision (touching top == true, touching slope == true) skips airborne, 
                // directly from slope to top in simulation, skipping the airborne that would be there with finer calculations..
                else if (collisionState.Slope)
                {
                    fsm.ChangeState(CStatesBase.ClimbingSlope); //TODO: Check this with topSlopes
                }
                else { Debug.LogError("ERROR: State Transition Top Corner"); }

                /*if (fsm.LastState == States.ClimbingSlope) {
                    velocity.x = 0;
                    fsm.ChangeState(States.ClimbingSlope);
                }*/
                enterCollisionTypes.Remove(CollisionType.Top);
            }

            /* Top Slope Collision. */
            else if (enterCollisionTypes.Contains(CollisionType.TopSlope)) // Heavy Check this.
            {
                if (slopeAngle > CStats.wallAngleMax && slopeAngle < CStats.topAngleMin)
                {
                    topSlopeSpeedCur = velocity;
                    //fsm.ChangeState(States.TopSlope);
                }
                else { Debug.LogError("TopCollision - Invalid Angle"); }

                velocity.y = 0;

                if (!collisionState.Slope && !collisionState.Left && !collisionState.Right && !collisionState.Bot)
                {
                    fsm.ChangeState(CStatesBase.Airborne);
                }
                // Case1.04: climbing slope topcollision (touching top == true, touching slope == true) skips airborne, 
                // directly from slope to top in simulation, skipping the airborne that would be there with finer calculations..
                else if (collisionState.Bot)
                {
                    fsm.ChangeState(CStatesBase.Idle);
                }
                else if (collisionState.Slope)
                {
                    fsm.ChangeState(CStatesBase.TopSlope); // Case 1.06
                }
                else { Debug.LogError("ERROR: State Transition Top Corner"); }

                enterCollisionTypes.Remove(CollisionType.TopSlope);
            }

            /* Steep Slope Collision. */
            else if (enterCollisionTypes.Contains(CollisionType.SteepSlope))
            {
                if (slopeAngle > CStats.slopeAngleMax && slopeAngle < CStats.topAngleMin)
                {
                    fsm.ChangeState(CStatesBase.SteepSlope, StateTransition.Safe);
                }
                else { Debug.LogError("SteepSlope Collision - Invalid Angle"); }
                enterCollisionTypes.Remove(CollisionType.SteepSlope);
            }

            /* Slope Collision. */
            else if (enterCollisionTypes.Contains(CollisionType.Slope))
            {
                /* Calc Top/Bot Slope Collision. */
                if (slopeAngle > CStats.slopeAngleMin && slopeAngle <= CStats.slopeAngleMax)
                {
                    fsm.ChangeState(CStatesBase.ClimbingSlope, StateTransition.Safe);
                }
                else { Debug.LogError("TopCollision - Invalid Angle"); }
                enterCollisionTypes.Remove(CollisionType.Slope);
            }

            /* Undefined Collision. */
            else
            {
                Debug.LogError("" + enterCollisionTypes);
                fsm.ChangeState(CStatesBase.FindState, StateTransition.Safe);
            }
        }
    }

    IEnumerator Action_Enter()
    {
        /* 1. Halts other calculations and states by switching to this state. 
           2. Sets cActionsBase.fsm from Waiting to FindState where the respective actionState is calculated.
           3. When the action is carried out or interrupted, switches to Waiti
           // Must have control of this class's vars
           4. Action continues from previous state.*/

        // TODO: Should wait for all exit functions to finish executing.
        Debug.Log("ACTION - Enter.");
        yield return new WaitForEndOfFrame();// WaitforEndofFrame();
        Debug.LogError("Should be end of frame.");
        cActionsBase.fsm.ChangeState(CActionsBase.CStatesActionsBase.FindState, StateTransition.Safe); // Access the enum of CActionBase.
    }

    void Action_Update()
    {
        Debug.Log("ACTION - Update. ");
        PreStateUpdate();
    }

    void Action_Finally()
    {
        Debug.Log("ACTION - Finally. ");

    }

    void FindState_Enter()
    {
        Debug.LogWarning("FINDSTATE - Enter from " + fsm.LastState);
    }

    void FindState_Update()
    {
        Debug.LogWarning("FINDSTATE - UPDATE");
        PreStateUpdate();
    }

}

// Fianlly: Reset object to desired configuration
// For Overwrite: fsm.ChangeState(States.MyNextState, StateTransition.Safe);

public enum CollisionType
{
    None,
    Top,
    Bot,
    Left,
    Right,
    Slope,
    TopSlope,
    SteepSlope
};