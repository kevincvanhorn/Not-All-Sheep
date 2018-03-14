using System.Collections;
using System.Collections.Generic;


using UnityEngine;

public class PState : MonoBehaviour {
    // 3.14.18 Had Issue with polymorphism, but this variable into the local states.
    //public PBehaviour behaviour; // The parent behaviour of the state - contains all of the shared variables for states in a behaviour.

    public virtual void OnStateEnter()
    {
        Debug.Log(this.GetType()+" - Enter");
    }

    public virtual void OnStateUpdate()
    {
        Debug.Log(this.GetType() + " - OnStateUpdate");
    }

    public virtual void OnInterrupt()
    {

    }

    public virtual void OnStateExit()
    {

    }
}
