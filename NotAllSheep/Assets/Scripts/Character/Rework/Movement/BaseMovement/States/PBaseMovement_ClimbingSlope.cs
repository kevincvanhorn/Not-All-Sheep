using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PBaseMovement_ClimbingSlope : PBaseMovement_State
{
    private class LocalCollisionManager : PBaseMovement_CollisionManager
    {
        public LocalCollisionManager(PBaseMovement_State owner, PCollisionState collisionState) : base(owner, collisionState)
        {
        }

        public override void WallCollision()
        {
            //base.WallCollision(); //TODO: Test 

            /* Wall Collision (Including Wall Slopes). */
            if (collisionState.Right_Enter)
            {
                behaviour.wallHitSpeed = behaviour.velocity;
                // TouchingWall.
                behaviour.velocity.x = 0;
                behaviour.velocity.y = 0;
            }
            else if (collisionState.Left_Enter)
            {
                behaviour.wallHitSpeed = behaviour.velocity;
                // TouchingWall.
                behaviour.velocity.x = 0;
                behaviour.velocity.y = 0;
            }
        }

        public override void TopCollision()
        {
            //base.TopCollision();
            if (collisionState.Top_Enter)
            {
                behaviour.velocity.x = 0;
                behaviour.velocity.y = 0; // Redundancy case - addressed in update
            }
        }

        public override void SteepSlopeCollision()
        {
            if(collisionState.SteepSlope_Enter)
            {
                if (collisionState.curSlopeAngle > PStats.slopeAngleMax && collisionState.curSlopeAngle < PStats.topAngleMin)
                {
                    if (behaviour.activeSpeed >= 20)//Mathf.Abs(velocity.x) >= 20)
                    {
                        behaviour.Transition(behaviour.SSteepSlope);
                    }
                    else
                    {
                        behaviour.velocity.x = 0;
                        behaviour.velocity.y = 0;
                    }
                }
                else { Debug.LogError("SteepSlope Collision - Invalid Angle"); }
            }
        }

        public override void SlopeCollision()
        {
            if(collisionState.Slope_Enter)
            {
                if (collisionState.curSlopeAngle > PStats.slopeAngleMin && collisionState.curSlopeAngle <= PStats.slopeAngleMax)
                {
                    //Stay
                }
                else { Debug.LogError("TopCollision - Invalid Angle " + collisionState.curSlopeAngle); }
            }
        }
    }

    public PBaseMovement_ClimbingSlope(PBaseMovement behaviourIn) : base(behaviourIn)
    {
        collisionManager = new LocalCollisionManager(this, collisionState);
    }

    public override void OnStateEnter()
    {
        base.OnStateEnter();

        behaviour.climbSlopeHitSpeed = behaviour.velocity;
        behaviour.velocity.x = behaviour.climbSlopeHitSpeed.x * Mathf.Cos(collisionState.curSlopeAngle * Mathf.Deg2Rad); // * directionMoving if not just .x
        behaviour.velocity.y = Mathf.Abs(behaviour.climbSlopeHitSpeed.x) * Mathf.Sign(behaviour.velocity.y) * Mathf.Sin(collisionState.curSlopeAngle * Mathf.Deg2Rad);
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

        //Debug.Log("dir: " + directionFacing + " R/L:" + Input.GetKey(KeyCode.RightArrow) +" "+Input.GetKey(KeyCode.LeftArrow));

        /* Sprint Calc ------------------------------------------------- */
        if (input.KeyHeld_Sprint) { behaviour.activeSpeed = behaviour.sprintSpeed; }
        //else { //activeSpeed = moveSpeed; }

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
            else
            {
                //  Debug.LogError(Mathf.Cos(collisionState.curSlopeAngle * Mathf.Deg2Rad) + " " + Mathf.Sign(collisionState.curSlopeAngle * Mathf.Deg2Rad));

                /* Acceleration. */
                if (behaviour.directionFacing == 1)
                {
                    if (behaviour.velocity.x < behaviour.activeSpeed * Mathf.Cos(collisionState.curSlopeAngle * Mathf.Deg2Rad))
                    {
                        //Debug.Log("1: "+ behaviour.lateralAccelGrounded + " " + Mathf.Cos(collisionState.curSlopeAngle * Mathf.Deg2Rad) +  " " + behaviour.directionFacing);
                        behaviour.velocity.x += behaviour.lateralAccelGrounded * Mathf.Cos(collisionState.curSlopeAngle * Mathf.Deg2Rad) * Time.deltaTime * behaviour.directionFacing;
                        behaviour.velocity.y += behaviour.lateralAccelGrounded * Mathf.Sin(collisionState.curSlopeAngle * Mathf.Deg2Rad) * collisionState.slopeDir * behaviour.directionFacing * Time.deltaTime;
                    }
                    else
                    {
                        //Debug.Log("2");
                        behaviour.velocity.x = behaviour.activeSpeed * Mathf.Cos(collisionState.curSlopeAngle * Mathf.Deg2Rad) * behaviour.directionFacing;
                        behaviour.velocity.y = behaviour.activeSpeed * Mathf.Sin(collisionState.curSlopeAngle * Mathf.Deg2Rad) * collisionState.slopeDir * behaviour.directionFacing;
                    }
                }
                else if (behaviour.directionFacing == -1)
                {
                    if (behaviour.velocity.x > -1 * behaviour.activeSpeed * Mathf.Cos(collisionState.curSlopeAngle * Mathf.Deg2Rad))
                    {
                        behaviour.velocity.x += behaviour.lateralAccelGrounded * Mathf.Cos(collisionState.curSlopeAngle * Mathf.Deg2Rad) * Time.deltaTime * behaviour.directionFacing;
                        behaviour.velocity.y += behaviour.lateralAccelGrounded * Mathf.Sin(collisionState.curSlopeAngle * Mathf.Deg2Rad) * collisionState.slopeDir * behaviour.directionFacing * Time.deltaTime;
                    }
                    else
                    {
                        behaviour.velocity.x = behaviour.activeSpeed * Mathf.Cos(collisionState.curSlopeAngle * Mathf.Deg2Rad) * behaviour.directionFacing;
                        behaviour.velocity.y = behaviour.activeSpeed * Mathf.Sin(collisionState.curSlopeAngle * Mathf.Deg2Rad) * collisionState.slopeDir * behaviour.directionFacing;
                    }
                }
            }

        }

        /* X Deceleration ---------------------------------------------- */
        else if (!input.KeyHeld_Right && !input.KeyHeld_Left)
        { // On-release of Lateral Movement controls - Deccelerate
            //Debug.LogError("DEVCELELLLELLEEL");
            if (behaviour.directionMoving == 1)
            { // Decceleration Right
                if (behaviour.velocity.x >= 0)
                {
                    //Debug.LogError("Case 1.");
                    behaviour.velocity.x -= behaviour.lateralAccelGrounded * Mathf.Cos(collisionState.curSlopeAngle * Mathf.Deg2Rad) * Time.deltaTime;
                    behaviour.velocity.y += collisionState.slopeDir * -1 * behaviour.lateralAccelGrounded * Mathf.Sin(collisionState.curSlopeAngle * Mathf.Deg2Rad) * Time.deltaTime;
                }
                if (behaviour.velocity.x < 0)
                {
                    //Debug.Log("Halt 1.");
                    behaviour.velocity.y = 0;
                    behaviour.velocity.x = 0;
                }
            }
            else if (behaviour.directionMoving == -1)
            { // Decceleration Left
                if (behaviour.velocity.x < 0)
                {
                    //Debug.LogError("Case 2.");
                    behaviour.velocity.x += behaviour.lateralAccelGrounded * Mathf.Cos(collisionState.curSlopeAngle * Mathf.Deg2Rad) * Time.deltaTime;
                    behaviour.velocity.y += collisionState.slopeDir * behaviour.lateralAccelGrounded * Mathf.Sin(collisionState.curSlopeAngle * Mathf.Deg2Rad) * Time.deltaTime;
                }
                if (behaviour.velocity.x >= 0)
                {
                    Debug.Log("Halt 2.");
                    behaviour.velocity.y = 0;
                    behaviour.velocity.x = 0;
                }
            }
        }

        if (collisionState.Top || collisionState.TopSlope)
        { // NOTE: Do not else with above, uses calculated velocity.
            if (behaviour.velocity.y > 0)
            {
                behaviour.velocity.x = 0;
                behaviour.velocity.y = 0;
            }
        }

        /* Steep Slope Min Velocity. */
        if (collisionState.SteepSlope && behaviour.activeSpeed < behaviour.steepSlopeMinEnterSpeed)
        {
            if (behaviour.velocity.x < 0 && collisionState.steepSlopeDir == -1)
            {
                behaviour.velocity.x = 0;
                behaviour.velocity.y = 0;
            }
            else if (behaviour.velocity.x > 0 && collisionState.steepSlopeDir == 1)
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
            behaviour.Transition(behaviour.SAirborne);
        }
        else if ((collisionState.Left || collisionState.Right) && !collisionState.Slope)
        { // Ran up slope and skid up wall
            // Case1.03: not on slope - just above at corner of slope and wall. 
            if (input.KeyHeld_Up && !collisionState.Top && !collisionState.TopSlope)
            {
                behaviour.velocity.y = behaviour.jumpVelocityMax;
                behaviour.Transition(behaviour.SOnWall);
            }
        }

        /* Vertical JUMP Calc ------------------------------------------ */
        // Jump if pressed or held && not touchingTop (ex: sandwiched between two platforms).
        else if (input.KeyHeld_Up && !collisionState.Top && !collisionState.TopSlope)
        {
            behaviour.velocity.y = behaviour.jumpVelocityMax;
            behaviour.Transition(behaviour.SAirborne);
        }

        /* Steep Slope Min Velocity. */
        else if (collisionState.SteepSlope && behaviour.activeSpeed >= behaviour.steepSlopeMinEnterSpeed)
        {
            if (behaviour.velocity.x < 0 && collisionState.steepSlopeDir == -1)
            {
                behaviour.Transition(behaviour.SSteepSlope);
            }
            else if (behaviour.velocity.x > 0 && collisionState.steepSlopeDir == 1)
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