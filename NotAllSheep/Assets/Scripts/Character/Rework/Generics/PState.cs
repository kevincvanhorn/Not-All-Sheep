using System.Collections;
using System.Collections.Generic;


using UnityEngine;

public class PState {
    // 3.14.18 Had Issue with polymorphism, but this variable into the local states.
    //public PBehaviour behaviour; // The parent behaviour of the state - contains all of the shared variables for states in a behaviour.

    public virtual void OnStateEnter()
    {
        //Debug.Log(this.GetType()+" - Enter"); //@tag: DEBUG
    }

    public virtual void OnFixedUpdate()
    {
        //Debug.Log(this.GetType() + " - OnStateUpdate"); //@tag: DEBUG
    }

    public virtual void OnInterrupt()
    {

    }

    /* Called prior to state transition, should not modify velocity. */
    public virtual void OnStateExit()
    {
        //Debug.Log(this.GetType() + " - OnStateExit"); //@tag: DEBUG
    }
}
