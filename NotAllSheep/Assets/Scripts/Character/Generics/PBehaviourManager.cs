using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Responsible for executing and managing all behaviour configurations of the Player. 
 * The intent is to remove any uncertainty of execution order by directly managing all the Update and queued events for the Player only in this class.
 */
 [RequireComponent(typeof(PBaseMovement))]
 [RequireComponent(typeof(PCollisionState))]
public class PBehaviourManager : MonoBehaviour {

    /* Singleton Behaviour. */
    private static PBehaviourManager _instance = null;
    public static PBehaviourManager Instance { get { return _instance; } }

    public PBehaviour curBehaviour;

    /* Behaviours: */
    private PBehaviour behaviour_Hobbling;
    private PBehaviour behaviour_BaseMovement;
    private PBehaviour behaviour_ScytheBase;

    /* Collisions: */
    private PCollisionState collisionState;

    
    private void Awake()
    {
        /* Enforce Singleton: */
        if (_instance != null && _instance != this)
            Destroy(this.gameObject);
        else
            _instance = this;
    }

    /* Enforce different instance each scene. */
    private void OnDestroy() { if (this == _instance) { _instance = null; } }

    public void OnStart()
    {
        /* Create Behaviours. */
        behaviour_BaseMovement = gameObject.GetComponent<PBaseMovement>();
        behaviour_ScytheBase = gameObject.GetComponent<PScytheMovement>();
        SetBehaviourSpecificVars();

        /* Set current Behaviour. */
        curBehaviour = behaviour_ScytheBase;//behaviour_BasesMovement; // 6.1.18

        /* Movement Components - Ensures that everything above is run before component creation. */
        collisionState = gameObject.GetComponent<PCollisionState>();
    }

    /* FixedUpdate for all Behaviours and States is only called in this Manager. */
    public void OnFixedUpdate()
    {
        
        curBehaviour.OnFixedUpdate();
    }

    /* Responsible for termination and allocation of new variables upon state switching.*/
    public void SwitchBehaviour(PBehaviour nextBehaviour)
    {

    }

    /* -- Methods For Readability. */
    protected void SetBehaviourSpecificVars()
    {
        behaviour_BaseMovement.OnStart();
        behaviour_ScytheBase.OnStart();
    }
}
