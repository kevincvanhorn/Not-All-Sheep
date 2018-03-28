using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* A modular, collision behaviour manager that handles transitions and velocity updates. */
public class PBaseMovement_CollisionManager
{
    public PBaseMovement_State owner;
    public PCollisionState collisionState;
    private PBaseMovement behaviour;

    public PBaseMovement_CollisionManager(PBaseMovement_State owner, PCollisionState collisionState)
    {
        this.owner = owner;
        this.collisionState = collisionState;
        behaviour = owner.behaviour;
    }

    public void DoCollisionBehaviour()
    {
        if (!collisionState.None)
        {
            BotCollision();
            SlopeCollision();
            WallCollision();
            SteepSlopeCollision();
            TopSlopeCollision();
            TopCollision();
        }
    }

    public virtual void BotCollision()
    {
        /* Bot Collision. */
        if (collisionState.Bot_Enter)
        {
            behaviour.velocity.y = 0;
            if (behaviour.velocity.x == 0) { behaviour.Transition(behaviour.SIdle); }
            else { behaviour.Transition(behaviour.SRunning); }
        }
    }

    public virtual void SlopeCollision()
    {
        /* Slope Collision. */
        if (collisionState.Slope_Enter)
        {
            if (collisionState.curSlopeAngle > PStats.slopeAngleMin && collisionState.curSlopeAngle <= PStats.slopeAngleMax)
            {
                behaviour.Transition(behaviour.SClimbingSlope);
            }
            else { Debug.LogError("Slope Collision - Invalid Angle"); }
        }
    }

    public virtual void WallCollision()
    {
        /* Wall Collision (Including Wall Slope). */
        if (collisionState.Left_Enter)
        {
            behaviour.wallHitSpeed = behaviour.velocity;
            //Debug.LogError("--Wallhitspeed" + wallHitSpeed);
            behaviour.velocity.x = 0; // Commented Out 1.5.18
            behaviour.velocity.y = 0; // added 1.16.18
            if (!collisionState.Bot)
            {
                behaviour.Transition(behaviour.SOnWall);
            }
            else
            {
                behaviour.velocity.x = 0;
                behaviour.velocity.y = 0;
                if (behaviour.velocity.x == 0) { behaviour.Transition(behaviour.SIdle); }
                else { behaviour.Transition(behaviour.SRunning); }
            }
        }
        else if (collisionState.Right_Enter)
        {
            behaviour.wallHitSpeed = behaviour.velocity;
            //Debug.LogError("--Wallhitspeed" + wallHitSpeed);
            behaviour.velocity.x = 0; // Commented Out 1.5.18
            behaviour.velocity.y = 0; // added 1.16.18
            if (!collisionState.Bot)
            {
                behaviour.Transition(behaviour.SOnWall);
            }
            else
            {
                Debug.LogWarning("AIRBORNE: This state should be inaccessible - grounded & touchingWall");
            }
        }
    }

    public virtual void SteepSlopeCollision()
    {
        /* Steep Slope Collision. */
        if (collisionState.SteepSlope_Enter)
        {
            if (collisionState.curSlopeAngle > PStats.slopeAngleMax && collisionState.curSlopeAngle < PStats.topAngleMin)
            {
                behaviour.Transition(behaviour.SSteepSlope);
            }
            else { Debug.LogError("SteepSlope Collision - Invalid Angle"); }
        }
    }

    public virtual void TopSlopeCollision()
    {
        /* Top Slope Collision. */
        if (collisionState.TopSlope_Enter)
        {
            behaviour.velocity.y = 0;
            if (collisionState.curSlopeAngle > PStats.wallAngleMax && collisionState.curSlopeAngle < PStats.topAngleMin)
            {
                behaviour.topSlopeSpeedCur = behaviour.velocity;
                behaviour.Transition(behaviour.STopSlope);
            }
            else { Debug.LogError("TopCollision - Invalid Angle"); }
            //Debug.DrawLine(debugSlopeHitLoc, debugSlopeHitLoc + velocity, Color.yellow, 20); // DEBUG.
        }
    }

    public virtual void TopCollision()
    {
        /* Top Collision. */
        if (collisionState.Top_Enter)
        {
            behaviour.velocity.y = 0;
        }
    }

}
