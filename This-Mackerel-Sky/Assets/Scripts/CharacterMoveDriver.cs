using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * General Character Platformer Controller - Parent of DeathForm, LifeForm, and ReaperForm MoveDrivers
 * (DFMoveDriver, LFMoveDriver, RFMoveDriver)
 */

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CharacterRayController))]
public class CharacterMoveDriver : CharacterRayController {

    public Rigidbody2D rigidBody; // Not Kinematic: moves not by transform, but by physics

    /* Collisions Vars */
    public bool isGrounded; // Essentially touchingBot
    //note: 3 states- left, right, and still require two variables
    public bool isSprinting;
    public bool isTouchingTop;
    public bool isTouchingRight;
    public bool isTouchingLeft;
    public bool onWall;

    /* Colliders */
    public List<GameObject> ChildrenColliders;
    public PlayerCollider topCollider;
    public PlayerCollider botCollider;
    public PlayerCollider leftCollider;
    public PlayerCollider rightCollider;

    /* Movement Variables */
    public float moveSpeed = 10;    // Horizontal speed.
    public float moveSpeedMin = 5;
    public float sprintSpeed = 20;
    public float activeSpeed;
    public float wallImpactSpeed;
    public Vector3 velocity;
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

    /* Define States */
    public enum MoveState
    {
        Idle,
        Jumping,
        Falling,
        WallRising,
        WallFalling,
        WallSticking,
        Sprinting,
        Dashing
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

    void Start()
    {
        /* Set child colliders. */
        foreach (Transform child in transform)
        {
            if (child.tag == "PlayerCollider")
            {
                ChildrenColliders.Add(child.gameObject);
            }
        }

        topCollider = ChildrenColliders[0].GetComponent<PlayerCollider>();
        botCollider = ChildrenColliders[1].GetComponent<PlayerCollider>();
        leftCollider = ChildrenColliders[2].GetComponent<PlayerCollider>();
        rightCollider = ChildrenColliders[3].GetComponent<PlayerCollider>();

        topCollider.OnEdgeEnter += onTopCollisionEnter;
        botCollider.OnEdgeEnter += onBotCollisionEnter;
        leftCollider.OnEdgeEnter += onLeftCollisionEnter;
        rightCollider.OnEdgeEnter += onRightCollisionEnter;

        topCollider.OnEdgeExit += onTopCollisionExit;
        botCollider.OnEdgeExit += onBotCollisionExit;
        leftCollider.OnEdgeExit += onLeftCollisionExit;
        rightCollider.OnEdgeExit += onRightCollisionExit;


        /* Set collision defaults. */
        isTouchingTop = false;
        isTouchingRight = false;
        isTouchingLeft = false;
        isGrounded = false;
        isSprinting = false;

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
    void Update()
    {
        CalcState();
        rigidBody.velocity = velocity;
        print(moveState);
        //print(velocity + " rp: " + isRightPressed + " tL: " + isTouchingLeft);
    }

    /** Called on Player collision with object. **/
    void onTopCollisionEnter()
    {
        velocity.y = 0;
        isTouchingTop = true;
    }
    void onBotCollisionEnter()
    {
        velocity.y = 0;
        isGrounded = true;
    }
    void onLeftCollisionEnter()
    {
        wallImpactSpeed = velocity.x;
        velocity.x = 0;
        isTouchingLeft = true;
        onWall = true;
    }
    void onRightCollisionEnter()
    {
        wallImpactSpeed = velocity.x;
        velocity.x = 0;
        isTouchingRight = true;
        onWall = true;
    }

    /** Called on Player leaving collision with an object. **/
    void onTopCollisionExit()
    {
        isTouchingTop = false;
    }
    void onBotCollisionExit()
    {
        isGrounded = false;
    }
    void onLeftCollisionExit()
    {
        isTouchingLeft = false;
        onWall = false;
        wallImpactSpeed = activeSpeed;
    }
    void onRightCollisionExit()
    {
        isTouchingRight = false;
        onWall = false;
        wallImpactSpeed = activeSpeed;
    }

    void CalcState()
    {
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
    }

    void FindState()
    {
        if (isGrounded)
        {
            if(velocity.x == 0)
            {
                ChangeState(MoveState.Idle);
            }
            else // velocity.x !=0
            {
                ChangeState(MoveState.Sprinting);
            }
        }
        else // !isGrounded
        {
            if(velocity.y > 0)
            {
                if (onWall)
                {
                    ChangeState(MoveState.WallRising);
                }
                else
                    ChangeState(MoveState.Jumping);
            }
            else if(velocity.y < 0)
            {
                if (onWall)
                {
                    ChangeState(MoveState.WallFalling);
                }
                else
                    ChangeState(MoveState.Falling);
            }
            else if(velocity.y == 0)
            {

            }
        }
    }

    void doIdle()
    {
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")); // Raw is no smoothing.

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
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            directionFacing = 1;
            velocity.x = activeSpeed; // since isGrounded
        }
        // When Left is first input.
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            directionFacing = -1;
            velocity.x = activeSpeed * -1; //  Necessary because input.x changes. - since isGrounded
        }

