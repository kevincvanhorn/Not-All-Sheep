using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PBaseMovement_Idle : PBaseMovement_State
{
    public override void OnStateEnter()
    {
        base.OnStateEnter();
        //Debug.Log("Idle Enter: Velocity " + behaviour.velocity);
    }

    public override void OnFixedUpdate()
    {
        base.OnFixedUpdate();                             // via PBaseMovement_State.
        // Note: DoCollisionBehaviour()
        //Debug.Log("Idle Update: Velocity " + behaviour.velocity);            
    }

    public override void OnInputBehaviour()
    {
        base.OnInputBehaviour();

        /* Vertical JUMP Calc ------------------------------------------ */
        if (input.KeyHeld_Up && !collisionState.Top && !collisionState.TopSlope)
        {
            behaviour.velocity.y = behaviour.jumpVelocityMax;
            behaviour.Transition(behaviour.SAirborne);
        }

        /* Lateral Calc -------------------------------------------------- */

    }

    /* Called prior to state transition, should not modify velocity. */
    public override void OnStateExit()
    {
        base.OnStateExit();
    }

    /* Class-specific Methods: */
}
