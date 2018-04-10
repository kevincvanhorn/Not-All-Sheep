using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PInputManager))]
[RequireComponent(typeof(PCollisionState))]
public class PBehaviour : MonoBehaviour {

    public PInputManager pInputManager;
    public List<PInput> inputFilter; // Inputs that this state accepts - any others should be discarded.
    public PCollisionState collisionState;

    public PState curState;

    public bool isTransitioning = false;

    public virtual void Awake()
    {
        pInputManager = GetComponent<PInputManager>();
        collisionState = GetComponent<PCollisionState>();
    }

    // Use this for initialization
    public virtual void Start()
    {
        
    }

    public virtual void OnFixedUpdate()
    {
        isTransitioning = false;
        curState.OnFixedUpdate();   
    }

    public virtual void Transition(PState nextState)
    {
        /* Only Allow one transition per Fixed Update.*/
        if (!isTransitioning)
        {
            isTransitioning = true;
            curState.OnStateExit();
            nextState.OnStateEnter();
            curState = nextState;
        }
    }
}
