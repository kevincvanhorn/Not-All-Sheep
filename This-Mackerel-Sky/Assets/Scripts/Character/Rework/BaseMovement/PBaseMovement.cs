using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PBaseMovement : PBehaviour {

    //Inherited:  PInputManager pInputManager
    //Inherited: PState curState

    public override void Start()
    {
        inputFilter = new List<PInput>() { PInput.Vertical, PInput.Horizontal, PInput.Up, PInput.Down, PInput.Left, PInput.Right, PInput.Sprint, PInput.Dash };
        base.Start(); // Creates Input Manager.

        curState = GetComponent<PBaseMovement_Airborne>();
    }

    public override void OnFixedUpdate()
    {
        base.OnFixedUpdate(); // Runs OnStateUpdate for the current State.
    }
}