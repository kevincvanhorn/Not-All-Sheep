using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MonsterLove.StateMachine; // State-Machine Package.

// Update -> OnTrigger -> OnCollision

[RequireComponent(typeof(CInputManager))]
[RequireComponent(typeof(CCollisionState))]
[RequireComponent(typeof(Rigidbody2D))]
public class CharacterBase : MonoBehaviour {

    Rigidbody2D rigidBody; // Not Kinematic: moves not by transform, but by physics

    /* Collisions Vars */
    public bool isGrounded;

    // note: 3 states- left, right, and still: requires two variables
    public bool isRunning;
    public bool isTouchingTop;
    public bool isTouchingRight;
    public bool isTouchingLeft;
    public bool isTouchingBot;
    public bool onWall;
    public bool onSlope;

    /* Colliders */
    HashSet<Vector2> contacts = new HashSet<Vector2>();

    /* Movement Variables */
    public float moveSpeed = 10;    // Horizontal speed.
    public float moveSpeedMin = 5;
    public float sprintSpeed = 20;
    public float activeSpeed;
    public Vector3 velocity;

    float wallImpactSpeed;
    float directionFacing = 1;

    /* Jump Variables */
    public float lateralAccelAirborne = 60;
    public float lateralAccelGrounded = 100;

    public float jumpHeightMax = 5;
    public float jumpHeightMin = .9f;
    public float timeToJumpApex = .4f;

    float gravity;
    float jumpVelocityMax;
    float jumpVelocityMin;

    /* Slope Variables */
    public float slopeDir;
    public float slopeAngle = 0;
    public float maxAngle = 80;

    CInputManager inputManager;

    public enum CollisionType {
        None,
        Top,
        Bot,
        Left,
        Right,
        Slope
    };

    /* Define States */
    public enum States {
        FindState,
        Action,
        Idle,
        Airborne,
        OnWall,
        Running,
        Dashing,
        ClimbingSlope,
        Simulate
    }

    CCollisionState collisionState;

    HashSet<CollisionType> enterCollisionTypes = new HashSet<CollisionType>(); // For use in that frame.
    //HashSet<CollisionType> collisionTypes; // For use in that frame.

    private StateMachine<States> fsm;

    public void Awake() {
        // Initialize State Machine Engine		
        fsm = StateMachine<States>.Initialize(this, States.Airborne);
    }

    public void Start() {
        /* Set collision defaults. */
        isTouchingTop = false;
        isTouchingRight = false;
        isTouchingLeft = false;
        isGrounded = false;
        isRunning = false;
        isTouchingBot = false;
        onSlope = false;

        activeSpeed = moveSpeed;
        wallImpactSpeed = activeSpeed;
        rigidBody = GetComponent<Rigidbody2D>();

        inputManager = GetComponent<CInputManager>();
        collisionState = GetComponent<CCollisionState>();

        /* Calc constants in terms of Jump time and apex height. */
        gravity = -(2 * jumpHeightMax) / Mathf.Pow(timeToJumpApex, 2);
        jumpVelocityMax = Mathf.Abs(gravity * timeToJumpApex);
        jumpVelocityMin = Mathf.Sqrt(2 * Mathf.Abs(gravity) * jumpHeightMin);
    }

    void PreStateUpdate() {
        /* Update directionFacing ------------------------------------------ */
        if (Input.GetKeyDown(KeyCode.RightArrow)) {
            directionFacing = 1;
        }
        // When Left is first input.
        else if (Input.GetKeyDown(KeyCode.LeftArrow)) {
            directionFacing = -1;
        }
        else if (Input.GetKey(KeyCode.LeftArrow) && !Input.GetKey(KeyCode.RightArrow)) {
            directionFacing = -1;
        }
        else if (Input.GetKey(KeyCode.RightArrow) && !Input.GetKey(KeyCode.LeftArrow)) {
            directionFacing = 1;
        }
    }

    void Update() {
        rigidBody.velocity = velocity;
    }


