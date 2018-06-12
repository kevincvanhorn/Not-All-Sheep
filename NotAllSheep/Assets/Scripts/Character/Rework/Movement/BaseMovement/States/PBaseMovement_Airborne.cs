using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PBaseMovement_Airborne : PBaseMovement_State {

    public PBaseMovement_Airborne(PBaseMovement behaviourIn) : base(behaviourIn)
    {
    }

    public override void OnStateEnter()
    {
        base.OnStateEnter();
    }

    public override void OnFixedUpdate()
    {
        base.OnFixedUpdate(); // via PBaseMovement_State.
        // Note: DoCollisionBehaviour()
        //behaviour.velocity.y += behaviour.gravity * Time.deltaTime; // Apply Gravity until grounded   
        //Debug.Log("Airborne: " + behaviour.velocity);
    }

    /* Responds to any Input events - called after collision handling. */
    public override void OnInputBehaviour()
    {
        base.OnInputBehaviour();

        /* Vertical Calc ----------------------------------------- */
        if (input.KeyUp_Up)
        {  // Variable jump - When Up is released in this frame.
            if (behaviour.velocity.y > behaviour.jumpVelocityMin)
            {
                behaviour.velocity.x = (behaviour.jumpVelocityMin * behaviour.velocity.x) / behaviour.velocity.y;
                behaviour.velocity.y = behaviour.jumpVelocityMin;
            }
        }

        /* Lateral Calc -------------------------------------------*/
        if (input.KeyHeld_Right && behaviour.velocity.x < behaviour.activeSpeed)
        { // in-air lateral move right
            behaviour.velocity.x += behaviour.lateralAccelAirborne * Time.deltaTime;
        }
        else if (input.KeyHeld_Left && behaviour.velocity.x > -behaviour.activeSpeed)
        { // in-air lateral move left
            behaviour.velocity.x -= behaviour.lateralAccelAirborne * Time.deltaTime;
        }

        if (collisionState.Top && behaviour.velocity.y > 0)
        {
            behaviour.velocity.y = 0;
        }

        behaviour.velocity.y += behaviour.gravity * Time.deltaTime; // Apply Gravity until grounded

        // Jumping While Against Wall.
        if (collisionState.Right || collisionState.Left)
        {
            if (behaviour.velocity.x != 0) behaviour.wallHitSpeed = behaviour.velocity;
            behaviour.velocity.x = 0;
            behaviour.Transition(behaviour.SOnWall);
        }

        // Trigger Action.
        if (input.KeyHeld_Action)
        {
            behaviour.Transition(behaviour.SAction);
        }
    }

    /* Called prior to state transition, should not modify velocity. */
    public override void OnStateExit()
    {
        base.OnStateExit();
    }



    /* Class-specific Methods: */

}
