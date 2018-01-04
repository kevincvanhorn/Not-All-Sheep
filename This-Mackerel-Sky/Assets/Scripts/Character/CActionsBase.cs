using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MonsterLove.StateMachine; // State-Machine Package.

public class CActionsBase : MonoBehaviour {

    private StateMachine<CStatesActionsBase> fsm;

    /*Reference movement vars from characterBase. */
    private CharacterBase characterBase; // 1.4.18 - character Base should be a virtual class so that the three forms can each overwrite and add to it.


    /* Define States */
    public enum CStatesActionsBase
    {
        Waiting,
        FindState,
        Dash
    }

    private void Awake()
    {
        // Initialize State Machine Engine		
        fsm = StateMachine<CStatesActionsBase>.Initialize(this, CStatesActionsBase.Waiting);
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
