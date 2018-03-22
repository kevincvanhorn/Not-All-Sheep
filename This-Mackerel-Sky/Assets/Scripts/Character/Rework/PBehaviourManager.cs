using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Responsible for executing and managing all behaviour configurations of the Player. 
 * The intent is to remove any uncertainty of execution order by directly managing all the Update and queued events for the Player only in this class.
 */
 [RequireComponent(typeof(PBaseMovement))]
 [RequireComponent(typeof(PCollisionState))]
public class PBehaviourManager : MonoBehaviour {

    private PBehaviour curBehaviour;

    /* Behaviours: */
    private PBehaviour behaviour_Hobbling;
    private PBehaviour behaviour_BaseMovement;
    private PBehaviour behaviour_ScytheBase;

    /* Collisionss: */
    private PCollisionState collisionState;

    public void Start()
    {
        /* Create Behaviours. */
        behaviour_BaseMovement = gameObject.GetComponent<PBaseMovement>();
        SetBehaviourSpecificVars();

        /* Set current Behaviour. */
        curBehaviour = behaviour_BaseMovement;

        /* Movement Components - Ensures that everything above is run before component creation. */
        collisionState = gameObject.GetComponent<PCollisionState>();
    }

    /* FixedUpdate for all Behaviours and States is only called in this Manager. */
    private void FixedUpdate()
    {
        curBehaviour.OnFixedUpdate();
    }

    /* Responsible for termination and allocation of new variables upon state switching.*/
    public void SwitchBehaviour(PBehaviour nextBehaviour)
    {

    }

    /* -- Methods For Readability. */
    private void SetBehaviourSpecificVars()
    {

    }
}
