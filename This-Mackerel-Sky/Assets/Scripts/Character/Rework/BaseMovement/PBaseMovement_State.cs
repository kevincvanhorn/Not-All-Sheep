using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Contains all of the common state variables for Player Base Movement. */
/* Methods and variables here apply to every state for Base Movement.   */
public class PBaseMovement_State : PState {
    /* Execution Order:
     * FixedUpdate
     * OnCollisionEnter2D
     */

    public PBaseMovement behaviour;// The parent behaviour of the state - contains all of the shared variables for states in a behaviour.

    /* Variables that each PBaseMovement State has a copy of: */
    public Vector2 velocity = Vector2.zero;

    /* When any Base Movement state is entered. */
    public override void OnStateEnter()
    {
        base.OnStateEnter();
    }

    /* When any Base Movement state is Updated. */
    public override void OnFixedUpdate()
    {
        base.OnFixedUpdate();
        DoCollisionBehaviour();

    }

    /* When any Base Movement state is Exited. Should not change velocity. */
    public override void OnStateExit()
    {
        base.OnStateExit();
    }

    /* Called every FixedUpdate for actions based on current collision overlaps. 
    * Precondition: Persistent and Enter collision overlaps have been checked for this fixed frame. */
    public virtual void DoCollisionBehaviour()
    {

    }
}
