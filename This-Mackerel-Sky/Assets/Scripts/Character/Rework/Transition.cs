using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Transition{

    public Transition()
    {

    }

    private void OnTransition(PState nextState)
    {
        nextState.OnStateEnter();

    }
}