        if(Input.GetKey(KeyCode.LeftArrow) && !Input.GetKey(KeyCode.RightArrow))
        {
            directionFacing = -1;
            velocity.x = activeSpeed * -1;
        }
        else if (Input.GetKey(KeyCode.RightArrow) && !Input.GetKey(KeyCode.LeftArrow))
        {
            directionFacing = 1;
            velocity.x = activeSpeed;
        }

        /* Change State -------------------------------------------------- */
        if (!isGrounded || velocity.x != 0) // Conditions to Transition out of state
        {
            FindState();
        }
    }


    void doJump()
    {
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")); // Raw is no smoothing.

        /* Vertical Calc ----------------------------------------- */
        // When Up is first input.
        if (Input.GetKeyDown(KeyCode.UpArrow)) // Continue adding velocity when pressed
        {
            if (isGrounded)
            { // normal Jumps
                velocity.y = jumpVelocityMax;
                isGrounded = false;
            }
        }

        // When Up is released in this frame.
        if (Input.GetKeyUp(KeyCode.UpArrow))
        {
            if (velocity.y > jumpVelocityMin)
            { // Keep applying velocity up while key is pressed - variable jump
                velocity.y = jumpVelocityMin;
            }
        }

        velocity.y += gravity * Time.deltaTime; // Apply Gravity until grounded

        /* Lateral Calc -------------------------------------------*/
        // When Right is first input.
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            directionFacing = 1;
        }
        // When Left is first input.
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            directionFacing = -1;
        }


        if (Input.GetKey(KeyCode.RightArrow) && velocity.x < activeSpeed)
        { // in-air lateral move right
            velocity.x += lateralAccelAirborne * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.LeftArrow) && velocity.x > -activeSpeed)
        { // in-air lateral move left
            velocity.x -= lateralAccelAirborne * Time.deltaTime;
        }

        /* Conditions to Transition out of state */
        if (isGrounded || onWall || velocity.y <= 0) 
        {
            FindState();
        }
    }

    void doFall()
    {
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")); // Raw is no smoothing.

        velocity.y += gravity * Time.deltaTime; // Apply Gravity until grounded

        /* Lateral Calc -------------------------------------------*/
        // When Right is first input.
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            directionFacing = 1;
        }
        // When Left is first input.
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            directionFacing = -1;
        }

        if (Input.GetKey(KeyCode.RightArrow) && velocity.x < activeSpeed)
        { // in-air lateral move right
            velocity.x += lateralAccelAirborne * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.LeftArrow) && velocity.x > -activeSpeed)
        { // in-air lateral move left
            velocity.x -= lateralAccelAirborne * Time.deltaTime;
        }

        if (isGrounded || onWall || velocity.y >= 0) // Conditions to Transition out of state
        {
            FindState();
        }

    }

    void doSprint()
    {
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")); // Raw is no smoothing.

        if (!isGrounded) // Conditions to Transition out of state
        {
            FindState();
        }

        /* Sprint Calc ------------------------------------------------- */
        if (Input.GetKey(KeyCode.LeftShift)) {
            activeSpeed = sprintSpeed;
            print("asdklfjalskdjfhakljsdf");
        }
        else {
            activeSpeed = moveSpeed;
        }

        /* Vertical JUMP Calc ------------------------------------------ */
        // When Up is released in this frame.
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            velocity.y = jumpVelocityMax;
            isGrounded = false;
        }
        else if (Input.GetKey(KeyCode.UpArrow))
        {
            velocity.y = jumpVelocityMax;
            isGrounded = false;
        }

        /* Lateral Calc -------------------------------------------------- */
        // When Right is first input.
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            directionFacing = 1;
            if(isGrounded)
                velocity.x = activeSpeed; // since isGrounded
        }
        // When Left is first input.
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            directionFacing = -1;
            if(isGrounded)
                velocity.x = activeSpeed * -1; //  Necessary because input.x changes. // since isGrounded
        }
        else if (Input.GetKey(KeyCode.LeftArrow) && !Input.GetKey(KeyCode.RightArrow))
        {
            directionFacing = -1;
            if(isGrounded)
                velocity.x = activeSpeed * -1;
        }
        else if (Input.GetKey(KeyCode.RightArrow) && !Input.GetKey(KeyCode.LeftArrow))
        {
            directionFacing = 1;
            if(isGrounded)
                velocity.x = activeSpeed;
        }

        /* X Acceleration ---------------------------------------------- */
        // When No input.
        if (isGrounded && !Input.GetKey(KeyCode.RightArrow) && !Input.GetKey(KeyCode.LeftArrow))
        { // On-release of Lateral Movement controls - Deccelerate
            if (directionFacing == 1 && velocity.x < 0 || directionFacing == -1 && velocity.x > 0)
            { // Stops deccel when hits 0 from the initial negative(left moving) or pos(right moving) val
                velocity.x = 0;
            }
            if (directionFacing == 1 && velocity.x > 0)
            { // Decceleration Right
                velocity.x -= lateralAccelGrounded * Time.deltaTime;
            }
            else if (directionFacing == -1 && velocity.x < 0)
            { // Decceleration Left
                velocity.x += lateralAccelGrounded * Time.deltaTime;
            }
        }

        if(isTouchingLeft || isTouchingRight)
        {
            velocity.x = 0;
        }

        // Conditions to Transition out of state
        if (!isGrounded || velocity.y != 0) 
        {
            FindState();
        }
    }

    void doWallRise()
    {
        velocity.y += gravity * Time.deltaTime; // Apply Gravity until grounded

        // When Up is first input.
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if(isTouchingLeft && Input.GetKey(KeyCode.LeftArrow)) // Jump toward left wall.
            {
                velocity.y = jumpVelocityMax;
                velocity.x = activeSpeed / 2;
            }
            else if(isTouchingRight && Input.GetKey(KeyCode.RightArrow)) // Jump toward right wall.
            {
                velocity.y = jumpVelocityMax;
                velocity.x = -1 * activeSpeed / 2;
            }
        }
        // When Up is released in this frame.
        if (Input.GetKeyUp(KeyCode.UpArrow))
        {
            if (velocity.y > jumpVelocityMin)
            { // Keep applying velocity up while key is pressed - variable jump
                velocity.y = jumpVelocityMin;
            }
        }
        // When Right is first input.
        if (Input.GetKeyDown(KeyCode.RightArrow))
        { // on L/R input - setting conditions.
            directionFacing = 1;
            if (isTouchingRight && Input.GetKey(KeyCode.UpArrow)) // Jumping toward right wall.
            {
                velocity.y = jumpVelocityMax;
                velocity.x = -1 * activeSpeed / 2;
            }
        }
        // When Left is first input.
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            directionFacing = -1;
            if (isTouchingLeft && Input.GetKey(KeyCode.UpArrow)) // Jumping toward left wall.
            {
                velocity.y = jumpVelocityMax;
                velocity.x = activeSpeed / 2;
            }
        }

        // When Right or Left is held down.
        if (Input.GetKey(KeyCode.RightArrow))
        {
            if (isTouchingLeft)
            {
                if (Input.GetKey(KeyCode.UpArrow)) // Jump away from left wall.
                {
                    velocity.y = jumpVelocityMax;
                    velocity.x = activeSpeed;
                }
                else // Fall away from wall
                    velocity.x += lateralAccelAirborne * Time.deltaTime;
            }
            
        }
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            if (isTouchingRight)
            {
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
        if (isGrounded || !onWall || velocity.y <= 0)
        {
            FindState();
        }
    }

    void doWallFall()
    {
        velocity.y += gravity * Time.deltaTime; // Apply Gravity until grounded

        // When Up is first input.
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
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
        if (Input.GetKeyDown(KeyCode.RightArrow))
        { // on L/R input - setting conditions.
            directionFacing = 1;
            if (isTouchingRight && Input.GetKey(KeyCode.UpArrow)) // Jump toward wall
            {
                velocity.y = jumpVelocityMax;
                velocity.x = -1 * activeSpeed / 2;
            }
        }

        // When Left is first input again.
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            directionFacing = -1;
            if (isTouchingLeft && Input.GetKey(KeyCode.UpArrow)) // jump toward wall
            {
                velocity.y = jumpVelocityMax;
                velocity.x = activeSpeed / 2;
            }
        }

        // When Right or Left is held down.
        if (Input.GetKey(KeyCode.RightArrow))
        {
            if (isTouchingLeft)
            {
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
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            if (isTouchingRight)
            {
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
        if (isGrounded || !onWall || velocity.y >= 0)
        {
            FindState();
        }
    }

    void doDash()
    {
    }

    void doWallStick()
    {
    }
    /* Example State
     * 
        void ExState()
        {
            if(conditionToTransition)
                FindState();
        }    
     */

    private void ChangeState(MoveState newState)
    {
        // no change...
        if (moveState == newState)
        {
            return;
        }

        // set
        prevState = moveState;
        moveState = newState;
    }
}