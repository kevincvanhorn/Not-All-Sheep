using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// {System.AppDomain.CurrentDomain.SetData("break",true)}
// System.AppDomain.CurrentDomain.GetData("break") != null

/**
 * General Character Platformer Controller - Parent of DeathForm, LifeForm, and ReaperForm MoveDrivers
 * (DFMoveDriver, LFMoveDriver, RFMoveDriver)
 */

[RequireComponent(typeof(Rigidbody2D))]
//[RequireComponent(typeof(Controller2D))]
public class CharacterMoveDriver : MonoBehaviour {

    //Controller2D controller;
    Rigidbody2D rigidBody; // Not Kinematic: moves not by transform, but by physics

    /* Collisions Vars */
    public bool isGrounded;

    // note: 3 states- left, right, and still: requires two variables
    public bool isSprinting;
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

    /* Define States */
    public enum MoveState {
        Idle,
        Jumping,
        Falling,
        WallRising,
        WallFalling,
        WallSticking,
        Sprinting,
        Dashing,
        ClimbingSlope
    }

    public MoveState moveState { get; private set; }
    public MoveState prevState { get; private set; }

    /* State Accessors */
    public bool IsIdle() { return moveState == MoveState.Idle; }
    public bool IsJumping() { return moveState == MoveState.Jumping; }
    public bool IsFalling() { return moveState == MoveState.Falling; }
    public bool IsWallRising() { return moveState == MoveState.WallRising; }
    public bool IsWallFalling() { return moveState == MoveState.WallFalling; }
    public bool IsWallSticking() { return moveState == MoveState.WallSticking; }
    public bool IsDashing() { return moveState == MoveState.Dashing; }
    public bool IsSprinting() { return moveState == MoveState.Sprinting; }
    public bool IsClimbingSlope() { return moveState == MoveState.ClimbingSlope; }

    void Start() {
        /* Set collision defaults. */
        isTouchingTop = false;
        isTouchingRight = false;
        isTouchingLeft = false;
        isGrounded = false;
        isSprinting = false;
        isTouchingBot = false;
        onSlope = false;

        activeSpeed = moveSpeed;
        wallImpactSpeed = activeSpeed;
        rigidBody = GetComponent<Rigidbody2D>();

        /* Calc constants in terms of Jump time and apex height. */
        gravity = -(2 * jumpHeightMax) / Mathf.Pow(timeToJumpApex, 2);
        jumpVelocityMax = Mathf.Abs(gravity * timeToJumpApex);
        jumpVelocityMin = Mathf.Sqrt(2 * Mathf.Abs(gravity) * jumpHeightMin);

        moveState = MoveState.Falling;
        prevState = moveState;
    }

    /** Update is called once per frame **/
    void Update() {
        CalcState();
        rigidBody.velocity = velocity;
        print(moveState);
    }

    /** Called on Player collision with a new object. **/
    void OnCollisionEnter2D(Collision2D coll) { // ~ Could convert Collision2D to Collider2D
        ContactPoint2D[] contactsIn = new ContactPoint2D[2]; // 2 when side collides (each corner) || 1 when on slope
        coll.GetContacts(contactsIn);

        bool enterSet = false;

        /* Add new contact points to hash. */
        for(int i = 0; i < contactsIn.Length; i++) {
            if(contactsIn[i].normal != Vector2.zero) {
                if (!contacts.Contains(contactsIn[i].normal)) {
                    contacts.Add(contactsIn[i].normal);
                    //print(contactsIn[i].normal);
                }
            }
        }

        /* Call Collider Enter Functions */
        for (int i =0; !enterSet && i < contactsIn.Length; i++) {
            /* If contact exists (entries are zero in larger alocated ContactPoint2D[])*/
            if (contactsIn[i].normal != Vector2.zero) {
                /* Vertical Collision */
                if (contactsIn[i].normal.x == 0) {
                    if (contactsIn[i].normal.y == 1) {
                        onBotCollisionEnter();
                        enterSet = true;
                    }
                    else if (contactsIn[i].normal.y == -1) {
                        onTopCollisionEnter();
                        enterSet = true;
                    }
                }
                /* Horizontal Collision */
                else if (contactsIn[i].normal.y==0) {
                    if (contactsIn[i].normal.x == 1) {
                        onLeftCollisionEnter();
                        enterSet = true;
                    }
                    else if (contactsIn[i].normal.x == -1) {
                        onRightCollisionEnter();
                        enterSet = true;
                    }
                }
                /* Slope Collision */
                else {
                    slopeDir = (contactsIn[i].normal.x < 0) ? 1 : -1; // 1 = right, -1 = left
                    slopeAngle = Vector2.Angle(contactsIn[i].normal, Vector2.up);
                    onSlopeCollisionEnter();
                }
            }
        }
        //print("-------------------");
    }

