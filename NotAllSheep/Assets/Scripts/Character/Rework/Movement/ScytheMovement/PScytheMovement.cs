using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PScytheMovement : PBaseMovement {

    /* Inherited Variables: */
    //public PBaseMovement_State SOnWall, SSteepSlope, STopSlope, SClimbingSlope, SDashing, SAction, SAirborne, SIdle, SRunning;
    public new PBaseMovement_State SAirborne;

    public override void Awake()
    {
        base.Awake();
    }

    public override void OnStart()
    {
        /* Input Setup. */
        inputFilter = new List<PInput>() { PInput.Vertical, PInput.Horizontal, PInput.Up, PInput.Down, PInput.Left, PInput.Right, PInput.Sprint, PInput.Dash };
        base.OnStart(); // Creates Input Manager.

        /* Calc Movement Variables. */
        gravity = -(2 * PStats.jumpHeightMax) / Mathf.Pow(PStats.timeToJumpApex, 2);
        jumpVelocityMax = Mathf.Abs(gravity * PStats.timeToJumpApex);
        jumpVelocityMin = Mathf.Sqrt(2 * Mathf.Abs(gravity) * PStats.jumpHeightMin);
        activeSpeed = moveSpeed;

        wallHitSpeed.x = activeSpeed;
        //wallFrictionDown = 1;

        /* Create States. */
        SAirborne = new PScytheMovement_Airborne(this); // Overridden Airborne type.
        SIdle = new PBaseMovement_Idle(this);
        SRunning = new PBaseMovement_Running(this);
        SOnWall = new PBaseMovement_OnWall(this);
        SClimbingSlope = new PBaseMovement_ClimbingSlope(this);
        STopSlope = new PBaseMovement_TopSlope(this);
        SSteepSlope = new PBaseMovement_SteepSlope(this);

        /* Set State. */
        curState = SAirborne;
    }

    public override void OnFixedUpdate()
    {
        base.OnFixedUpdate();
    }

    public override void Transition(PState nextState)
    {
        base.Transition(nextState);
    }
}
