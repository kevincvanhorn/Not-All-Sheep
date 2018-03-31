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
        base.OnFixedUpdate(); // via PBaseMovement_State.
        // Note: DoCollisionBehaviour()
        behaviour.velocity.y += behaviour.gravity * Time.deltaTime; // Apply Gravity until grounded   
        //Debug.Log("Airborne: " + behaviour.velocity);
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