    /** Called on Player collision Exit. **/
    void OnCollisionExit2D(Collision2D coll) { // ~ Could convert Collision2D to Collider2D
        ContactPoint2D[] contactsRB = new ContactPoint2D[2]; // 2 when side collides (each corner) || 1 when on slope
        rigidBody.GetContacts(contactsRB);

        bool exitSet = false;

        /* Make a hash with the current normals touching the object. */
        HashSet<Vector2> contactNormalsRB = new HashSet<Vector2>();
        foreach (ContactPoint2D c in contactsRB) {
            contactNormalsRB.Add(c.normal);
        }

        HashSet<Vector2> exitContacts = new HashSet<Vector2>();
        exitContacts.UnionWith(contacts);     // exitContacts = contacts

        exitContacts.ExceptWith(contactNormalsRB);  // Set ExitContacts.
        contacts.ExceptWith(exitContacts);   // Remove Exit contacts from Hash.

        /* Call Collider Enter Functions */
        foreach (Vector2 exitContact in exitContacts) {
            if (!exitSet) {
                /* If contact exists (entries are zero in larger alocated ContactPoint2D[])*/
                if (exitContact != Vector2.zero) {
                    /* Vertical Collision */
                    if (exitContact.x == 0) {
                        if (exitContact.y == 1) {
                            onBotCollisionExit();
                            exitSet = true;
                        }
                        else if (exitContact.y == -1) {
                            onTopCollisionExit();
                            exitSet = true;
                        }
                    }
                    /* Horizontal Collision */
                    else if (exitContact.y == 0) {
                        if (exitContact.x == 1) {
                            onLeftCollisionExit();
                            exitSet = true;
                        }
                        else if (exitContact.x == -1) {
                            onRightCollisionExit();
                            exitSet = true;
                        }
                    }
                    /* Slope Collision */
                    else {
                        onSlopeCollisionExit();
                    }
                }
            }
        }
    }

    /** Called on Player collision with object. **/
    void onTopCollisionEnter() {
        velocity.y = 0;
        isTouchingTop = true;
        print("Enter Top--------------");
    }
    void onBotCollisionEnter() {
        velocity.y = 0;
        isGrounded = true;
        isTouchingBot = true;
        onSlope = false;
    }
    void onLeftCollisionEnter() {
        wallImpactSpeed = velocity.x;
        velocity.x = 0;
        isTouchingLeft = true;
        if(!isGrounded)
            onWall = true;
    }
    void onRightCollisionEnter() {
        wallImpactSpeed = velocity.x;
        velocity.x = 0;
        isTouchingRight = true;
        if (!isGrounded)
            onWall = true;
    }
    void onSlopeCollisionEnter() {
        onSlope = true;
        velocity.y = 0; // For falling -> slope transition.
        isGrounded = true; //TODO: make work with upper slopes
    }

