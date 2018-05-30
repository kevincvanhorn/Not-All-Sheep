using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PBaseMovement_Idle : PBaseMovement_State
{
    public PBaseMovement_Idle(PBaseMovement behaviourIn) : base(behaviourIn)
    {
    }

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

        if (input.KeyHeld_Action)
        {
            behaviour.Transition(behaviour.SAction);
        }
        /* Vertical JUMP Calc ------------------------------------------ */
        else if (input.KeyHeld_Up && !collisionState.Top && !collisionState.TopSlope)
        {
            behaviour.velocity.y = behaviour.jumpVelocityMax;
            behaviour.Transition(behaviour.SAirborne);
        }

        /* Lateral Calc -------------------------------------------------- */
        else if (input.KeyHeld_Left || input.KeyHeld_Right)
        {
            if (collisionState.Slope)
            {
                //TODO: could be wall Slope
                if (collisionState.curSlopeAngle > PStats.slopeAngleMin && collisionState.curSlopeAngle <= PStats.slopeAngleMax)
                {
                    behaviour.Transition(behaviour.SClimbingSlope);
                }
                else
                {
                    Debug.LogError("TopCollision - Invalid Angle");
                }
            }
            else if (collisionState.Bot)
            {
                behaviour.Transition(behaviour.SRunning);
            }
            else if (collisionState.SteepSlope)
            {
                behaviour.Transition(behaviour.SSteepSlope);
            }
            else
            {
                Debug.LogError("ERROR: Invalid Idle Transition.");
                collisionState.printStatesError();
            }
        }
    }

    /* Called prior to state transition, should not modify velocity. */
    public override void OnStateExit()
    {
        base.OnStateExit();
    }

    /* Class-specific Methods: */
}
