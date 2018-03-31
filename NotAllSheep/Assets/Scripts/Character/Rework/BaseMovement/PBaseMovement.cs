using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(PBaseMovement_Airborne))]
[RequireComponent(typeof(PBaseMovement_Idle))]
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
    public sbyte directionFacing = 1; // @sybte of size -128 to 127
    public sbyte directionMoving = 1; // @sybte of size -128 to 127 
    public float moveSpeed = 10;
    public float activeSpeed;         // Horizontal Speed.
    public float lateralAccelAirborne = 60;
    public float lateralAccelGrounded = 100;

    /* Airborne Variables: */
    [HideInInspector]
    public float jumpVelocityMax;
    [HideInInspector]
    public float jumpVelocityMin;
    public Vector2 velocity = Vector2.zero;

    /* Wall Variables: */
    public Vector2 wallHitSpeed;

    /* Top Slope Vars: */
    public Vector2 topSlopeSpeedCur;

    /* Declare States: */
    public PBaseMovement_Airborne SAirborne; // -- TODO: 3.14.18 Tried polymorphism with PStates, but presented issues.
    public PBaseMovement_Idle SIdle;
    public PBaseMovement_Running SRunning;
    public PBaseMovement_State SOnWall, SSteepSlope, STopSlope, SClimbingSlope, SDashing, SAction;

    /* Collision Variables: */
    public HashSet<CollisionType> enterCollisionTypes = new HashSet<CollisionType>(); // Used to simulate onCollisionEnter each FixedUpdate.

    /* Camera Variables: */
    public bool hasLateralInput;

    public override void Awake()
    {
        base.Awake();
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
        activeSpeed = moveSpeed;

        /* Create States. */
        SAirborne = gameObject.GetComponent<PBaseMovement_Airborne>();
        SIdle = gameObject.GetComponent<PBaseMovement_Idle>();
        SRunning = gameObject.GetComponent<PBaseMovement_Running>();

        SetStateParentBehaviours();

        /* Set State. */
        curState = SAirborne;
    }

    public override void OnFixedUpdate()
    {
        /* Receive input from any pInutManager Updates. */

        /* Pre-State Update. */ 
        UpdateLateralInputVars(); // Sets directionMoving, directionFacing, hasLateralInput.

        /* Collision Update. */
        collisionState.OnFixedUpdate();

        /* State Update. */
        base.OnFixedUpdate();     // Via PBehaviour: Runs OnFixedUpdate for the current State.
                                  // If Transition: curState.Exit() -> nextState.Enter().

        rigidBody.velocity = velocity;//((PBaseMovement_State)curState).velocity; // Gets the velocity from the current PBaseMovement_State.
        pInputManager.OnFixedUpdate(); // Resets all keyDown events in input controller.
    }

    public override void Transition(PState nextState)
    {
        base.Transition(nextState); // Calls exit and enter methods for prev and next state respectively.
    }

    /* ---- Methods for Readability (Called once, solely to slim down overriden methods above.) */

    /* Set the behaviour var in each state for referencing this Behaviour. Ideally this would be via constructor. */
    private void SetStateParentBehaviours()
    {
        //SAirborne.behaviour = this;
        //SAirborne.input = pInputManager;
        //SAirborne.collisionState = collisionState;
        //SAirborne.collisionManager = new PBaseMovement_Collision(SAirborne, collisionState);
        SAirborne.OnStart(this);

        //SIdle.behaviour = this;
        //SIdle.input = pInputManager;
        //SIdle.collisionState = collisionState;
        //SIdle.collisionManager = new PBaseMovement_Collision(SIdle, collisionState);
        SIdle.OnStart(this);

        SRunning.OnStart(this);
    }

    /* Set Lateral Input Vars: directionFacing, directionMoving, hasLateralInput*/
    private void UpdateLateralInputVars()
    {
        /* Update Direction Moving. */
        directionMoving = (velocity.x >= 0) ? (sbyte)1 : (sbyte)-1; // @sbyte an explicit cast. //((PBaseMovement_State)curState).

        /* Update Direction Facing. */
        if (pInputManager.KeyDown_Right)
        {
            directionFacing = 1;
        }
        else if (pInputManager.KeyDown_Left)
        {
            directionFacing = -1;
        }
        else if (pInputManager.KeyHeld_Left && !pInputManager.KeyHeld_Right)
        {
            directionFacing = -1;
        }
        else if (pInputManager.KeyHeld_Right && !pInputManager.KeyHeld_Left)
        {
            directionFacing = 1;
        }

        /* Check if there is Lateral input: For Camera Manager. */
        if (!pInputManager.KeyHeld_Right && !pInputManager.KeyHeld_Left)
        {
            hasLateralInput = false;
        }
        else
        {
            hasLateralInput = true;
        }
    }
}