    /** Called on Player leaving collision with an object. **/
    void onTopCollisionExit() {
        isTouchingTop = false;
        print("EXIT Top--------------");
    }
    void onBotCollisionExit() {
        print("EXIT Bot--------------");
        if (!onSlope) {
            isGrounded = false;
        }
        isTouchingBot = false;
    }
    void onLeftCollisionExit() {
        print("EXIT Left--------------");
        isTouchingLeft = false;
        onWall = false;
        wallImpactSpeed = activeSpeed;
    }
    void onRightCollisionExit() {
        print("EXIT Right--------------");
        isTouchingRight = false;
        onWall = false;
        wallImpactSpeed = activeSpeed;
    }
    void onSlopeCollisionExit() {
        if (!isTouchingRight && !isTouchingLeft) // Case: when running into obstacles on slope.
            onSlope = false;
    }

    void CalcState() {
        // Do State Actions:
        if (IsIdle())
            doIdle();
        else if (IsJumping())
            doJump();
        else if (IsFalling())
            doFall();
        else if (IsWallRising())
            doWallRise();
        else if (IsWallFalling())
            doWallFall();
        else if (IsWallSticking())
            doWallStick();
        else if (IsDashing())
            doDash();
        else if (IsSprinting())
            doSprint();
        else if(IsClimbingSlope())
            climbSlope();
    }

    void FindState() {
        if (isGrounded) {
            if (onSlope) {
                ChangeState(MoveState.ClimbingSlope);
            }
            else {
                if (velocity.x == 0) {
                    ChangeState(MoveState.Idle);
                }
                else // velocity.x !=0
                {
                    ChangeState(MoveState.Sprinting);
                }
            }
            
        }
        else // !isGrounded
        {
            if (velocity.y > 0) {
                if (onWall) {
                    ChangeState(MoveState.WallRising);
                }
                else
                    ChangeState(MoveState.Jumping);
            }
            else if (velocity.y < 0) {
                if (onWall) {
                    ChangeState(MoveState.WallFalling);
                }
                else
                    ChangeState(MoveState.Falling);
            }
            else if (velocity.y == 0) {
                ChangeState(MoveState.Falling);
            }
        }
    }

    void doIdle() {
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")); // Raw is no smoothing.
        //velocity.y = 0;
        /* Vertical JUMP Calc ------------------------------------------ */
        // When Up is first input.
        if (Input.GetKeyDown(KeyCode.UpArrow)) // Velocity when initial pressed
        {
            velocity.y = jumpVelocityMax;
            isGrounded = false;
        }
        else if (Input.GetKey(KeyCode.UpArrow)) // Up Held down.
        {
            velocity.y = jumpVelocityMax;
            isGrounded = false;
        }
        // When Right is first input.
        /* Lateral Calc -------------------------------------------------- */
        if (Input.GetKeyDown(KeyCode.RightArrow)) {
            directionFacing = 1;
            velocity.x = activeSpeed; // since isGrounded
        }
        // When Left is first input.
        else if (Input.GetKeyDown(KeyCode.LeftArrow)) {
            directionFacing = -1;
            velocity.x = activeSpeed * -1; //  Necessary because input.x changes. - since isGrounded
        }

        if (Input.GetKey(KeyCode.LeftArrow) && !Input.GetKey(KeyCode.RightArrow)) {
            directionFacing = -1;
            velocity.x = activeSpeed * -1;
        }
        else if (Input.GetKey(KeyCode.RightArrow) && !Input.GetKey(KeyCode.LeftArrow)) {
            directionFacing = 1;
            velocity.x = activeSpeed;
        }

        /* Change State -------------------------------------------------- */
        if (!isGrounded || velocity.x != 0) // Conditions to Transition out of state
        {
            FindState();
        }
    }

