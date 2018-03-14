using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Contains all of the common state variables for Player Base Movement. */
/* Methods and variables here apply to every state for Base Movement.   */
public class PBaseMovement_State : PState {

    public PBaseMovement behaviour;                     // The parent behaviour of the state - contains all of the shared variables for states in a behaviour.

    /* Variables that this state will modify: */
    public Vector2 velocity;                            // Modified and shared by every PBaseMovement state individually

    /* When any Base Movement state is entered. */
    public override void OnStateEnter()
    {
        base.OnStateEnter();
    }

    /* When any Base Movement state is Updated. */
    public override void OnFixedUpdate()
    {
        base.OnFixedUpdate();
    }

    /* When any Base Movement state is Exited. */
    public override void OnStateExit()
    {
        base.OnStateExit();
    }
}
