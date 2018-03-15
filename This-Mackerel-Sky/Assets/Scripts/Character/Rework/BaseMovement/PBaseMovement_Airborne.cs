using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PBaseMovement_Airborne : PBaseMovement_State {
    public override void OnStateEnter()
    {
        base.OnStateEnter();
        Debug.Log("AIRBORNE - Enter");
    }

    public override void OnFixedUpdate()
    {
        base.OnFixedUpdate();                             // via PBaseMovement_State.
        // Note: DoCollisionBehaviour()
        velocity.y += behaviour.gravity * Time.deltaTime; // Apply Gravity until grounded        
    }

    public override void OnStateExit()
    {
        base.OnStateExit();
    }

    /* Called every FixedUpdate for actions based on current collision overlaps. 
    * Precondition: persistent collision overlaps have been checked for this fixed frame. */
    public override void DoCollisionBehaviour()
    {
        base.DoCollisionBehaviour();
        if (behaviour.collisionState.Bot_Enter)
        {
            velocity.y = 0;
        }
    }

    /* Class-specific Methods: */

}
