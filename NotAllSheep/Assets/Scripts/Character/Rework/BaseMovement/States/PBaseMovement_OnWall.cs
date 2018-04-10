using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PBaseMovement_OnWall : PBaseMovement_State {
    private class LocalCollisionManager : PBaseMovement_CollisionManager
    {
        public LocalCollisionManager(PBaseMovement_State owner, PCollisionState collisionState) : base(owner, collisionState)
        {
        }

        public override void SlopeCollision()
        {
            if (collisionState.Slope_Enter)
            {
                if (collisionState.curSlopeAngle > PStats.slopeAngleMin && collisionState.curSlopeAngle <= PStats.slopeAngleMax)
                {
                    behaviour.velocity.x = 0;
                    behaviour.velocity.y = 0;
                    behaviour.Transition(behaviour.SClimbingSlope);
                }
                else { Debug.LogError("TopCollision - Invalid Angle"); }
            }
        }

        public override void TopSlopeCollision()
        {
            if (collisionState.SteepSlope)
            {
                if (collisionState.curSlopeAngle > PStats.wallAngleMax && collisionState.curSlopeAngle < PStats.topAngleMin)
                {
                    behaviour.velocity.y = 0;
                    behaviour.topSlopeSpeedCur = behaviour.velocity; // TODO: ~Maybe not imported from DoCollision Correctly.
                    behaviour.Transition(behaviour.STopSlope);       // TODO: ~Maybe not imported from DoCollision Correctly.
                }
                else { Debug.LogError("TopCollision - Invalid Angle"); }
            }
        }
    }

    public override void OnStart(PBaseMovement behaviourIn)
    {
        base.OnStart(behaviourIn);
        collisionManager = new LocalCollisionManager(this, collisionState);
    }

    public void OnWallEnter()
    {
        collisionState.printStatesWarning();
        //yield return new WaitForEndOfFrame();// WaitforEndofFrame();
        behaviour.velocity = behaviour.wallHitSpeed;
        behaviour.wallStickTime = Mathf.Abs(behaviour.wallHitSpeed.x); // 1.16.18
        Debug.LogWarning(collisionState.wallHitNormal);
        float wallNormalAngle = Vector2.Angle(behaviour.velocity * -1, collisionState.wallHitNormal);

        Debug.DrawLine(collisionState.debugWallHitLoc, collisionState.debugWallHitLoc + (Vector3)collisionState.wallHitNormal * 5, Color.blue, 10f);
        Debug.DrawLine(collisionState.debugWallHitLoc, collisionState.debugWallHitLoc + (Vector3)behaviour.velocity * -1 * 0.2f, Color.yellow, 10f);

        /* Enable / Disable Wallsticking 1.16.17 */
        if (!(collisionState.Left && input.KeyHeld_Right || collisionState.Right && input.KeyHeld_Left))
        {
            if (wallNormalAngle <= 45 && behaviour.velocity.y < 0 && input.KeyHeld_Up) // coming down hit wall: set vel 0 and keep falling
            {
                Debug.LogWarning("WallStick Case = 1");
                behaviour.wallCase = 1;
                behaviour.isWallSticking = true;
                behaviour.velocity = Vector2.zero;
            }
            /*else if (wallNormalAngle <= 20)
            {
                Debug.LogWarning("WallStick Case = 2");
                behaviour.wallCase = 2;
                behaviour.isWallSticking = true;
                behaviour.velocity = Vector2.zero;
            }*/
            else if (wallNormalAngle <= 45 && behaviour.velocity.y > 0)
            {
                Debug.LogWarning("WallStick Case = 3");
                behaviour.wallCase = 3;
                behaviour.isWallSticking = true;
            }
            else
            {
                Debug.LogWarning("WallStick = false");
                behaviour.isWallSticking = false;
            }
        }
        else behaviour.isWallSticking = false;


        if (!collisionState.Bot && !collisionState.Slope && !collisionState.TopSlope) // 1.13.18 - had vel > 0?
        {

            behaviour.wallFallSpeed = Mathf.Sin(wallNormalAngle * Mathf.Deg2Rad) * behaviour.velocity.magnitude * Mathf.Sign(behaviour.velocity.y);
             Debug.LogError("Velocity   " + behaviour.velocity.magnitude);
             Debug.LogError("WallHitAngle      " + wallNormalAngle);
             Debug.LogError("Speed Calc " + behaviour.wallFallSpeed);
        }
        else
        {
            behaviour.wallFallSpeed = behaviour.velocity.magnitude;
            //collisionState.printStatesError();
            Debug.LogWarning("WARNING: OnWall State: Probably should not be here");
        }
    }

    /* OnWall method for jumping player toward a wall */
    // @ param dir: 1 = left, -1 = right
    private void jumpTowardWall(int dir)
    {
        Debug.Log("JumpTowardWall : " + dir);
        behaviour.velocity.y = behaviour.jumpVelocityMax;
        behaviour.velocity.x = dir * behaviour.activeSpeed / 2;
        behaviour.Transition(behaviour.SAirborne);
    }

    /* OnWall method for jumping player away from a wall */
    // @ param dir: 1 = left, -1 = right
    private void jumpAwayFromWall(int dir)
    {
        Debug.Log("JumpAwayFromWall : " + dir);
        behaviour.velocity.y = behaviour.jumpVelocityMax;
        behaviour.velocity.x = dir * behaviour.activeSpeed;
        behaviour.Transition(behaviour.SAirborne);
    }

    public override void OnStateEnter()
    {
        base.OnStateEnter();
        OnWallEnter();
    }

    public override void OnFixedUpdate()
    {
        base.OnFixedUpdate(); // via PBaseMovement_State.
        // Note: DoCollisionBehaviour()
    }

    /* Responds to any Input events - called after collision handling. */
    public override void OnInputBehaviour()
    {
        collisionState.printStatesError();
        base.OnInputBehaviour();

        if (!collisionState.Right && !collisionState.Left)
        {
            Debug.Log("OnWall - touchingNone - transition to Airborne");
            behaviour.Transition(behaviour.SAirborne);
        }
        else
        {
            // Wall Stick Waiting
            if (behaviour.isWallSticking)
            {
                if (behaviour.wallStickTime > 0 && !(collisionState.Left && input.KeyHeld_Right || collisionState.Right && input.KeyHeld_Left))
                {
                    behaviour.wallStickTime += behaviour.gravity * Time.deltaTime;

                    if (behaviour.wallCase == 1)
                    {
                        behaviour.wallFallSpeed += behaviour.gravity * Time.deltaTime * 0.5f; // Slide down Slope 1.16.18
                        behaviour.velocity.x = behaviour.wallFallSpeed * Mathf.Cos(collisionState.curWallAngle * Mathf.Deg2Rad) * collisionState.slopeDir; // steepSlopeSpeed
                        behaviour.velocity.y = behaviour.wallFallSpeed * Mathf.Sin(collisionState.curWallAngle * Mathf.Deg2Rad);
                    }
                    if (behaviour.wallCase == 3)
                    {
                        if (behaviour.wallFallSpeed > 0)
                        {
                            behaviour.wallFallSpeed += behaviour.gravity * Time.deltaTime; // Slide down Slope 1.16.18
                            behaviour.velocity.x = behaviour.wallFallSpeed * Mathf.Cos(collisionState.curWallAngle * Mathf.Deg2Rad) * collisionState.slopeDir; // steepSlopeSpeed
                            behaviour.velocity.y = behaviour.wallFallSpeed * Mathf.Sin(collisionState.curWallAngle * Mathf.Deg2Rad);
                        }
                        if (behaviour.wallFallSpeed <= 0)
                        {
                            behaviour.velocity = Vector2.zero;
                            behaviour.wallFallSpeed = 0;
                        }
                    }
                }
                else
                {
                    behaviour.isWallSticking = false;
                }
            }

            else
            {
                // Apply gravity to dummy variable.
                behaviour.wallFallSpeed += behaviour.gravity * Time.deltaTime; // Slide down Slope 1.16.18

                behaviour.velocity.x = behaviour.wallFallSpeed * Mathf.Cos(collisionState.curWallAngle * Mathf.Deg2Rad) * collisionState.slopeDir; // steepSlopeSpeed
                behaviour.velocity.y = behaviour.wallFallSpeed * Mathf.Sin(collisionState.curWallAngle * Mathf.Deg2Rad);
            }

            // If only touching one side.
            if (!(collisionState.Left && collisionState.Right))
            {
                // When Up is released in this frame.
                if (input.KeyUp_Up)
                {
                    if (behaviour.wallFallSpeed > behaviour.jumpVelocityMin)
                    { // Keep applying velocity up while key is pressed - variable jump
                        behaviour.wallFallSpeed = behaviour.jumpVelocityMin;
                    }
                }

                // When Up is first input.
                if (input.KeyDown_Up)
                {
                    if (collisionState.Left && input.KeyHeld_Left) // Jump toward left wall.
                    {
                        jumpTowardWall(1);
                    }
                    else if (collisionState.Left && input.KeyHeld_Right) // Jump away from left wall
                    {
                        jumpAwayFromWall(1);
                    }
                    else if (collisionState.Right && input.KeyHeld_Right) // Jump toward right wall.
                    {
                        jumpTowardWall(-1);
                    }
                    else if (collisionState.Right && input.KeyHeld_Left) // Jump away from right wall.
                    {
                        jumpAwayFromWall(-1);
                    }
                }
                // When Right is first input.
                else if (input.KeyDown_Right)
                { // on L/R input - setting conditions.
                    behaviour.directionFacing = 1;
                    if (collisionState.Right && input.KeyHeld_Up) // Jumping toward right wall.
                    {
                        jumpTowardWall(-1);
                    }
                    else if (collisionState.Left && input.KeyHeld_Up) // Jump away from left wall.
                    {
                        jumpAwayFromWall(1);
                    }
                }
                // When Left is first input.
                else if (input.KeyDown_Left)
                {
                    behaviour.directionFacing = -1;
                    if (collisionState.Left && input.KeyHeld_Up) // Jumping toward left wall.
                    {
                        jumpTowardWall(1);
                    }
                    else if (collisionState.Right && input.KeyHeld_Up) // Jump away from right wall.
                    {
                        jumpAwayFromWall(-1);
                    }
                }

                // When Right or Left is held down.
                else if (input.KeyHeld_Right)
                {
                    if (collisionState.Left)
                    {
                        if (input.KeyHeld_Up) // Jump away from left wall.
                        {
                            jumpAwayFromWall(1);
                        }
                        else
                        { // Fall away from left wall
                            if (behaviour.velocity.y <= 0)
                            {
                                if (behaviour.velocity.x < 0) behaviour.velocity.x = 0;  // Needed for falling from wall but sticking bc velocity is negative into wall.
                                behaviour.velocity.x += behaviour.lateralAccelAirborne * Time.deltaTime;
                                behaviour.Transition(behaviour.SAirborne);
                            }
                            // Doesn't allow falling away from wall when wallRising
                        }
                    }
                    else if (collisionState.Right && input.KeyHeld_Up) // Jumping toward right wall.
                    {
                        // When coming from a non-grounded state, immediately jump when hit wall
                        if (behaviour.velocity.y < 0)
                        {
                            jumpTowardWall(-1);
                        }
                    }

                }
                else if (input.KeyHeld_Left)
                {
                    if (collisionState.Right)
                    {
                        if (input.KeyHeld_Up) // Jump away from right wall.
                        {
                            jumpAwayFromWall(-1);
                        }
                        else
                        { // Fall away from wall
                            if (behaviour.velocity.y <= 0)
                            {
                                if (behaviour.velocity.x > 0) behaviour.velocity.x = 0;// Needed for falling from wall but sticking bc velocity is negative into wall.
                                behaviour.velocity.x -= behaviour.lateralAccelAirborne * Time.deltaTime;
                                behaviour.Transition(behaviour.SAirborne);
                            }
                        }
                        // Doesn't allow falling away from wall when wallRising
                    }
                    else if (collisionState.Left && input.KeyHeld_Up) // Jumping toward left wall.
                    {
                        // When coming from a non-grounded state, immediately jump when hit wall
                        if (behaviour.velocity.y < 0)
                        {
                            jumpTowardWall(1);
                        }
                    }
                }
            }
        }

        //Debug.DrawLine(debugWallHitLoc, debugWallHitLoc + velocity * .25f, Color.yellow, 10f);
    }

    /* Called prior to state transition, should not modify velocity. */
    public override void OnStateExit()
    {
        base.OnStateExit();
    }
}
