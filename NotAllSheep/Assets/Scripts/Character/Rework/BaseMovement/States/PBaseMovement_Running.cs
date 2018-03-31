using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PBaseMovement_Running : PBaseMovement_State {

    public override void OnStateEnter()
    {
        base.OnStateEnter();
    }

    public override void OnFixedUpdate()
    {
        base.OnFixedUpdate(); // via PBaseMovement_State.
        // Note: DoCollisionBehaviour()
    }

    /* Responds to any Input events - called after collision handling. */
    public override void OnInputBehaviour()
    {
        base.OnInputBehaviour();

        /* Lateral Calc -------------------------------------------------- */
        if (input.KeyHeld_Left || input.KeyHeld_Right)
        {
            /* Acceleration. */
            if (behaviour.directionFacing == 1)
            {
                if (behaviour.velocity.x < behaviour.activeSpeed)
                {
                    behaviour.velocity.x += behaviour.lateralAccelGrounded * Time.deltaTime * behaviour.directionFacing;
                }
                else
                {
                    behaviour.velocity.x = behaviour.activeSpeed * behaviour.directionFacing;
                }
            }
            else if (behaviour.directionFacing == -1)
            {
                if (behaviour.velocity.x > -1 * behaviour.activeSpeed)
                {
                    behaviour.velocity.x += behaviour.lateralAccelGrounded * Time.deltaTime * behaviour.directionFacing;
                }
                else
                {
                    behaviour.velocity.x = behaviour.activeSpeed * behaviour.directionFacing;
                }
            }
        }

        /* X Deceleration ---------------------------------------------- */
        else if (behaviour.velocity.x != 0 && !input.KeyHeld_Right && !input.KeyHeld_Left)
        { // On-release of Lateral Movement controls - Deccelerate
            if (behaviour.directionMoving == 1)
            { // Decceleration Right
                if (behaviour.velocity.x >= 0)
                {
                    behaviour.velocity.x -= behaviour.lateralAccelGrounded * Time.deltaTime;
                }
                if (behaviour.velocity.x < 0) { behaviour.velocity.x = 0; }
            }
            else if (behaviour.directionMoving == -1)
            { // Decceleration Left
                if (behaviour.velocity.x < 0)
                {
                    behaviour.velocity.x += behaviour.lateralAccelGrounded * Time.deltaTime;
                }
                if (behaviour.velocity.x >= 0) { behaviour.velocity.x = 0; }
            }
        }

        /* Run/deccelerate into wall - Applied here once instead of conditionals above. */
        if (behaviour.velocity.x > 0 && collisionState.Right)
        {
            behaviour.velocity.x = 0;
        }
        else if (behaviour.velocity.x < 0 && collisionState.Left)
        {
            behaviour.velocity.x = 0;
        }

        /* Steep Slope Min Velocity. */
        if (collisionState.SteepSlope)
        {
            if (behaviour.velocity.x > -PStats.steepSlopeMinEnterSpeed && behaviour.velocity.x < 0 && collisionState.slopeDir == -1)
            {
                if (input.KeyHeld_Left)
                {
                    behaviour.velocity.x = 0;
                }

            }
            else if (behaviour.velocity.x < PStats.steepSlopeMinEnterSpeed && behaviour.velocity.x > 0 && collisionState.slopeDir == 1)
            {
                if (input.KeyHeld_Right)
                {
                    behaviour.velocity.x = 0;
                }

            }
        }

        /* Priority Cases*/
        if (input.KeyHeld_Action)
        { // Trigger Action.
            behaviour.Transition(behaviour.SAction);
        }
        else if (collisionState.None)
        { // Case - slide off edge
            behaviour.Transition(behaviour.SAirborne);
        }

        /* Vertical JUMP Calc ------------------------------------------ */
        // Jump if pressed or held && not touchingTop (ex: sandwiched between two platforms).
        else if (input.KeyHeld_Up && !collisionState.Top && !collisionState.TopSlope)
        {
            behaviour.velocity.y = behaviour.jumpVelocityMax;
            behaviour.Transition(behaviour.SAirborne);
        }
        else if (behaviour.velocity.x == 0 && !input.KeyHeld_Right && !input.KeyHeld_Left)
        {
            behaviour.Transition(behaviour.SIdle);
        }
        else if (collisionState.SteepSlope && Mathf.Abs(behaviour.velocity.x) >= PStats.steepSlopeMinEnterSpeed)
        {
            if (behaviour.velocity.x < 0 && collisionState.slopeDir == -1)
            {
                behaviour.Transition(behaviour.SSteepSlope);
            }
            else if (behaviour.velocity.x > 0 && collisionState.slopeDir == 1)
            {
                behaviour.Transition(behaviour.SSteepSlope);
            }
        }
    }

    /* Called prior to state transition, should not modify velocity. */
    public override void OnStateExit()
    {
        base.OnStateExit();
    }
}