    /** Called on Player collision with a new object. **/
    void BaseCollisionEnter2D(Collision2D collision) { // ~ Could convert Collision2D to Collider2D
        PreStateUpdate();
        enterCollisionTypes.Clear();
        collisionState.CheckOverlaps();

        ContactPoint2D[] contactsIn = new ContactPoint2D[4]; // 2 when side collides (each corner) || 1 when on slope
        collision.GetContacts(contactsIn);
        /*Debug.LogError("BASECOLLISIONENTER");
        foreach (ContactPoint2D e in contactsIn) {
            Debug.LogError(e.normal);
        }
        Debug.LogError("----------");*/

        /* Call Collider Enter Functions */
        for (int i = 0; i < contactsIn.Length; i++) {
            /* If contact exists (entries are zero in larger alocated ContactPoint2D[])*/
            if (contactsIn[i].normal != Vector2.zero) {
                slopeAngle = Vector2.Angle(contactsIn[i].normal, Vector2.down);
                /* Wall Collision */
                if (slopeAngle < 91 && slopeAngle > 89) {
                    if (contactsIn[i].normal.x > 0) {
                        enterCollisionTypes.Add(CollisionType.Left);
                        collisionState.left = true;
                        collisionState.none = false;
                    }
                    if (contactsIn[i].normal.x < 0) {
                        enterCollisionTypes.Add(CollisionType.Right);
                        collisionState.none = false;
                        collisionState.right = true;
                    }
                }
                /* Top Collision*/
                else if (slopeAngle >= 0 && slopeAngle < 5) {
                    if (contactsIn[i].normal.x > 0) { // contactsIn[i].normal.x == 1
                        enterCollisionTypes.Add(CollisionType.Top);
                        collisionState.top = true;
                        collisionState.none = false;
                    }
                }
                else if (slopeAngle == 180) { // contactsIn[i].normal.y == -1
                    enterCollisionTypes.Add(CollisionType.Bot);
                    collisionState.bot = true;
                    collisionState.none = false;
                }
                /* Slope Collision */
                else { // This is now bot.
                    Debug.LogError(slopeAngle);
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
    }

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
    void Idle_Enter() {
        // velocity.x = 0
        Debug.Log("IDLE - Enter");
    }

    void Idle_Update() {
        Debug.Log("IDLE - Update");
        PreStateUpdate();

        //collisionState.printStatesError();

        /* Vertical JUMP Calc ------------------------------------------ */
        // Jump if pressed or held && not touchingTop (ex: sandwiched between two platforms).
        if (Input.GetKey(KeyCode.UpArrow) && !collisionState.Top) {
            velocity.y = jumpVelocityMax;
            fsm.ChangeState(States.Simulate, StateTransition.Safe);
        }

        /*if (!collisionState.Bot || !collisionState.Slope) {
            fsm.ChangeState(States.Simulate, StateTransition.Safe);
        }*/

        /* Lateral Calc -------------------------------------------------- */
        else if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow)) {
            if (collisionState.Slope) {
                fsm.ChangeState(States.ClimbingSlope, StateTransition.Safe);
            }
            else if(collisionState.Bot) {
                fsm.ChangeState(States.Running, StateTransition.Safe);
            }
            else {
                Debug.LogError("ERROR: Invalid Idle Transition.");
                collisionState.printStatesError();
            }
        }

        if (inputManager.ActionKeyPressed()) {
            fsm.ChangeState(States.Action);
        }
    }

    void Idle_OnCollisionEnter2D(Collision2D collision) {
        BaseCollisionEnter2D(collision);
    }

    void Airborne_Enter() {
        Debug.Log("AIRBORNE - Enter");
    }

    void Airborne_Update() {
        Debug.Log("AIRBORNE -  Update");
        PreStateUpdate();

        /* Vertical Calc ----------------------------------------- */
        if (Input.GetKeyUp(KeyCode.UpArrow)) {  // Variable jump - When Up is released in this frame.
            if (velocity.y > jumpVelocityMin) {
                velocity.y = jumpVelocityMin;
            }
        }

        /* Lateral Calc -------------------------------------------*/
        if (Input.GetKey(KeyCode.RightArrow) && velocity.x < activeSpeed) { // in-air lateral move right
            velocity.x += lateralAccelAirborne * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.LeftArrow) && velocity.x > -activeSpeed) { // in-air lateral move left
            velocity.x -= lateralAccelAirborne * Time.deltaTime;
        }

        if (collisionState.Top && velocity.y > 0) {
            velocity.y = 0;
        }

        velocity.y += gravity * Time.deltaTime; // Apply Gravity until grounded

        // Jumping While Against Wall.
        if (collisionState.Right || collisionState.Left) {
            /*print("CollisionState -------------");
            collisionState.printStates();
            print("----------------------------");*/
            velocity.x = 0;
            fsm.ChangeState(States.OnWall);
        }

        // Trigger Action.
        if (inputManager.ActionKeyPressed()) {
            fsm.ChangeState(States.Action);
        }
    }