    void doJump() {
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")); // Raw is no smoothing.

        /* Vertical Calc ----------------------------------------- */
        // When Up is first input.
        if (Input.GetKeyDown(KeyCode.UpArrow)) // Continue adding velocity when pressed
        {
            if (isGrounded) { // normal Jumps
                velocity.y = jumpVelocityMax;
                isGrounded = false;
            }
        }

        // When Up is released in this frame.
        if (Input.GetKeyUp(KeyCode.UpArrow)) {
            if (velocity.y > jumpVelocityMin) { // Keep applying velocity up while key is pressed - variable jump
                velocity.y = jumpVelocityMin;
            }
        }

        velocity.y += gravity * Time.deltaTime; // Apply Gravity until grounded

        /* Lateral Calc -------------------------------------------*/
        // When Right is first input.
        if (Input.GetKeyDown(KeyCode.RightArrow)) {
            directionFacing = 1;
        }
        // When Left is first input.
        else if (Input.GetKeyDown(KeyCode.LeftArrow)) {
            directionFacing = -1;
        }


        if (Input.GetKey(KeyCode.RightArrow) && velocity.x < activeSpeed) { // in-air lateral move right
            velocity.x += lateralAccelAirborne * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.LeftArrow) && velocity.x > -activeSpeed) { // in-air lateral move left
            velocity.x -= lateralAccelAirborne * Time.deltaTime;
        }

