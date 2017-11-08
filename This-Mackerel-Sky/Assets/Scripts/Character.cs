using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D))]

// For wall collide store collision velocity, stop, then add that velocity as modifier to wall jump

public class Character : MonoBehaviour {
    public LayerMask collisionMask;
    public Rigidbody2D rigidBody; // Not Kinematic: moves not by transform, but by physics
    public CollisionInfo collisions;

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

    /** Called on Frame initialization. **/
    void Start() {
        /* Set child colliders. */
        foreach (Transform child in transform) {
            if (child.tag == "PlayerCollider") {
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
        collisions.isTouchingTop = false;
        collisions.isTouchingRight = false;
        collisions.isTouchingLeft = false;
        collisions.isRightPressed = false;
        collisions.isLeftPressed = false;
        collisions.isGrounded = false;
        collisions.isSprinting = false;
        
        activeSpeed = moveSpeed;
        rigidBody = GetComponent<Rigidbody2D>();

        /* Calc constants in terms of Jump time and apex height. */
        gravity = -(2 * jumpHeightMax) / Mathf.Pow(timeToJumpApex, 2);
        jumpVelocityMax = Mathf.Abs(gravity * timeToJumpApex);
        jumpVelocityMin = Mathf.Sqrt(2 * Mathf.Abs(gravity) * jumpHeightMin);
    }

    /** Update is called once per frame **/
    void Update() {
        calcJump();
    }

    /** Called on Player collision with object. **/
    void onTopCollisionEnter() {
        velocity.y = 0;
        collisions.isTouchingTop = true;
    }
    void onBotCollisionEnter() {
        velocity.y = 0;
        collisions.isGrounded = true;
    }
    void onLeftCollisionEnter() {
        velocity.x = 0;
        collisions.isTouchingLeft = true;
        collisions.onWall = true;
    }
    void onRightCollisionEnter() {
        velocity.x = 0;
        collisions.isTouchingRight = true;
        collisions.onWall = true;
    }

    /** Called on Player leaving collision with an object. **/
    void onTopCollisionExit() {
        collisions.isTouchingTop = false;
    }
    void onBotCollisionExit() {
        collisions.isGrounded = false;
    }
    void onLeftCollisionExit() {
        collisions.isTouchingLeft = false;
        collisions.onWall = false;
    }
    void onRightCollisionExit() {
        collisions.isTouchingRight = false;
        collisions.onWall = false;
    }

    /** Calculates movement and updates rigidbody velocity. **/
    void calcJump() {
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")); // Raw is no smoothing.

        /* Vertical JUMP Calc ------------------------------------------ */
        if (Input.GetKeyDown(KeyCode.UpArrow)) {// replace with more general
            if(collisions.isGrounded){ // normal Jumps
                velocity.y = jumpVelocityMax;
                collisions.isGrounded = false;
            }
            else if(collisions.onWall){ // Wall Jumps // Note: to fix the sticking to wall part: check above equations with gravity - may need to use a different equation for gravity.
                if (collisions.isTouchingLeft){
                    velocity.y = jumpVelocityMax;
                    velocity.x = moveSpeed;
                }
                else if(collisions.isTouchingRight){
                    velocity.x = moveSpeed * -1;
                    velocity.y = jumpVelocityMax;
                }
            }
        }

        if (Input.GetKeyUp(KeyCode.UpArrow)) {
            if (velocity.y > jumpVelocityMin) { // Keep applying velocity up while key is pressed - variable jump
                velocity.y = jumpVelocityMin;
            }
        }

        if (!collisions.isGrounded) { // Apply Gravity every frame until grounded
            velocity.y += gravity * Time.deltaTime;
        }

        /* Lateral Calc -------------------------------------------------- */
        if (Input.GetKeyDown(KeyCode.LeftShift)) { // Sprint - Start.
            collisions.isSprinting = true;
            if (collisions.isGrounded) {
                activeSpeed = sprintSpeed;
                velocity.x = activeSpeed * input.x;
            }
        }

        if (Input.GetKeyUp(KeyCode.LeftShift)) { // Sprint - Stop.
            collisions.isSprinting = false;
            activeSpeed = moveSpeed;
        }

        if (Input.GetKeyDown(KeyCode.RightArrow) && !collisions.isRightPressed) { // on L/R input - setting conditions.
            collisions.isRightPressed = true;
            collisions.isLeftPressed = false;
            directionFacing = 1;
            if (collisions.isGrounded) {
                velocity.x = activeSpeed;
            }
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow) && !collisions.isLeftPressed) {
            collisions.isLeftPressed = true;
            collisions.isRightPressed = false;
            directionFacing = -1;
            if (collisions.isGrounded) {
                velocity.x = activeSpeed * -1; //  Necessary because input.x changes.
            }

        }
        if (Input.GetKeyUp(KeyCode.RightArrow)) {
            collisions.isRightPressed = false;
        }
        else if (Input.GetKeyUp(KeyCode.LeftArrow)) {
            collisions.isLeftPressed = false;
        }
        // X Acceleration
        if (collisions.isGrounded && (collisions.isRightPressed || collisions.isLeftPressed)) {
            velocity.x = activeSpeed * directionFacing; // Ground Sliding
        }
        else if (collisions.isGrounded && !collisions.isRightPressed && !collisions.isLeftPressed) { // On-release of Lateral Movement controls - Deccelerate
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
        else if (!collisions.isGrounded) { // Air Lateral Movement
            if (!collisions.onWall && (collisions.isTouchingLeft || collisions.isTouchingRight)) { // in-air, hitting a wall laterally
                velocity.x = 0;
            }
            else if (collisions.isRightPressed && velocity.x < activeSpeed) { // in-air lateral move right
                velocity.x += lateralAccelAirborne * Time.deltaTime;
            }
            else if (collisions.isLeftPressed && velocity.x > -activeSpeed) { // in-air lateral move left
                velocity.x -= lateralAccelAirborne * Time.deltaTime;
            }
        }

        rigidBody.velocity = velocity;
    }
}

public struct CollisionInfo {
    public bool isGrounded; // Essentially touchingBot
    public bool isSprinting;
    public bool isRightPressed; // is a force right applied
    public bool isLeftPressed; // is a force left applied
    //note: 3 states- left, right, and still require two variables

    public bool isTouchingTop;
    public bool isTouchingRight;
    public bool isTouchingLeft;

    public bool onWall;
}