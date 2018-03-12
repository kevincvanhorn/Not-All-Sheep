using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PBaseMovement_Airborne : PState {
    public override void OnStateEnter()
    {
        base.OnStateEnter();
        Debug.Log("AIRBORNE - Enter");

    }

    public override void OnStateUpdate()
    {
        base.OnStateUpdate();

        velocity.y += gravity * Time.deltaTime; // Apply Gravity until grounded
    }

    public override void OnStateExit()
    {
        base.OnStateExit();
    }
}
