using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PBaseMovement_ExampleState : PBaseMovement_State {
    private class LocalCollisionManager : PBaseMovement_CollisionManager
    {
        public LocalCollisionManager(PBaseMovement_State owner, PCollisionState collisionState) : base(owner, collisionState)
        {
        }
    }

    public PBaseMovement_ExampleState(PBaseMovement behaviourIn) : base(behaviourIn)
    {
        stateID = (int)PBaseMovement_States.Null; //-1

        collisionManager = new LocalCollisionManager(this, collisionState);
    }

    public override void OnStateEnter()
    {
        base.OnStateEnter();
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
    }

    /* Called prior to state transition, should not modify velocity. */
    public override void OnStateExit()
    {
        base.OnStateExit();
    }
}