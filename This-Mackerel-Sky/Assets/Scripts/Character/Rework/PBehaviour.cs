using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PBehaviour : MonoBehaviour {

    PInputManager pInputManager;
    List<PInput> inputFilter; // Inputs that this state accepts - any others should be discarded.

    PState curState;

    // Use this for initialization
    void Start()
    {
        pInputManager = GetComponent<PInputManager>();
    }

    public void OnFixedUpdate()
    {
        //pInputManager.FilterInput(inputFilter); // Filter input so that the input array in PInputManager 
        curState.OnStateUpdate();
        
    }
}
