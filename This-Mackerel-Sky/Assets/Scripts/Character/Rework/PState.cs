using System.Collections;
using System.Collections.Generic;


using UnityEngine;

public class PState : MonoBehaviour {
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
