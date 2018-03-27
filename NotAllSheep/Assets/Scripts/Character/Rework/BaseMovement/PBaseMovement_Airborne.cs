using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PBaseMovement_Airborne : PBaseMovement_State {
    public override void OnStateEnter()
    {
        base.OnStateEnter();
    }

    public override void OnFixedUpdate()
    {
        base.OnFixedUpdate();                             // via PBaseMovement_State.
        // Note: DoCollisionBehaviour()
        velocity.y += behaviour.gravity * Time.deltaTime; // Apply Gravity until grounded        
    }

    /* Called every FixedUpdate for actions based on current collision overlaps. 
    * Precondition: persistent collision overlaps have been checked for this fixed frame. */
    public override void DoCollisionBehaviour()
    {
        base.DoCollisionBehaviour();
        if (behaviour.collisionState.Bot_Enter)
        {
            velocity.y = 0;
            behaviour.Transition(behaviour.SIdle);
        }
    }

    /* Responds to any Input events - called after collision handling. */
    public override void OnInputBehaviour()
    {
        base.OnInputBehaviour();

    }

    /* Called prior to state transition, should not modify velocity. */
    public override void OnStateExit()
    {
        base.OnStateExit();
    }



    /* Class-specific Methods: */

}
