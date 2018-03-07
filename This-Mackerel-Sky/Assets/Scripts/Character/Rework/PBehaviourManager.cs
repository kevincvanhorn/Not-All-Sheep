using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PBehaviourManager : MonoBehaviour {

    private PBehaviour curBehaviour;

    private PBehaviour Hobbling;
    private PBehaviour BaseMovement;
    private PBehaviour ScytheBehaviour;

    public void Start()
    {
        //BaseMovement = GetComponent<BaseMovement>();

        curBehaviour = BaseMovement;
    }

    public void SwitchBehaviour(PBehaviour nextBehaviour)
    {

    }

    private void FixedUpdate()
    {
        curBehaviour.OnFixedUpdate();

    }
}