    void Airborne_OnCollisionEnter2D(Collision2D collision) {
        BaseCollisionEnter2D(collision);

        Debug.Log("AIRBORNE - OnCollisionEnter");
        /* These are the new collisions this frame from this specific collision. */
        // ? Iterate for all combinations not needed with contains.
        if (enterCollisionTypes.Count > 0) {
            // Grounded.
            if (enterCollisionTypes.Contains(CollisionType.Bot)) {
                velocity.y = 0;
                enterCollisionTypes.Remove(CollisionType.Bot); // Addressed this collision so delete.
                if (velocity.x == 0) {
                    fsm.ChangeState(States.Idle, StateTransition.Overwrite);
                }
                else {
                    fsm.ChangeState(States.Running, StateTransition.Overwrite);
                }
                // Continues execution from here after NextState.Enter() before Update() next frame.
            }
            else if (enterCollisionTypes.Contains(CollisionType.Slope)) {
                velocity.y = 0;
                velocity.x = 0;
                // TODO : make x and y relate to the angle?
                enterCollisionTypes.Remove(CollisionType.Slope);
                fsm.ChangeState(States.ClimbingSlope, StateTransition.Overwrite);
            }
            else if (enterCollisionTypes.Contains(CollisionType.Left)) {
                velocity.x = 0;
                // OnWall.
                enterCollisionTypes.Remove(CollisionType.Left);
                if (!collisionState.Bot) {
                    fsm.ChangeState(States.OnWall, StateTransition.Overwrite);
                }
                else {
                    velocity.x = 0;
                    Debug.LogWarning("AIRBORNE: This state should be inaccessible - grounded & touchingWall");
                }
            }
            else if (enterCollisionTypes.Contains(CollisionType.Right)) {
                velocity.x = 0;
                enterCollisionTypes.Remove(CollisionType.Right);
                if (!collisionState.Bot) {
                    fsm.ChangeState(States.OnWall, StateTransition.Overwrite);
                }
                else {
                    velocity.x = 0;
                    Debug.LogWarning("AIRBORNE: This state should be inaccessible - grounded & touchingWall");
                }
            }
            else if (enterCollisionTypes.Contains(CollisionType.Top)) {
                enterCollisionTypes.Remove(CollisionType.Top);
                velocity.y = 0;
            }
            else {
                fsm.ChangeState(States.FindState, StateTransition.Overwrite);
            }
        }

    }

    void Running_Enter() {
        Debug.Log("RUNNING - Enter");
        /* If Enter State and Collision has not been addressed. */
        /*if (enterCollisionTypes.Count > 0 && enterCollisionTypes.Contains(CollisionType.Left)) {
            velocity.x = 0;
            enterCollisionTypes.Remove(CollisionType.Left); // Addressed this collision so delete.
        }*/
    }

