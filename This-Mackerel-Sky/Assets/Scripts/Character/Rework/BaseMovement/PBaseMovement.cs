using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PBaseMovement : PBehaviour {
    
    /* Inherited Variables: */
    // Inherited: PInputManager pInputManager
    // Inherited: PState curState

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
    PBaseMovement_Airborne SAirborne; // -- TODO: 3.14.18 Tried polymorphism with PStates, but presented issues.

    private void Awake()
    {
        /* Get Components. */
        rigidBody = GetComponent<Rigidbody2D>(); // Note: Could be in thi.Start Method
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
        SetStateParentBehaviours();

        /* Set State. */
        curState = SAirborne;
    }

    public override void OnFixedUpdate()
    {
        /* Pre-State Update. */
        directionMoving = (((PBaseMovement_State)curState).velocity.x >= 0) ? (sbyte)1 : (sbyte)-1; // @sbyte an explicit cast. 

        /* State Update. */
        base.OnFixedUpdate(); // Runs OnFixedUpdate for the current State.
        rigidBody.velocity = ((PBaseMovement_State)curState).velocity; // Get the velocity from the current PBaseMovement_State
    }

    /* ---- Methods for Readability (Called once, solely to slim down overriden methods above.) */

    /* Set the behaviour var in each state to reference this Behaviour. */
    private void SetStateParentBehaviours()
    {
        SAirborne.behaviour = this;
    }
}