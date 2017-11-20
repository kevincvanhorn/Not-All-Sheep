using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * General Character Platformer Controller - Parent of DeathForm, LifeForm, and ReaperForm MoveDrivers
 * (DFMoveDriver, LFMoveDriver, RFMoveDriver)
 */

[RequireComponent(typeof(Rigidbody2D))]
public class CharacterMoveDriver : MonoBehaviour {

    public Rigidbody2D rigidBody; // Not Kinematic: moves not by transform, but by physics
    public CollisionInfo collisions;

    /* Collisions Vars */
    public bool isGrounded; // Essentially touchingBot
    public bool isRightPressed; // is a force right applied
    public bool isLeftPressed; // is a force left applied
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
    public float sprintSpeed = 20;
    public float activeSpeed;
    public float wallImpactSpeed;
    public Vector3 velocity;
    float directionFacing = 1;

    /* Jump Variables */
    public float lateralAccelAirborne = 60;
    public float lateralAccelGrounded = 60;

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
        isRightPressed = false;
        isLeftPressed = false;
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
    }

    /** Update is called once per frame **/
    void Update()
    {
        CalcState();
        rigidBody.velocity = velocity;
        print(moveState);
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
        wallImpactSpeed = moveSpeed;
    }
    void onRightCollisionExit()
    {
        isTouchingRight = false;
        onWall = false;
        wallImpactSpeed = moveSpeed;
    }

    void CalcState()
    {
        // Update internal vars:
        if (Input.GetKeyUp(KeyCode.RightArrow))
        {
            isRightPressed = false;
        }
        if (Input.GetKeyUp(KeyCode.LeftArrow))
        {
            isLeftPressed = false;
        }

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
        if (Input.GetKeyDown(KeyCode.UpArrow)) // Velocity when initial pressed
        {
            velocity.y = jumpVelocityMax;
            isGrounded = false;
        }

        /* Lateral Calc -------------------------------------------------- */
        if (Input.GetKeyDown(KeyCode.RightArrow) && !isRightPressed)
        {
            isRightPressed = true;
            isLeftPressed = false;
            directionFacing = 1;
            velocity.x = activeSpeed; // since isGrounded
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow) && !isLeftPressed)
        {
            isLeftPressed = true;
            isRightPressed = false;
            directionFacing = -1;
            velocity.x = activeSpeed * -1; //  Necessary because input.x changes. - since isGrounded
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
        if (Input.GetKeyDown(KeyCode.UpArrow)) // Continue adding velocity when pressed
        { 
            if (isGrounded)
            { // normal Jumps
                velocity.y = jumpVelocityMax;
                isGrounded = false;
            }
        }

        if (Input.GetKeyUp(KeyCode.UpArrow))
        {
            if (velocity.y > jumpVelocityMin)
            { // Keep applying velocity up while key is pressed - variable jump
                velocity.y = jumpVelocityMin;
            }
        }

        velocity.y += gravity * Time.deltaTime; // Apply Gravity until grounded

        /* Lateral Calc -------------------------------------------*/
        if (Input.GetKeyDown(KeyCode.RightArrow) && !isRightPressed)
        {
            isRightPressed = true;
            isLeftPressed = false;
            directionFacing = 1;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow) && !isLeftPressed)
        {
            isLeftPressed = true;
            isRightPressed = false;
            directionFacing = -1;
        }


        if (isRightPressed && velocity.x < activeSpeed)
        { // in-air lateral move right
            velocity.x += lateralAccelAirborne * Time.deltaTime;
        }
        else if (isLeftPressed && velocity.x > -activeSpeed)
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
        if (Input.GetKeyDown(KeyCode.RightArrow) && !isRightPressed)
        {
            isRightPressed = true;
            isLeftPressed = false;
            directionFacing = 1;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow) && !isLeftPressed)
        {
            isLeftPressed = true;
            isRightPressed = false;
            directionFacing = -1;
        }

        if (isRightPressed && velocity.x < activeSpeed)
        { // in-air lateral move right
            velocity.x += lateralAccelAirborne * Time.deltaTime;
        }
        else if (isLeftPressed && velocity.x > -activeSpeed)
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

        /* Vertical JUMP Calc ------------------------------------------ */
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
                velocity.y = jumpVelocityMax;
                isGrounded = false;
        }

        /* Lateral Calc -------------------------------------------------- */
        if (Input.GetKeyDown(KeyCode.RightArrow) && !isRightPressed)
        {
            isRightPressed = true;
            isLeftPressed = false;
            directionFacing = 1;
            velocity.x = activeSpeed; // since isGrounded
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow) && !isLeftPressed)
        {
            isLeftPressed = true;
            isRightPressed = false;
            directionFacing = -1;
            velocity.x = activeSpeed * -1; //  Necessary because input.x changes. // since isGrounded
        }
        /* X Acceleration ---------------------------------------------- */
        if (isGrounded && (isRightPressed || isLeftPressed))
        {
            velocity.x = activeSpeed * directionFacing; // Ground Sliding
        }
        else if (isGrounded && !isRightPressed && !isLeftPressed)
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

        // Conditions to Transition out of state
        if (!isGrounded || velocity.y != 0) 
        {
            FindState();
        }
    }

    void doWallRise()
    {
        velocity.y += gravity * Time.deltaTime; // Apply Gravity until grounded

        // Conditions to Transition out of state
        if (!onWall)
        {
            FindState();
        }
    }

    void doWallFall()
    {
        velocity.y += gravity * Time.deltaTime; // Apply Gravity until grounded

        // Conditions to Transition out of state
        if (!onWall)
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

    private void ChangeState(MoveState newState)
    {
        // no change...
        if (moveState == newState)
        {
            return;
        }

        // set
        moveState = newState;
    }
}