    void Running_Update() {
        Debug.Log("RUNNING - Update");
        PreStateUpdate();

        //check contacts and set velocity.x = 0 should be touching the ground still

        /* Sprint Calc ------------------------------------------------- */
        if (Input.GetKey(KeyCode.LeftShift)) {
            activeSpeed = sprintSpeed;
        }
        else {
            activeSpeed = moveSpeed;
        }

        /* Lateral Calc -------------------------------------------------- */
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow)) {
            velocity.x = activeSpeed * directionFacing;
        }

        /* X Acceleration ---------------------------------------------- */
        else if (!Input.GetKey(KeyCode.RightArrow) && !Input.GetKey(KeyCode.LeftArrow)) { // On-release of Lateral Movement controls - Deccelerate
            velocity.x = 0;
            /*if (directionFacing == 1 && velocity.x < 0 || directionFacing == -1 && velocity.x > 0) { // Stops deccel when hits 0 from the initial negative(left moving) or pos(right moving) val
                velocity.x = 0;
            }
            if (directionFacing == 1 && velocity.x > 0) { // Decceleration Right
                velocity.x -= lateralAccelGrounded * Time.deltaTime;
            }
            else if (directionFacing == -1 && velocity.x < 0) { // Decceleration Left
                velocity.x += lateralAccelGrounded * Time.deltaTime;
            }*/
        }

        /* Run/deccelerate into wall - Applied here once instead of conditionals above. */
        if (velocity.x > 0 && collisionState.Right) {
            velocity.x = 0;
        }
        else if (velocity.x < 0 && collisionState.Left) {
            velocity.x = 0;
        }

        /*if (Input.GetKeyDown(KeyCode.RightArrow)) {
            directionFacing = 1;
            if (isGrounded) {
                if (isTouchingRight) {
                    velocity.x = 0;
                }
                else
                    velocity.x = activeSpeed; // since isGrounded
            }
        }*/

        /* Priority Cases*/
        if (inputManager.ActionKeyPressed()) { // Trigger Action.
            print("Running Transition 3");
            fsm.ChangeState(States.Action);
        }

        else if (collisionState.None) { // Case - slide off edge
            fsm.ChangeState(States.Simulate, StateTransition.Safe);
        }

        /* Vertical JUMP Calc ------------------------------------------ */
        // Jump if pressed or held && not touchingTop (ex: sandwiched between two platforms).
        else if (Input.GetKey(KeyCode.UpArrow) && !collisionState.Top) {
            velocity.y = jumpVelocityMax;
            isGrounded = false;
            print("Running Transition 1");
            fsm.ChangeState(States.Simulate, StateTransition.Safe);
        }
        else if (velocity.x == 0 && !Input.GetKey(KeyCode.RightArrow) && !Input.GetKey(KeyCode.LeftArrow)) {
            Debug.Log("Running Transition 2");
            fsm.ChangeState(States.Idle, StateTransition.Safe);
            
        }
    }

    void Running_OnCollisionEnter2D(Collision2D collision)  {
        BaseCollisionEnter2D(collision);

        Debug.Log("RUNNING - OnCollisionEnter");
        /* These are the new collisions this frame from this specific collision. */
        // ? Iterate for all combinations not needed with contains.
        if (enterCollisionTypes.Count > 0) {
            if (enterCollisionTypes.Contains(CollisionType.Right)) {
                // TouchingWall.
                enterCollisionTypes.Remove(CollisionType.Right);
                velocity.x = 0;
            }
            else if (enterCollisionTypes.Contains(CollisionType.Left)) {
                // TouchingWall.
                enterCollisionTypes.Remove(CollisionType.Left);
                velocity.x = 0;
            }
            else if (enterCollisionTypes.Contains(CollisionType.Top)) {
                velocity.y = 0; // Redundancy case - addressed in this.Update.
                enterCollisionTypes.Remove(CollisionType.Top);
            }
            else if (enterCollisionTypes.Contains(CollisionType.Slope)) {
                fsm.ChangeState(States.ClimbingSlope, StateTransition.Overwrite);
                enterCollisionTypes.Remove(CollisionType.Slope);
            }
            else {
                fsm.ChangeState(States.FindState, StateTransition.Overwrite);
            }
        }
    }

    void OnWall_Enter() {
        Debug.Log("ONWALL - Enter");
    }

    void OnWall_Update() {
        Debug.Log("ONWALL - Update");
        PreStateUpdate();

        bool isTouchingLeft = collisionState.Left;
        bool isTouchingRight = collisionState.Right;

        if (collisionState.Slope) {
            velocity.y = 0;
            velocity.x = 0;
        }
        else if (!collisionState.Right && !collisionState.Left) { // Case - slide off edge
            fsm.ChangeState(States.Simulate, StateTransition.Safe);
        }
        else {
            velocity.y += gravity * Time.deltaTime; // Apply Gravity until grounded

            // Only Touching one side.
            if (!(isTouchingLeft && isTouchingRight)) {

                // When Up is released in this frame.
                if (Input.GetKeyUp(KeyCode.UpArrow)) {
                    if (velocity.y > jumpVelocityMin) { // Keep applying velocity up while key is pressed - variable jump
                        velocity.y = jumpVelocityMin;
                        //fsm.ChangeState(States.Airborne, StateTransition.Safe);
                    }
                }
                // When Up is first input.
                if (Input.GetKeyDown(KeyCode.UpArrow)) {
                    if (isTouchingLeft && Input.GetKey(KeyCode.LeftArrow)) // Jump toward left wall.
                    {
                        velocity.y = jumpVelocityMax;
                        velocity.x = activeSpeed / 2;
                        fsm.ChangeState(States.Simulate, StateTransition.Safe);
                    }
                    else if (isTouchingRight && Input.GetKey(KeyCode.RightArrow)) // Jump toward right wall.
                    {
                        velocity.y = jumpVelocityMax;
                        velocity.x = -1 * activeSpeed / 2;
                        fsm.ChangeState(States.Simulate, StateTransition.Safe);
                    }
                }

                // When Right is first input.
                else if (Input.GetKeyDown(KeyCode.RightArrow)) { // on L/R input - setting conditions.
                    directionFacing = 1;
                    if (isTouchingRight && Input.GetKey(KeyCode.UpArrow)) // Jumping toward right wall.
                    {
                        velocity.y = jumpVelocityMax;
                        velocity.x = -1 * activeSpeed / 2;
                        fsm.ChangeState(States.Simulate, StateTransition.Safe);
                    }
                }
                // When Left is first input.
                else if (Input.GetKeyDown(KeyCode.LeftArrow)) {
                    print("WALL - State Change 1");
                    directionFacing = -1;
                    if (isTouchingLeft && Input.GetKey(KeyCode.UpArrow)) // Jumping toward left wall.
                    {
                        velocity.y = jumpVelocityMax;
                        velocity.x = activeSpeed / 2;
                        fsm.ChangeState(States.Simulate, StateTransition.Safe);
                    }
                }

                // When Right or Left is held down.
                else if (Input.GetKey(KeyCode.RightArrow)) {
                    if (isTouchingLeft) {
                        if (Input.GetKey(KeyCode.UpArrow)) // Jump away from left wall.
                        {
                            velocity.y = jumpVelocityMax;
                            velocity.x = activeSpeed;
                            fsm.ChangeState(States.Simulate, StateTransition.Safe);
                        }
                        else { // Fall away from wall
                            velocity.x += lateralAccelAirborne * Time.deltaTime;
                            fsm.ChangeState(States.Simulate, StateTransition.Safe);
                        }
                    }
                    else if (isTouchingRight && Input.GetKey(KeyCode.UpArrow)) // Jumping toward right wall.
                    {
                        // When coming from a non-grounded state, immediately jump when hit wall
                        if (velocity.y < 0) {
                            velocity.y = jumpVelocityMax;
                            velocity.x = -1 * activeSpeed / 2;
                            fsm.ChangeState(States.Simulate, StateTransition.Safe);
                        }
                    }

                }
                else if (Input.GetKey(KeyCode.LeftArrow)) {
                    if (isTouchingRight) {
                        print("WALL - State Change 2");
                        if (Input.GetKey(KeyCode.UpArrow)) // Jump away from right wall.
                            {
                            print("WALL - State Change 3");
                            velocity.y = jumpVelocityMax;
                            velocity.x = -1 * activeSpeed / 2;
                            fsm.ChangeState(States.Simulate, StateTransition.Safe);
                        }
                        else { // Fall away from wall
                            velocity.x -= lateralAccelAirborne * Time.deltaTime;
                            fsm.ChangeState(States.Simulate, StateTransition.Safe);
                        }
                    }
                    else if (isTouchingLeft && Input.GetKey(KeyCode.UpArrow)) // Jumping toward left wall.
                    {
                        // When coming from a non-grounded state, immediately jump when hit wall
                        if (velocity.y < 0) {
                            velocity.y = jumpVelocityMax;
                            velocity.x = activeSpeed / 2;
                            fsm.ChangeState(States.Simulate, StateTransition.Safe);
                        }
                    }
                }
            }
        }
        //collisionState.printStatesShort();
        //print("ONWALL - End of Update. Vel " + velocity);
    }

    void OnWall_OnCollisionEnter2D(Collision2D collision) {
        BaseCollisionEnter2D(collision);
        Debug.Log("ONWALL - OnCollisionEnter");

        if (enterCollisionTypes.Count > 0) {
            if (enterCollisionTypes.Contains(CollisionType.Bot)) {
                velocity.y = 0;
                enterCollisionTypes.Remove(CollisionType.Bot); // Addressed this collision so delete.
                if (velocity.x == 0) {
                    fsm.ChangeState(States.Idle, StateTransition.Safe);
                }
                else {
                    fsm.ChangeState(States.Running, StateTransition.Safe);
                }
                // Continues execution from here after NextState.Enter() before Update() next frame.
            }
            else if (enterCollisionTypes.Contains(CollisionType.Slope)) {
                velocity.x = 0;
                velocity.y = 0;
                fsm.ChangeState(States.ClimbingSlope, StateTransition.Safe);
                enterCollisionTypes.Remove(CollisionType.Slope);
            }
            else if (enterCollisionTypes.Contains(CollisionType.Top)) {
                velocity.y = 0;
                enterCollisionTypes.Remove(CollisionType.Top);
            }
            else if (enterCollisionTypes.Contains(CollisionType.Left)) {
                wallImpactSpeed = velocity.x;
                velocity.x = 0;

                enterCollisionTypes.Remove(CollisionType.Left);
                Debug.LogWarning("This should not usually occur. Addressed in Update.");
            }
            else if (enterCollisionTypes.Contains(CollisionType.Right)) {
                wallImpactSpeed = velocity.x;
                velocity.x = 0;

                enterCollisionTypes.Remove(CollisionType.Right);
                Debug.LogWarning("This should not usually occur. Addressed in Update.");
            }
            else {
                fsm.ChangeState(States.FindState, StateTransition.Safe);
            }
        }
    }

    void ClimbingSlope_Enter() {
        Debug.Log("SLOPE - Enter");
    }

    void ClimbingSlope_Update() {
        PreStateUpdate();
        Debug.Log("SLOPE - Update ");
        //Debug.Log("dir: " + directionFacing + " R/L:" + Input.GetKey(KeyCode.RightArrow) +" "+Input.GetKey(KeyCode.LeftArrow));
        collisionState.printStatesShort();

        slopeAngle = collisionState.curSlopeAngle;

        /* Sprint Calc ------------------------------------------------- */
        if (Input.GetKey(KeyCode.LeftShift)) {
            activeSpeed = sprintSpeed;
        }
        else {
            activeSpeed = moveSpeed;
        }

        /* Lateral Calc -------------------------------------------------- */
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow)) {
            if(collisionState.Right && Input.GetKey(KeyCode.RightArrow)) {
                velocity.x = 0;
                velocity.y = 0;
            }
            else if (collisionState.Left && Input.GetKey(KeyCode.LeftArrow)) {
                velocity.x = 0;
                velocity.y = 0;
            }
            else {
                Debug.Log("SLOPE - Lateral Calc. ");
                velocity.x = activeSpeed * Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * directionFacing;
                velocity.y = activeSpeed * Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * slopeDir * directionFacing;
            }
            
        }

        /* X Acceleration ---------------------------------------------- */
        else if (!Input.GetKey(KeyCode.RightArrow) && !Input.GetKey(KeyCode.LeftArrow)) { // On-release of Lateral Movement controls - Deccelerate
            velocity.x = 0;
            velocity.y = 0;
            /*if (directionFacing == 1 && velocity.x < 0 || directionFacing == -1 && velocity.x > 0) { // Stops deccel when hits 0 from the initial negative(left moving) or pos(right moving) val
                velocity.x = 0;
                velocity.y = 0;
            }
            if (directionFacing == 1 && velocity.x > 0) { // Decceleration Right //Journal: 12-27-17
                velocity.x -= lateralAccelGrounded * Mathf.Cos(slopeAngle * Mathf.Deg2Rad)* Time.deltaTime;
                velocity.y += slopeDir * -1 * lateralAccelGrounded * Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * Time.deltaTime;
            }
            else if (directionFacing == -1 && velocity.x < 0) { // Decceleration Left
                velocity.x += lateralAccelGrounded * Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * Time.deltaTime;
                velocity.y += slopeDir *lateralAccelGrounded * Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * Time.deltaTime;
            }
            else if(velocity.y != 0){
                Debug.LogWarning("SLOPE - Decel stopping early. ");
                velocity.y = 0;
                velocity.x = 0;
            }*/
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

        if (collisionState.Top) { //NOTE: Do not else with above, uses calculated velocity.
            if(velocity.y > 0) {
                velocity.x = 0;
                velocity.y = 0;
            }
        }

        /* Priority Cases*/
        if (inputManager.ActionKeyPressed()) { // Trigger Action.
            fsm.ChangeState(States.Action);
        }
        else if (collisionState.None) { // Case - slide off edge
            fsm.ChangeState(States.Simulate, StateTransition.Safe);
        }
        else if ((collisionState.Left || collisionState.Right) && !collisionState.Slope) { // Ran up slope and skid up wall
            // Case1.03: not on slope - just above at corner of slope and wall. 
            if (Input.GetKey(KeyCode.UpArrow) && !collisionState.Top) {
                //Debug.LogError("Case1.03");
                velocity.y = jumpVelocityMax;
                fsm.ChangeState(States.OnWall, StateTransition.Safe);
            }
        }

        /* Vertical JUMP Calc ------------------------------------------ */
        // Jump if pressed or held && not touchingTop (ex: sandwiched between two platforms).
        else if (Input.GetKey(KeyCode.UpArrow) && !collisionState.Top) {
            //NOTE! Remember to copy this jump behavior to the Case1.03 Above
            velocity.y = jumpVelocityMax;
            fsm.ChangeState(States.Simulate, StateTransition.Safe);
        }
        else if (velocity.x == 0 && !Input.GetKey(KeyCode.RightArrow) && !Input.GetKey(KeyCode.LeftArrow)) {
            
            velocity.y = 0;
            if (collisionState.Slope) {
                Debug.Log("Slope - Transition 1");
            }
            else{
                Debug.Log("Slope - Transition 2");
            }

            //fsm.ChangeState(States.Idle, StateTransition.Safe);
        }

       Debug.Log(velocity);
    }

    private void ClimbingSlope_OnCollisionEnter2D(Collision2D collision) {
        Debug.Log("SLOPE - OnCollisionEnter2D");
        BaseCollisionEnter2D(collision);

        if (enterCollisionTypes.Count > 0) {
            if (enterCollisionTypes.Contains(CollisionType.Right)) {
                // TouchingWall.
                velocity.x = 0;
                velocity.y = 0;
                enterCollisionTypes.Remove(CollisionType.Right);
            }
            else if (enterCollisionTypes.Contains(CollisionType.Left)) {
                // TouchingWall.
                velocity.x = 0;
                velocity.y = 0;
                enterCollisionTypes.Remove(CollisionType.Left);
            }
            else if (enterCollisionTypes.Contains(CollisionType.Top)) {
                velocity.x = 0;
                velocity.y = 0; // Redundancy case - addressed in this.Update.
                enterCollisionTypes.Remove(CollisionType.Top);
            }
            else if (enterCollisionTypes.Contains(CollisionType.Slope)) {
                //fsm.ChangeState(States.ClimbingSlope, StateTransition.Overwrite);
                enterCollisionTypes.Remove(CollisionType.Slope);
            }
            else if (enterCollisionTypes.Contains(CollisionType.Bot)) {
                fsm.ChangeState(States.Running, StateTransition.Overwrite);
                enterCollisionTypes.Remove(CollisionType.Slope);
            }
            else {
                fsm.ChangeState(States.FindState, StateTransition.Overwrite);
            }
        }
    }

    /* Simulate is for the 4-5 frames after a jump/transition away from an object into empty space occurs.
     Needed for the collision state to catch up so that actions like airborne checking if it's touching the floor
     in an update does not occur immediately at the first frame of up pressed out of the grounded state while the object is 
     still "grounded" by the bounding check.*/
    void Simulate_Enter() {
        Debug.Log("SIMULATE - Enter from" + fsm.LastState);
    }

    void Simulate_Update() {
        Debug.Log("Simulate_Update");
        PreStateUpdate();

        /* Lateral Calc -------------------------------------------*/
        if (Input.GetKey(KeyCode.RightArrow) && velocity.x < activeSpeed) { // in-air lateral move right
            velocity.x += lateralAccelAirborne * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.LeftArrow) && velocity.x > -activeSpeed) { // in-air lateral move left
            velocity.x -= lateralAccelAirborne * Time.deltaTime;
        }
        velocity.y += gravity * Time.deltaTime; // Apply Gravity until grounded

        // Trigger Action.
        if (inputManager.ActionKeyPressed()) {
            fsm.ChangeState(States.Action);
        }
        else if (fsm.LastState == States.Running) {
            if (!collisionState.Bot) {
                fsm.ChangeState(States.Airborne);
            }
        }
        else if (fsm.LastState == States.Idle) {
            if (!collisionState.Bot) {
                fsm.ChangeState(States.Airborne);
            }
            else if (!collisionState.Slope) {
                fsm.ChangeState(States.Airborne);
            }
        }
        else if (fsm.LastState == States.OnWall) {
            if (!collisionState.Left && !collisionState.Right) {
                fsm.ChangeState(States.Airborne);
            }
            if (collisionState.Bot) {
                fsm.ChangeState(States.Idle);
            }
            else if (collisionState.Slope) {
                fsm.ChangeState(States.ClimbingSlope);
            }
        }
        else if (fsm.LastState == States.ClimbingSlope) {
            if (!collisionState.Slope) {
                fsm.ChangeState(States.Airborne);
            }
        }
        else {
            Debug.LogWarning("Simulate_Update: State Simulate not defined from " + fsm.LastState);
        }

    }

    void Simulate_OnCollisionEnter2D(Collision2D collision) {
        Debug.Log("Simulate - OnCollisionEnter from " + fsm.LastState);
        BaseCollisionEnter2D(collision);

        /*collisionState.printStatesError();
        Debug.LogError("TOP " + enterCollisionTypes.Contains(CollisionType.Top) + " SLOPE " + enterCollisionTypes.Contains(CollisionType.Slope));
        foreach (CollisionType e in enterCollisionTypes) {
            Debug.LogError("---- " + e);
        }
        foreach (ContactPoint2D c in collision.contacts) {
            Debug.LogError("THIS - " + c.normal);
        }*/


        /* These are the new collisions this frame from this specific collision. */
        // ? Iterate for all combinations not needed with contains.
        if (enterCollisionTypes.Count > 0) {
            // Grounded.
            if (enterCollisionTypes.Contains(CollisionType.Bot)) {
                velocity.y = 0;
                enterCollisionTypes.Remove(CollisionType.Bot); // Addressed this collision so delete.
                if (velocity.x == 0) {
                    fsm.ChangeState(States.Idle, StateTransition.Overwrite);
                }
                else {
                    fsm.ChangeState(States.Running, StateTransition.Overwrite);
                }
                // Continues execution from here after NextState.Enter() before Update() next frame.
            }
            else if (enterCollisionTypes.Contains(CollisionType.Left)) {
                // OnWall.
                enterCollisionTypes.Remove(CollisionType.Left);
                if (!collisionState.Bot) {
                    fsm.ChangeState(States.OnWall, StateTransition.Overwrite);
                }
                else {
                    velocity.x = 0;
                    Debug.LogWarning("SIMULATE: This state should be inaccessible - grounded & touchingWall");
                }
            }
            else if (enterCollisionTypes.Contains(CollisionType.Right)) {
                enterCollisionTypes.Remove(CollisionType.Right);
                if (!collisionState.Bot) {
                    fsm.ChangeState(States.OnWall, StateTransition.Overwrite);
                }
                else {
                    velocity.x = 0;
                    Debug.LogWarning("SIMULATE: This state should be inaccessible - grounded & touchingWall");
                }
            }
            else if (enterCollisionTypes.Contains(CollisionType.Top)) {
                velocity.y = 0;
                if (!collisionState.Slope && !collisionState.Left && !collisionState.Right && !collisionState.Bot) {
                    fsm.ChangeState(States.Airborne);
                }
                // Case1.04: climbing slope topcollision (touching top == true, touching slope == true) skips airborne, 
                // directly from slope to top in simulation, skipping the airborne that would be there with finer calculations..
                else if (collisionState.Slope) {
                    fsm.ChangeState(States.ClimbingSlope);
                }
                else {
                    Debug.LogError("ERROR: State Transition Top Corner");
                }
                
                /*if (fsm.LastState == States.ClimbingSlope) {
                    velocity.x = 0;
                    fsm.ChangeState(States.ClimbingSlope);
                }*/
                enterCollisionTypes.Remove(CollisionType.Top);
            }
            else if (enterCollisionTypes.Contains(CollisionType.Slope)) {
                enterCollisionTypes.Remove(CollisionType.Slope);
                fsm.ChangeState(States.ClimbingSlope, StateTransition.Overwrite);
            }
            else {
                Debug.LogError(""+enterCollisionTypes);
                fsm.ChangeState(States.FindState, StateTransition.Overwrite);
            }
        }
    }

    void FindState_Enter() {
        Debug.LogWarning("FINDSTATE - Enter from " + fsm.LastState);
    }

    void FindState_Update() {
        PreStateUpdate();
    }

}

// Fianlly: Reset object to desired configuration
// For Overwrite: fsm.ChangeState(States.MyNextState, StateTransition.Overwrite);