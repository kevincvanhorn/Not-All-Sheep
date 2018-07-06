using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PBaseMovement_Running : PBaseMovement_State {

    private class LocalCollisionManager : PBaseMovement_CollisionManager
    {
        public LocalCollisionManager(PBaseMovement_State owner, PCollisionState collisionState) : base(owner, collisionState)
        {
        }

        public override void SteepSlopeCollision()
        {
            /* Steep Slope Collision. */
            if (collisionState.SteepSlope_Enter)
            {
                if (collisionState.curSlopeAngle > PStats.slopeAngleMax && collisionState.curSlopeAngle < PStats.topAngleMin)
                {
                    if (Mathf.Abs(behaviour.velocity.x) >= 20)
                    {
                        behaviour.Transition(behaviour.SSteepSlope);
                    }
                    else
                    {
                        behaviour.velocity.x = 0;
                    }
                }
                else { Debug.LogError("SteepSlope Collision - Invalid Angle"); }
            }
        }

        public override void TopCollision()
        {
            /* Top Slope Collision. */
            if (collisionState.TopSlope_Enter)
            {
                if (!collisionState.Bot && !collisionState.Slope && collisionState.curSlopeAngle > PStats.wallAngleMax && collisionState.curSlopeAngle < PStats.topAngleMin)
                {
                    behaviour.velocity.y = 0; // Top Slope Collisions
                    behaviour.topSlopeSpeedCur = behaviour.velocity;
                    behaviour.Transition(behaviour.STopSlope);
                }
                else { Debug.LogError("TopCollision - Invalid Angle"); }
            }
        }

        public override void TopSlopeCollision()
        {
            base.TopSlopeCollision();
        }
    }

    public PBaseMovement_Running(PBaseMovement behaviourIn) : base(behaviourIn)
    {
        stateID = (int)PBaseMovement_States.Running; // 1
        collisionManager = new LocalCollisionManager(this, collisionState);
    }

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

        /* Sprint Calc ------------------------------------------------- */
        if (input.KeyHeld_Sprint)
        {
            behaviour.activeSpeed = behaviour.sprintSpeed;
        }

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
