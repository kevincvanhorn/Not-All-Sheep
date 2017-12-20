using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MonsterLove.StateMachine; // State-Machine Package.

public class CharacterBase : MonoBehaviour {


    /* Define States */
    public enum States {
        Idle,
        Jumping,
        Falling,
        WallRising,
        WallFalling,
        WallSticking,
        Sprinting,
        Dashing,
        ClimbingSlope
    }

    private StateMachine<States> fsm;

    public void Awake() {
        // Initialize State Machine Engine		
        fsm = StateMachine<States>.Initialize(this, States.Falling);
    }
	
	// Update is called once per frame
	void Update() {
        CalcState();
	}

    void CalcState() {
        var state = fsm.State;
        if(state == States.Idle) {

        }
    }

    void Idle_Enter() {

    }

    void Idle_Update() {
        if () {

        }
    }

    void Idle_Exit() {

    }

    void Idle_Finally() {
        // Reset object to desired configuration
        // For Overwrite: fsm.ChangeState(States.MyNextState, StateTransition.Overwrite);
    }
}
