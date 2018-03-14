using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PBaseMovement : PBehaviour {
    //Inherited: PInputManager pInputManager
    //Inherited: PState curState
    public Rigidbody2D rigidBody;

    /* Airborne Variables: */
    public float gravity;
    public const float jumpHeightMax = 5;
    public const float jumpHeightMin = .9f;
    public const float timeToJumpApex = .4f;

    /* States: */
    // 3.14.18 Tried polymorphism with PStates, but presented issues.
    PBaseMovement_Airborne SAirborne;

    public override void Start()
    {
        /* Input Setup. */
        inputFilter = new List<PInput>() { PInput.Vertical, PInput.Horizontal, PInput.Up, PInput.Down, PInput.Left, PInput.Right, PInput.Sprint, PInput.Dash };
        base.Start(); // Creates Input Manager.

        /* Calc Movement Variables. */
        gravity = -(2 * jumpHeightMax) / Mathf.Pow(timeToJumpApex, 2);

        /* Create States. */
        SAirborne = gameObject.AddComponent(typeof(PBaseMovement_Airborne)) as PBaseMovement_Airborne;
        SetStateParentBehaviours();

        /* Get Components. */
        rigidBody = GetComponent<Rigidbody2D>();
        curState = SAirborne;
    }

    private void SetStateParentBehaviours() {
        SAirborne.behaviour = this;
    }

    public override void OnFixedUpdate()
    {
        base.OnFixedUpdate(); // Runs OnStateUpdate for the current State.
        rigidBody.velocity = ((PBaseMovement_State)curState).velocity; // Get the velocity from the current PBaseMovement_State
    }
}