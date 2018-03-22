using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PBaseMovement : PBehaviour {

    /* Inherited Variables: */
    // Inherited: PInputManager pInputManager
    // Inherited: PState curState
    // Inherited: PCollisionState collisionState

    /* Declare Components: */
    public Rigidbody2D rigidBody;
    public new Collider2D collider; // @new for gameobject.collider keyword hiding.

    /* Base Movement Variables: */
    public float gravity;
    sbyte directionFacing = 1; // @sybte of size -128 to 127
    sbyte directionMoving = 1;   
    
    /* Airborne Variables: */
    float jumpVelocityMax;
    float jumpVelocityMin;

    /* Declare States: */
    public PBaseMovement_Airborne SAirborne; // -- TODO: 3.14.18 Tried polymorphism with PStates, but presented issues.
    public PBaseMovement_Idle SIdle;

    /* Collision Variables: */
    public HashSet<CollisionType> enterCollisionTypes = new HashSet<CollisionType>(); // Used to simulate onCollisionEnter each FixedUpdate.

    private void Awake()
    {
        /* Get Components. */
        rigidBody = GetComponent<Rigidbody2D>(); // Note: Could be in this.Start Method
        collider = GetComponent<Collider2D>();   // For CameraFollow's Start Method.
    }

    public override void Start()
    {
        /* Input Setup. */
        inputFilter = new List<PInput>() { PInput.Vertical, PInput.Horizontal, PInput.Up, PInput.Down, PInput.Left, PInput.Right, PInput.Sprint, PInput.Dash };
        base.Start(); // Creates Input Manager.

        /* Calc Movement Variables. */
        gravity = -(2 * PStats.jumpHeightMax) / Mathf.Pow(PStats.timeToJumpApex, 2);
        jumpVelocityMax = Mathf.Abs(gravity * PStats.timeToJumpApex);
        jumpVelocityMin = Mathf.Sqrt(2 * Mathf.Abs(gravity) * PStats.jumpHeightMin);

        /* Create States. */
        SAirborne = gameObject.AddComponent(typeof(PBaseMovement_Airborne)) as PBaseMovement_Airborne;
        SIdle = gameObject.AddComponent(typeof(PBaseMovement_Idle)) as PBaseMovement_Idle;
        SetStateParentBehaviours();

        /* Set State. */
        curState = SAirborne;
    }

    public override void OnFixedUpdate()
    {
        /* Pre-State Update. */
        directionMoving = (((PBaseMovement_State)curState).velocity.x >= 0) ? (sbyte)1 : (sbyte)-1; // @sbyte an explicit cast. 
        // TODO: update DirectionFacing

        /* Collision Update. */
        collisionState.OnFixedUpdate();

        /* State Update. */
        base.OnFixedUpdate();     // Via PBehaviour: Runs OnFixedUpdate for the current State.
        // Note: Transition would occur here.
            // curState.Exit()
            // nextState.Enter()
        rigidBody.velocity = ((PBaseMovement_State)curState).velocity; // Gets the velocity from the current PBaseMovement_State.
    }

    public override void OnTransition(PState nextState)
    {
        base.OnTransition(nextState); // Calls exit and enter methods for prev and next state respectively.
    }

    /* ---- Methods for Readability (Called once, solely to slim down overriden methods above.) */

    /* Set the behaviour var in each state for referencing this Behaviour. */
    private void SetStateParentBehaviours()
    {
        SAirborne.behaviour = this;
        SIdle.behaviour = this; 
    }
}