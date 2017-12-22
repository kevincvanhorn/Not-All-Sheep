using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MonsterLove.StateMachine; // State-Machine Package.

// FixedUpdate -> OnTrigger -> OnCollision

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

    /* Define States */
    public enum States {
        FindState,
        Action,
        Idle,
        Airborne,
        OnWall,
        Running,
        Dashing,
        ClimbingSlope
    }

    private enum CollisionType {
        None,
        Top,
        Bot,
        Left,
        Right,
        Slope
    };

    HashSet<CollisionType> enterCollisionTypes = new HashSet<CollisionType>(); // For use in that frame.
    HashSet<CollisionType> exitCollisionTypes = new HashSet<CollisionType>();
    HashSet<CollisionType> collisionTypes = new HashSet<CollisionType>();

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

        /* Calc constants in terms of Jump time and apex height. */
        gravity = -(2 * jumpHeightMax) / Mathf.Pow(timeToJumpApex, 2);
        jumpVelocityMax = Mathf.Abs(gravity * timeToJumpApex);
        jumpVelocityMin = Mathf.Sqrt(2 * Mathf.Abs(gravity) * jumpHeightMin);
    }

    void FixedUpdate() {
        Debug.Log("Main - Fixed Update");
        enterCollisionTypes.Clear();
        exitCollisionTypes.Clear();
        rigidBody.velocity = velocity;

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

    /** Called on Player collision with a new object. **/
    void BaseCollisionEnter2D(Collision2D collision) { // ~ Could convert Collision2D to Collider2D
        ContactPoint2D[] contactsIn = new ContactPoint2D[4]; // 2 when side collides (each corner) || 1 when on slope
        collision.GetContacts(contactsIn);

        /* Add new contact points to hash. */
        for (int i = 0; i < contactsIn.Length; i++) {
            if (contactsIn[i].normal != Vector2.zero) {
                if (!contacts.Contains(contactsIn[i].normal)) {
                    contacts.Add(contactsIn[i].normal);
                    //print(contactsIn[i].normal);
                }
            }
        }

        /* Call Collider Enter Functions */
        for (int i = 0; i < contactsIn.Length; i++) {
            /* If contact exists (entries are zero in larger alocated ContactPoint2D[])*/
            //print("--------------" + slopeAngle + " " + contactsIn[i].normal);
            if (contactsIn[i].normal != Vector2.zero) {
                /* Vertical Collision */
                slopeAngle = Vector2.Angle(contactsIn[i].normal, Vector2.up);
                if (contactsIn[i].normal.x == 0) { // contactsIn[i].normal.x == 0
                    if (contactsIn[i].normal.y == 1) {
                        enterCollisionTypes.Add(CollisionType.Bot); // For this frame.
                        collisionTypes.Add(CollisionType.Bot);      // For all states.
                    }
                    else if (contactsIn[i].normal.y == -1) { // contactsIn[i].normal.y == -1
                        enterCollisionTypes.Add(CollisionType.Top);
                        collisionTypes.Add(CollisionType.Top);
                    }
                }
                /* Horizontal Collision */
                else if (slopeAngle > maxAngle) { // contactsIn[i].normal.y == 0
                    if (contactsIn[i].normal.x > 0) { // contactsIn[i].normal.x == 1
                        enterCollisionTypes.Add(CollisionType.Left);
                        collisionTypes.Add(CollisionType.Left);
                    }
                    else if (contactsIn[i].normal.x < 0) { // contactsIn[i].normal.x == -1
                        enterCollisionTypes.Add(CollisionType.Right);
                        collisionTypes.Add(CollisionType.Right);
                    }
                }
                /* Slope Collision */
                else {
                    slopeDir = (contactsIn[i].normal.x < 0) ? 1 : -1; // 1 = right, -1 = left
                    //slopeAngle = Vector2.Angle(contactsIn[i].normal, Vector2.up);
                    enterCollisionTypes.Add(CollisionType.Slope);
                    collisionTypes.Add(CollisionType.Slope);
                }
            }
        }
    }

    /** Called on Player collision Exit. **/
    void BaseCollisionExit2D(Collision2D collision) { // ~ Could convert Collision2D to Collider2D
        ContactPoint2D[] contactsRB = new ContactPoint2D[4]; // 2 when side collides (each corner) || 1 when on slope
        rigidBody.GetContacts(contactsRB);

        float slopeAngleExit = 0;

        /* Make a hash with the current normals touching the object. */
        HashSet<Vector2> contactNormalsRB = new HashSet<Vector2>();
        foreach (ContactPoint2D c in contactsRB) {
            contactNormalsRB.Add(c.normal);
        }

        HashSet<Vector2> exitContacts = new HashSet<Vector2>();
        exitContacts.UnionWith(contacts);           // exitContacts = contacts

        exitContacts.ExceptWith(contactNormalsRB);  // Set ExitContacts.
        contacts.ExceptWith(exitContacts);          // Remove Exit contacts from Hash.

        /* Call Collider Enter Functions */
        foreach (Vector2 exitContact in exitContacts) {
            /* If contact exists (entries are zero in larger allocated ContactPoint2D[])*/
            if (exitContact != Vector2.zero) {
                slopeAngleExit = Vector2.Angle(exitContact, Vector2.up);
                //print("EXIT --- " + slopeAngleExit);
                /* Vertical Collision */
                if (exitContact.x == 0) {
                    if (exitContact.y == 1) {
                        exitCollisionTypes.Add(CollisionType.Bot);
                        collisionTypes.Remove(CollisionType.Bot);
                    }
                    else if (exitContact.y == -1) {
                        exitCollisionTypes.Add(CollisionType.Top);
                        collisionTypes.Remove(CollisionType.Top);
                    }
                }
                /* Horizontal Collision */
                else if (slopeAngleExit > maxAngle) { //exitContact.y == 0
                    if (exitContact.x > 0) {
                        exitCollisionTypes.Add(CollisionType.Left);
                        collisionTypes.Remove(CollisionType.Left);
                    }
                    else if (exitContact.x < 0) {
                        exitCollisionTypes.Add(CollisionType.Right);
                        collisionTypes.Remove(CollisionType.Right);
                    }
                }
                /* Slope Collision */
                else {
                    exitCollisionTypes.Add(CollisionType.Slope);
                    collisionTypes.Remove(CollisionType.Slope);
                }
            }
        }
    }

    /* Collision Methods: Custom ---------------------------------------------*/
    // Enter - Called immediately when changeState is called (before Main FixedUpdate).
    // Exit
    // FixedUpdate - Called after Main FixedUpdate
    // Collision Enter/Exit
    // Input Events
    // Update
    // LateUpdate
    // Finally

    void Idle_Enter() {
        // velocity.x = 0
        Debug.Log("IDLE - Enter");
    }

    void Idle_FixedUpdate() {
        Debug.Log("IDLE - FixedUpdate");

        /* Vertical JUMP Calc ------------------------------------------ */
        // Jump if pressed or held && not touchingTop (ex: sandwiched between two platforms).
        if (Input.GetKey(KeyCode.UpArrow) && !collisionTypes.Contains(CollisionType.Top)) {
            velocity.y = jumpVelocityMax;
            isGrounded = false;
            fsm.ChangeState(States.Airborne, StateTransition.Safe);
            print("Idle Transition 1");
        }

        /* Lateral Calc -------------------------------------------------- */
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow)) {
            velocity.x = activeSpeed * directionFacing;
            fsm.ChangeState(States.Running, StateTransition.Safe);
            print("Idle Transition 2");
        }

        if (inputManager.ActionKeyPressed()) {
            fsm.ChangeState(States.Action);
            print("Idle Transition 3");
        }
    }

    void Idle_OnCollisionExit2D(Collision2D collision) {
        BaseCollisionExit2D(collision);
    }

    void Airborne_Enter() {
        Debug.Log("AIRBORNE - Enter");
    }

    void Airborne_FixedUpdate() {
        Debug.Log("AIRBORNE - Fixed Update");
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

        velocity.y += gravity * Time.deltaTime; // Apply Gravity until grounded

        // Reduntant Case for platforms moving down while in air. 
        if (collisionTypes.Contains(CollisionType.Top)) {
            velocity.y = 0;
        }

        // Jumping While Against Wall.
        if (collisionTypes.Contains(CollisionType.Right) || collisionTypes.Contains(CollisionType.Left)) {
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
                // Continues execution from here after NextState.Enter() before FixedUpdate() next frame.
            }
            else if (enterCollisionTypes.Contains(CollisionType.Left)) {
                // OnWall.
                enterCollisionTypes.Remove(CollisionType.Left);
                if (!collisionTypes.Contains(CollisionType.Bot)) {
                    fsm.ChangeState(States.OnWall, StateTransition.Overwrite);
                }
                else {
                    velocity.x = 0;
                    print("AIRBORNE: This state should be inaccessible - grounded & touchingWall");
                }
            }
            else if (enterCollisionTypes.Contains(CollisionType.Right)) {
                enterCollisionTypes.Remove(CollisionType.Right);
                if (!collisionTypes.Contains(CollisionType.Bot)) {
                    fsm.ChangeState(States.OnWall, StateTransition.Overwrite);
                }
                else {
                    velocity.x = 0;
                    print("AIRBORNE: This state should be inaccessible - grounded & touchingWall");
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

    void Airborne_OnCollisionExit2D(Collision2D collision) {
        BaseCollisionExit2D(collision);
    }

    void Running_Enter() {
        Debug.Log("RUNNING - Enter");
        /* If Enter State and Collision has not been addressed. */
        /*if (enterCollisionTypes.Count > 0 && enterCollisionTypes.Contains(CollisionType.Left)) {
            velocity.x = 0;
            enterCollisionTypes.Remove(CollisionType.Left); // Addressed this collision so delete.
        }*/
    }

    void Running_FixedUpdate() {
        Debug.Log("RUNNING - FixedUpdate");
        //check conatcts and set velocity.x = 0 should be touching the ground still

        /* Sprint Calc ------------------------------------------------- */
        if (Input.GetKey(KeyCode.LeftShift)) {
            activeSpeed = sprintSpeed;
        }
        else {
            activeSpeed = moveSpeed;
        }

        /* Vertical JUMP Calc ------------------------------------------ */
        // Jump if pressed or held && not touchingTop (ex: sandwiched between two platforms).
        if (Input.GetKey(KeyCode.UpArrow) && !collisionTypes.Contains(CollisionType.Top)) {
            velocity.y = jumpVelocityMax;
            isGrounded = false;
            print("Running Transition 1");
            fsm.ChangeState(States.Airborne, StateTransition.Safe);
        }

        /* Lateral Calc -------------------------------------------------- */
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow)) {
            velocity.x = activeSpeed * directionFacing;
        }

        /* X Acceleration ---------------------------------------------- */
        else if (!Input.GetKey(KeyCode.RightArrow) && !Input.GetKey(KeyCode.LeftArrow)) { // On-release of Lateral Movement controls - Deccelerate
            if (directionFacing == 1 && velocity.x < 0 || directionFacing == -1 && velocity.x > 0) { // Stops deccel when hits 0 from the initial negative(left moving) or pos(right moving) val
                velocity.x = 0;
            }
            if (directionFacing == 1 && velocity.x > 0) { // Decceleration Right
                velocity.x -= lateralAccelGrounded * Time.deltaTime;
            }
            else if (directionFacing == -1 && velocity.x < 0) { // Decceleration Left
                velocity.x += lateralAccelGrounded * Time.deltaTime;
            }
        }

        /* Run/deccelerate into wall - Applied here once instead of conditionals above. */
        if (velocity.x > 0 && collisionTypes.Contains(CollisionType.Right)) {
            velocity.x = 0;
        }
        else if (velocity.x < 0 && collisionTypes.Contains(CollisionType.Left)) {
            velocity.x = 0;
        }

        if (Input.GetKeyDown(KeyCode.RightArrow)) {
            directionFacing = 1;
            if (isGrounded) {
                if (isTouchingRight) {
                    velocity.x = 0;
                }
                else
                    velocity.x = activeSpeed; // since isGrounded

            }

        }

        if (velocity.x == 0 && !Input.GetKey(KeyCode.RightArrow) && !Input.GetKey(KeyCode.LeftArrow)) {
            print("Running Transition 2");
            fsm.ChangeState(States.Idle, StateTransition.Safe);
        }

        // Trigger Action.
        if (inputManager.ActionKeyPressed()) {
            print("Running Transition 3");
            fsm.ChangeState(States.Action);
        }
    }

    void Running_OnCollisionEnter2D(Collision2D collision) {
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
                velocity.y = 0; // Redundancy case - addressed in this.FixedUpdate.
            }
            else {
                fsm.ChangeState(States.FindState, StateTransition.Overwrite);
            }
        }
    }

    void Running_OnCollisionExit2D(Collision2D collision) {
        BaseCollisionExit2D(collision);
    }

    void OnWall_Enter() {
        Debug.Log("ONWALL - Enter");
    }

    void OnWall_FixedUpdate() {
        Debug.Log("ONWALL - FixedUpdate");
        velocity.y += gravity * Time.deltaTime; // Apply Gravity until grounded

        // When Up is first input.
        if (Input.GetKeyDown(KeyCode.UpArrow)) {
            if (isTouchingLeft && Input.GetKey(KeyCode.LeftArrow)) // Jump toward left wall.
            {
                velocity.y = jumpVelocityMax;
                velocity.x = activeSpeed / 2;
            }
            else if (isTouchingRight && Input.GetKey(KeyCode.RightArrow)) // Jump toward right wall.
            {
                velocity.y = jumpVelocityMax;
                velocity.x = -1 * activeSpeed / 2;
            }
        }
        // When Up is released in this frame.
        if (Input.GetKeyUp(KeyCode.UpArrow)) {
            if (velocity.y > jumpVelocityMin) { // Keep applying velocity up while key is pressed - variable jump
                velocity.y = jumpVelocityMin;
            }
        }
        // When Right is first input.
        if (Input.GetKeyDown(KeyCode.RightArrow)) { // on L/R input - setting conditions.
            directionFacing = 1;
            if (isTouchingRight && Input.GetKey(KeyCode.UpArrow)) // Jumping toward right wall.
            {
                velocity.y = jumpVelocityMax;
                velocity.x = -1 * activeSpeed / 2;
            }
        }
        // When Left is first input.
        else if (Input.GetKeyDown(KeyCode.LeftArrow)) {
            directionFacing = -1;
            if (isTouchingLeft && Input.GetKey(KeyCode.UpArrow)) // Jumping toward left wall.
            {
                velocity.y = jumpVelocityMax;
                velocity.x = activeSpeed / 2;
            }
        }

        // When Right or Left is held down.
        if (Input.GetKey(KeyCode.RightArrow)) {
            if (isTouchingLeft) {
                if (Input.GetKey(KeyCode.UpArrow)) // Jump away from left wall.
                {
                    velocity.y = jumpVelocityMax;
                    velocity.x = activeSpeed;
                }
                else // Fall away from wall
                    velocity.x += lateralAccelAirborne * Time.deltaTime;
            }

        }
        else if (Input.GetKey(KeyCode.LeftArrow)) {
            if (isTouchingRight) {
                if (Input.GetKey(KeyCode.UpArrow)) // Jump away from right wall.
                    {
                    velocity.y = jumpVelocityMax;
                    velocity.x = -1 * activeSpeed / 2;
                }
                else // Fall away from wall
                    velocity.x -= lateralAccelAirborne * Time.deltaTime;
            }
        }

    }
}

// Fianlly: Reset object to desired configuration
// For Overwrite: fsm.ChangeState(States.MyNextState, StateTransition.Overwrite);