        /* Conditions to Transition out of state */
        if (isGrounded || onWall || velocity.y <= 0) {
            FindState();
        }
    }

    void doFall() {
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")); // Raw is no smoothing.

        if (isGrounded || onSlope) {
            FindState();
            return;
        }

        velocity.y += gravity * Time.deltaTime; // Apply Gravity until grounded

        /* Lateral Calc -------------------------------------------*/
        // When Right is first input.
        if (Input.GetKeyDown(KeyCode.RightArrow)) {
            directionFacing = 1;
        }
        // When Left is first input.
        else if (Input.GetKeyDown(KeyCode.LeftArrow)) {
            directionFacing = -1;
        }

        if (Input.GetKey(KeyCode.RightArrow) && velocity.x < activeSpeed) { // in-air lateral move right
            velocity.x += lateralAccelAirborne * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.LeftArrow) && velocity.x > -activeSpeed) { // in-air lateral move left
            velocity.x -= lateralAccelAirborne * Time.deltaTime;
        }

        if (isGrounded || onWall || velocity.y >= 0) // Conditions to Transition out of state
        {
            FindState();
        }

    }

    void doSprint() {
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")); // Raw is no smoothing.
        //velocity.y = 0;

        if (!isGrounded || onSlope) // Conditions to Transition out of state
        {
            FindState();
            return;
        }

        /* Sprint Calc ------------------------------------------------- */
        if (Input.GetKey(KeyCode.LeftShift)) {
            activeSpeed = sprintSpeed;
        }
        else {
            activeSpeed = moveSpeed;
        }

        /* Vertical JUMP Calc ------------------------------------------ */
        // When Up is released in this frame.
        if (Input.GetKeyDown(KeyCode.UpArrow)) {
            velocity.y = jumpVelocityMax;
            isGrounded = false;
        }
        else if (Input.GetKey(KeyCode.UpArrow)) {
            velocity.y = jumpVelocityMax;
            isGrounded = false;
        }

        /* Lateral Calc -------------------------------------------------- */
        // When Right is first input.
        if (Input.GetKeyDown(KeyCode.RightArrow)) {
            directionFacing = 1;
            if (isGrounded)
                velocity.x = activeSpeed; // since isGrounded
        }
        // When Left is first input.
        else if (Input.GetKeyDown(KeyCode.LeftArrow)) {
            directionFacing = -1;
            if (isGrounded)
                velocity.x = activeSpeed * -1; //  Necessary because input.x changes. // since isGrounded
        }
        else if (Input.GetKey(KeyCode.LeftArrow) && !Input.GetKey(KeyCode.RightArrow)) {
            directionFacing = -1;
            if (isGrounded)
                velocity.x = activeSpeed * -1;
        }
        else if (Input.GetKey(KeyCode.RightArrow) && !Input.GetKey(KeyCode.LeftArrow)) {
            directionFacing = 1;
            if (isGrounded)
                velocity.x = activeSpeed;
        }

        /* X Acceleration ---------------------------------------------- */
        // When No input.
        if (isGrounded && !Input.GetKey(KeyCode.RightArrow) && !Input.GetKey(KeyCode.LeftArrow)) { // On-release of Lateral Movement controls - Deccelerate
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

        if (isTouchingLeft || isTouchingRight) {
            velocity.x = 0;
        }

        // Conditions to Transition out of state
        if (!isGrounded || velocity.y != 0 || onSlope) {
            FindState();
        }
    }

    void doWallRise() {
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

        // Conditions to Transition out of state
        if (isGrounded || !onWall || velocity.y <= 0) {
            FindState();
        }
    }

    void doWallFall() {
        velocity.y += gravity * Time.deltaTime; // Apply Gravity until grounded

        // When Up is first input.
        if (Input.GetKeyDown(KeyCode.UpArrow)) {
            if (isTouchingLeft && Input.GetKey(KeyCode.LeftArrow)) // Jump toward wall
            {
                velocity.y = jumpVelocityMax;
                velocity.x = activeSpeed / 2;
            }
            else if (isTouchingRight && Input.GetKey(KeyCode.RightArrow)) // Jump toward wall
            {
                velocity.y = jumpVelocityMax;
                velocity.x = -1 * activeSpeed / 2;
            }
        }

        // When Right is first input again.
        if (Input.GetKeyDown(KeyCode.RightArrow)) { // on L/R input - setting conditions.
            directionFacing = 1;
            if (isTouchingRight && Input.GetKey(KeyCode.UpArrow)) // Jump toward wall
            {
                velocity.y = jumpVelocityMax;
                velocity.x = -1 * activeSpeed / 2;
            }
        }

        // When Left is first input again.
        else if (Input.GetKeyDown(KeyCode.LeftArrow)) {
            directionFacing = -1;
            if (isTouchingLeft && Input.GetKey(KeyCode.UpArrow)) // jump toward wall
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
            else if (isTouchingRight && Input.GetKey(KeyCode.UpArrow)) // Jumping toward right wall.
            {
                // When coming from a non-grounded state, immediately jump when hit wall
                velocity.y = jumpVelocityMax;
                velocity.x = -1 * activeSpeed / 2;
            }

        }
        else if (Input.GetKey(KeyCode.LeftArrow)) {
            if (isTouchingRight) {
                if (Input.GetKey(KeyCode.UpArrow)) // Jump away from right wall.
                {
                    velocity.y = jumpVelocityMax;
                    velocity.x = -1 * activeSpeed;
                }
                else // Fall away from wall
                    velocity.x -= lateralAccelAirborne * Time.deltaTime;
            }
            else if (isTouchingLeft && Input.GetKey(KeyCode.UpArrow)) // Jumping toward left wall.
            {
                // When coming from a non-grounded state, immediately jump when hit wall
                velocity.y = jumpVelocityMax;
                velocity.x = activeSpeed / 2;
            }

        }

        // Conditions to Transition out of state
        if (isGrounded || !onWall || velocity.y >= 0) {
            FindState();
        }
    }

    void doDash() {
    }

    void doWallStick() {
    }

    void climbSlope() {
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")); // Raw is no smoothing.

        /* Change State -------------------------------------------------- */
        if (!onSlope || !isGrounded) // Conditions to Transition out of state
        {
            FindState();
            return;
        }

        /* Vertical JUMP Calc ------------------------------------------ */
        // When Up is first input.
        if (Input.GetKeyDown(KeyCode.UpArrow) && !isTouchingTop) // Velocity when initial pressed
        {
            velocity.y = jumpVelocityMax;
            isGrounded = false;
            onSlope = false;
            if (isTouchingRight || isTouchingRight) // Case: Jumping from slope against a wall. 
                onWall = true;
            FindState();
            return;
        }
        
        else if (Input.GetKey(KeyCode.UpArrow) && !isTouchingTop) // Up Held down.
        {
            velocity.y = jumpVelocityMax;
            isGrounded = false;
            onSlope = false;
            if (isTouchingRight || isTouchingRight) // Case: Jumping from slope against a wall. 
                onWall = true;
            FindState();
            return;
        }

        /* Lateral Calc -------------------------------------------------- */
        // When Right is first input.
        if (Input.GetKeyDown(KeyCode.RightArrow)) {
            directionFacing = 1;
            if (isGrounded) {
                if(!isTouchingRight) {
                    velocity.x = activeSpeed * Mathf.Cos(slopeAngle * Mathf.Deg2Rad); // since isGrounded
                    velocity.y = activeSpeed * Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * slopeDir;
                }
                else { // Wall on slope.
                    velocity.x = 0;
                    velocity.y = 0;
                }
            }
            
                
        }
        // When Left is first input.
        else if (Input.GetKeyDown(KeyCode.LeftArrow)) {
            directionFacing = -1;
            if (isGrounded) {
                if (!isTouchingLeft) {
                    velocity.x = activeSpeed * Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * -1;
                    velocity.y = activeSpeed * Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * slopeDir * -1;
                }
                else { // Case: Wall on slope.
                    velocity.x = 0;
                    velocity.y = 0;
                }
            }
                
        }
        else if (Input.GetKey(KeyCode.LeftArrow) && !Input.GetKey(KeyCode.RightArrow)) {
            directionFacing = -1;
            if (isGrounded) {
                if (!isTouchingLeft) {
                    velocity.x = activeSpeed * Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * -1;
                    velocity.y = activeSpeed * Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * slopeDir * -1;
                }
                else { // Case: Wall on slope.
                    velocity.x = 0;
                    velocity.y = 0;
                }
            }
                
        }
        else if (Input.GetKey(KeyCode.RightArrow) && !Input.GetKey(KeyCode.LeftArrow)) {
            directionFacing = 1;
            if (isGrounded) {
                if (!isTouchingRight) {
                    velocity.x = activeSpeed * Mathf.Cos(slopeAngle * Mathf.Deg2Rad);
                    velocity.y = activeSpeed * Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * slopeDir;
                }
                else { // Wall on slope.
                    velocity.x = 0;
                    velocity.y = 0;
                }
            }
                
        }

        /* X Acceleration ---------------------------------------------- */
        // When No input.
        if (isGrounded && !Input.GetKey(KeyCode.RightArrow) && !Input.GetKey(KeyCode.LeftArrow)) { // On-release of Lateral Movement controls - Deccelerate
            //velocity.x = 0;
            //velocity.y = 0;
            if (directionFacing == 1 && velocity.x < 0 || directionFacing == -1 && velocity.x > 0) { // Stops deccel when hits 0 from the initial negative(left moving) or pos(right moving) val
                velocity.x = 0;
                velocity.y = 0;
            }
            if (directionFacing == 1 && velocity.x > 0) { // Decceleration Right
                velocity.x -= lateralAccelGrounded * Time.deltaTime;
                velocity.y -= lateralAccelGrounded * Time.deltaTime;
            }
            else if (directionFacing == -1 && velocity.x < 0) { // Decceleration Left
                velocity.x += lateralAccelGrounded * Time.deltaTime;
                velocity.y -= lateralAccelGrounded * Time.deltaTime;
            }
        }

        /* Change State -------------------------------------------------- */
        if (!onSlope || !isGrounded) // Conditions to Transition out of state
        {
            FindState();
        }
    }
    /* Example State
     * 
        void ExState()
        {
            if(conditionToTransition)
                FindState();
        }    
     */

    private void ChangeState(MoveState newState) {
        // no change...
        if (moveState == newState) {
            return;
        }

        // set
        prevState = moveState;
        moveState = newState;
    }
}
