using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PBaseMovement_Idle : PBaseMovement_State
{
    public override void OnStateEnter()
    {
        base.OnStateEnter();
    }

    public override void OnFixedUpdate()
    {
        base.OnFixedUpdate();                             // via PBaseMovement_State.
        // Note: DoCollisionBehaviour()
    }

    /* Called every FixedUpdate for actions based on current collision overlaps. 
    * Precondition: persistent collision overlaps have been checked for this fixed frame. */
    public override void DoCollisionBehaviour()
    {
        base.DoCollisionBehaviour();
        
    }

    /* Called prior to state transition, should not modify velocity. */
    public override void OnStateExit()
    {
        base.OnStateExit();
    }

    /* Class-specific Methods: */
}
