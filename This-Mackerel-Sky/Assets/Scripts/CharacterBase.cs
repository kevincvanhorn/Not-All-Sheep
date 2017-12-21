using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MonsterLove.StateMachine; // State-Machine Package.

[RequireComponent(typeof(Rigidbody2D))]
public class CharacterBase : MonoBehaviour {

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
    public enum States {
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

    private StateMachine<States> fsm;

    public void Awake() {
        // Initialize State Machine Engine		
        fsm = StateMachine<States>.Initialize(this, States.Falling);
    }

    public void Start() {
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
    }

    // Update is called once per frame
    void Update() {
        CalcState();
        rigidBody.velocity = velocity;
    }

    void CalcState() {
        var state = fsm.State;
        if(state == States.Idle) {

        }
    }

    void Falling_OnCollisionEnter2D(Collision2D collision) {
        bool enterSet = false;

        Debug.Log("FALLING - OnCollisionEnter");
        velocity.y = 0;
        fsm.ChangeState(States.Idle, StateTransition.Overwrite);
    }

    void Falling_Update() {
        velocity.y += gravity * Time.deltaTime; // Apply Gravity until grounded
    }

    void Idle_Enter() {
        velocity.y = 0;
    }

    /* ------------------------------------------------------------------------------ Collision Methods: Custom  */

    /** Called on Player collision with a new object. **/
    void BaseCollisionEnter2D(Collision2D coll) { // ~ Could convert Collision2D to Collider2D
        ContactPoint2D[] contactsIn = new ContactPoint2D[4]; // 2 when side collides (each corner) || 1 when on slope
        coll.GetContacts(contactsIn);

        /* Add new contact points to hash. */
        for (int i = 0; i < contactsIn.Length; i++) {
            if (contactsIn[i].normal != Vector2.zero) {
                if (!contacts.Contains(contactsIn[i].normal)) {
                    contacts.Add(contactsIn[i].normal);
                    //print(contactsIn[i].normal);
                }
            }
        }

    }

    void BaseCollisionExit2D(ref Collision2D collision) {

    }
}

// Fianlly: Reset object to desired configuration
// For Overwrite: fsm.ChangeState(States.MyNextState, StateTransition.Overwrite);