using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PBaseMovement_TopSlope : PBaseMovement_State {
    public override void OnStart(PBaseMovement behaviourIn)
    {
        base.OnStart(behaviourIn);
    }


    public override void OnStateEnter()
    {
        base.OnStateEnter();

        behaviour.velocity = behaviour.topSlopeSpeedCur;
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

        float platformSlippiness = 20;
        behaviour.topSlideFactor = 1 / (Mathf.Abs(behaviour.velocity.x)) * 100 + platformSlippiness;
        if (behaviour.topSlideFactor < 1)
        {
            behaviour.topSlideFactor = 1;
        }

        behaviour.velocity = behaviour.topSlopeSpeedCur;

        Vector2 slopeVector = (Vector2)(Quaternion.Euler(0, 0, 180 - collisionState.curSlopeAngle) * Vector2.right); //PFEF
        float hitAngle = Vector2.Angle(behaviour.velocity, slopeVector);
        //velocity.x = slopeHitSpeed.x * Mathf.Cos((180 - slopeAngle) * Mathf.Deg2Rad); // - velocity?
        behaviour.velocity.y = Mathf.Abs(behaviour.topSlopeSpeedCur.x) * Mathf.Sin((180 - collisionState.curSlopeAngle) * Mathf.Deg2Rad);

        //Debug.Log("SlideFactor " + behaviour.topSlideFactor);
        //Debug.Log("Slope Angle: " + (180 - collisionState.curSlopeAngle));
        //Debug.LogError("Slope Vector: " + slopeVector);
        //Debug.Log("Hit Angle : " + hitAngle);
        //Debug.DrawLine(new Vector3(0,0,0), slopeVector, Color.yellow, 20);
        //Debug.DrawLine(new Vector3(0, 0, 0), velocity, Color.red, 20);
        Debug.DrawLine(collisionState.debugSlopeHitLoc, collisionState.debugSlopeHitLoc + (Vector3)slopeVector, Color.green, 20);
        Debug.DrawLine(collisionState.debugSlopeHitLoc, collisionState.debugSlopeHitLoc + (Vector3)behaviour.velocity, Color.red, 20);

        if (behaviour.velocity.x == 0)
        {
            behaviour.topSlopeSpeedCur.y = 0;
        }
        else
        {
            behaviour.topSlopeSpeedCur.y += behaviour.gravity * behaviour.topSlideFactor * Time.deltaTime; // Apply Gravity until grounded
        }

        if (behaviour.topSlopeSpeedCur.y <= 0)
        {
            behaviour.Transition(behaviour.SAirborne);
        }

        if (!collisionState.Slope)
        {
            behaviour.Transition(behaviour.SAirborne);
        }
    }

    /* Called prior to state transition, should not modify velocity. */
    public override void OnStateExit()
    {
        base.OnStateExit();
    }
}