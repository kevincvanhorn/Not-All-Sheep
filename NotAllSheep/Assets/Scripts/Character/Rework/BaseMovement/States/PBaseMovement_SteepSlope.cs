using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PBaseMovement_SteepSlope : PBaseMovement_State {
    private class LocalCollisionManager : PBaseMovement_CollisionManager
    {
        public LocalCollisionManager(PBaseMovement_State owner, PCollisionState collisionState) : base(owner, collisionState)
        {
        }

        public override void WallCollision()
        {
            if (collisionState.Right_Enter || collisionState.Left_Enter)
            {
                behaviour.wallHitSpeed = behaviour.velocity;
                // TouchingWall.
                behaviour.velocity.x = 0;
                behaviour.velocity.y = 0;
            }
        }

        public override void TopCollision()
        {
            if (collisionState.Top_Enter)
            {
                behaviour.velocity.x = 0;
                behaviour.velocity.y = 0; // Redundancy case - addressed in this.Update.
            }
        }

        public override void TopSlopeCollision()
        {
            if (collisionState.TopSlope_Enter)
            {
                // 01.01.18b01 ?
                if (collisionState.curSlopeAngle > CStats.wallAngleMax && collisionState.curSlopeAngle < CStats.topAngleMin)
                { // TODO: Address this. Should stop moving if hits topSlope.
                    behaviour.velocity.x = 0;
                    behaviour.velocity.y = 0; // Redundancy case - addressed in this.Update.
                }
                else { Debug.LogError("TopCollision - Invalid Angle"); }
            }
        }

        public override void BotCollision()
        {
            if (collisionState.Bot_Enter)
            {
                //velocity.x = activeSpeed; //1.3.18
                behaviour.velocity.y = 0;
                behaviour.Transition(behaviour.SRunning);
            }
        }

        public override void SteepSlopeCollision()
        {
            if (collisionState.SteepSlope_Enter)
            {
                // TODO: Add the if statements and else to be sure of angle.
            }
        }

        public override void SlopeCollision()
        {
            if (collisionState.Slope_Enter)
            {
                //fsm.ChangeState(States.ClimbingSlope, StateTransition.Safe);
                if (collisionState.curSlopeAngle > PStats.slopeAngleMin && collisionState.curSlopeAngle <= PStats.slopeAngleMax)
                {
                    behaviour.Transition(behaviour.SClimbingSlope);
                }
                else { Debug.LogError("TopCollision - Invalid Angle " + collisionState.curSlopeAngle); }
            }
        }
    }

    public override void OnStart(PBaseMovement behaviourIn)
    {
        base.OnStart(behaviourIn);
        collisionManager = new LocalCollisionManager(this, collisionState);
    }


    public override void OnStateEnter()
    {
        base.OnStateEnter();

        Debug.Log("STEEPSLOPE - Enter");

        /* Sprint Calc ------------------------------------------------- */ //TODO: Remove from here - uneeded.
        if (input.KeyDown_Sprint)
        {
            behaviour.activeSpeed = behaviour.sprintSpeed;
        }
        else
        {
            behaviour.activeSpeed = behaviour.moveSpeed;
        }

        if (behaviour.velocity.y < 0 && !collisionState.Bot && !collisionState.Slope)
        {
            //Debug.DrawLine(debugSlopeHitLoc, debugSlopeHitLoc + (Vector3)steepSlopeHitNormal * 5, Color.blue, 10f);
            //Debug.DrawLine(debugSlopeHitLoc, debugSlopeHitLoc + velocity * -1, Color.yellow, 10f);
            float steepNormalAngle = Vector2.Angle(behaviour.velocity * -1, collisionState.steepSlopeHitNormal);
            behaviour.steepSlopeSpeed = Mathf.Sin(steepNormalAngle * Mathf.Deg2Rad) * behaviour.velocity.magnitude * Mathf.Sign(behaviour.velocity.y);

            //Debug.LogError("Velocity   " + velocity.magnitude);
            //Debug.LogError("Angle      " + steepNormalAngle);
            //Debug.LogError("Speed Calc " + steepSlopeSpeed);
        }
        else
        {
            behaviour.steepSlopeSpeed = behaviour.velocity.magnitude;
        }   
    }

    /* Switches order of input and collision. [This class only]*/
    public override void OnFixedUpdate()
    {
        base.OnFixedUpdate();
        //OnFixedUpdate_GrandparentCall();
        //OnInputBehaviour();
        //DoCollisionBehaviour();
    }

    /* Responds to any Input events - called after collision handling. */
    public override void OnInputBehaviour()
    {
        base.OnInputBehaviour();

        Debug.Log("STEEPSLOPE - Update");

        /* Lateral Calc -------------------------------------------------- */
        if (input.KeyHeld_Left || input.KeyHeld_Right)
        {
            if (collisionState.Right && input.KeyHeld_Right)
            {
                behaviour.velocity.x = 0;
                behaviour.velocity.y = 0;
            }
            else if (collisionState.Left && input.KeyHeld_Left)
            {
                behaviour.velocity.x = 0;
                behaviour.velocity.y = 0;
            }
        }

        behaviour.steepSlopeSpeed += behaviour.gravity * Time.deltaTime; // Slide down Slope

        behaviour.velocity.x = behaviour.steepSlopeSpeed * Mathf.Cos(collisionState.curSteepSlopeAngle * Mathf.Deg2Rad) * collisionState.steepSlopeDir; // steepSlopeSpeed
        behaviour.velocity.y = behaviour.steepSlopeSpeed * Mathf.Sin(collisionState.curSteepSlopeAngle * Mathf.Deg2Rad);

        if (collisionState.Top || collisionState.TopSlope)
        { // NOTE: Do not else with above, uses calculated velocity.
            if (behaviour.velocity.y > 0)
            {
                behaviour.velocity.x = 0;
                behaviour.velocity.y = 0;
            }
        }

        /* Priority Cases (Change the actual state). */
        if (input.KeyHeld_Action)
        { // Trigger Action.
            behaviour.Transition(behaviour.SAction);
        }
        else if (collisionState.None)
        { // Case - slide off edge
            Debug.Log("STEEPSLOPE - Transition 5");
            behaviour.Transition(behaviour.SAirborne);
        }
        else if ((collisionState.Left || collisionState.Right) && !collisionState.SteepSlope) // TODO: Test this. 
        { // Ran up slope and skid up wall
            // Case1.03: not on slope - just above at corner of slope and wall. 
            if (!collisionState.Top && !collisionState.TopSlope)
            {
                if (input.KeyDown_Up)
                {
                    Debug.Log("STEEPSLOPE - Transition 4");
                    //NOTE! Remember to copy this jump behavior to the Case1.03 Above
                    behaviour.velocity.y = behaviour.jumpVelocityMax;
                    behaviour.Transition(behaviour.SAirborne);
                }
                else if (behaviour.velocity.y >= 0 && (input.KeyDown_Left || input.KeyDown_Right))
                {
                    Debug.Log("STEEPSLOPE - Transition 7");
                    behaviour.velocity.y = behaviour.jumpVelocityMax;
                    behaviour.Transition(behaviour.SAirborne);
                }
            }
        }

        /* Vertical JUMP Calc ------------------------------------------ */
        // Jump if pressed && not touchingTop (ex: sandwiched between two platforms).
        else if (!collisionState.Top && !collisionState.TopSlope)
        {
            if (input.KeyDown_Up)
            {
                Debug.Log("STEEPSLOPE - Transition 3");
                //NOTE! Remember to copy this jump behavior to the Case1.03 Above
                behaviour.velocity.y = behaviour.jumpVelocityMax;
                behaviour.Transition(behaviour.SAirborne);
            }
            else if (behaviour.velocity.y >= 0 && (input.KeyDown_Left || input.KeyDown_Right))
            {
                Debug.Log("STEEPSLOPE - Transition 6");
                behaviour.velocity.y = behaviour.jumpVelocityMax;
                behaviour.Transition(behaviour.SAirborne);
            }
        }
    }

    /* Called prior to state transition, should not modify velocity. */
    public override void OnStateExit()
    {
        base.OnStateExit();
    }
}