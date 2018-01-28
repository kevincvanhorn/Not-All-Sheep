using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MonsterLove.StateMachine; // State-Machine Package.

public class E1_01FollowBot : EnemyBase {

    private StateMachine<E1_01FollowBotStates> fsm;

    private enum E1_01FollowBotStates
    {
        Idle,
        Fleeing,
        Interest
    }

	// Update is called once per frame
	void Update () {
		
	}

    void Idle_OnEnter()
    {

    }

    void Idle_Update()
    {

    }
}
