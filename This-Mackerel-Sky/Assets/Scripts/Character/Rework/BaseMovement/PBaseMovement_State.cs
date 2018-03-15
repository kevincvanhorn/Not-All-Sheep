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

    public PBaseMovement behaviour;                     // The parent behaviour of the state - contains all of the shared variables for states in a behaviour.

    /* Variables that this state will modify: */
    public Vector2 velocity;                            // Modified and shared by every PBaseMovement state individually

    /* When any Base Movement state is entered. */
    public override void OnStateEnter()
    {
        base.OnStateEnter();
    }

    /* When any Base Movement state is Updated. 
     * Precondition: Any new collisions from the movements in the last fixed frame are stored in enterCollisions.
     */
    public override void OnFixedUpdate()
    {
        base.OnFixedUpdate();
        CollisionBehaviour();

        /* End of FixedUpdate, after all calculations. */
        behaviour.enterCollisionTypes.Clear(); // Clear enterCollisions, filled again by OnCollisionEnter before end of Fixed Frame.
    }

    /* When any Base Movement state is Exited. */
    public override void OnStateExit()
    {
        base.OnStateExit();
    }

    /* Called every FixedUpdate for actions based on current collision overlaps. 
    * Precondition: persistent collision overlaps have been checked for this fixed frame. */
    public virtual void CollisionBehaviour()
    {
        // TODO: Replace OnCollisionEnter2D with maintaining a list of enterCollisions - cleared at beginning of this method.
    }

    /* Called after FixedUpdate, addressed in next fixed frame before cleared. 
     * Precondition: enterCollisionTypes has been cleared & all FixedUpdate calculations have completed for this fixed frame.
     */
    public void OnCollisionEnter2D(Collision2D collision)
    {
        ContactPoint2D[] contactsIn = new ContactPoint2D[4]; // 2 when side collides (each corner) || 1 when on slope
        collision.GetContacts(contactsIn);
    }
}
