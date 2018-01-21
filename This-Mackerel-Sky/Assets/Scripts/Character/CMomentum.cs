using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* The live vars shared between individual momentem levels and CMomentum class. 
 - can probably combine all 3 of these into 1. */
public class MomentumGlobals
{
    public static float curMomentum;
    public static float curMomentumLevel;
}

public class MomentumLevel : MonoBehaviour
{
    float drainDelayTime;
    float maxStateMomentum;
    bool lockedState = false; // cannot transition when locked.
    float increaseRate;

    public void Init(float maxMomentum, float drainDelayTime, float increaseRate)
    {
        this.drainDelayTime = drainDelayTime;
        this.increaseRate = increaseRate;
    }

    public void EnterState()
    {
        StartCoroutine(WaitForDrainDelay());
        StartCoroutine(IncreaseMomentum());
        
    }

    // A delay occurs when an event occurs and draining should start, but a delay has to finish first: gives room for error.
    IEnumerator WaitForDrainDelay()
    {
        yield return new WaitForSeconds(drainDelayTime);
    }

    IEnumerator IncreaseMomentum()
    {
        
        while(!lockedState && MomentumGlobals.curMomentum == maxStateMomentum)
        {
            MomentumGlobals.curMomentum += increaseRate; // 2 should be increaseRate
            yield return new WaitForSeconds(0.1f);
        }
    }

    // ------------------ More complicated procedures:
    public void OnDrainEvent()
    {

    }

    public void SetMaxMomentum()
    {
        MomentumGlobals.curMomentum = maxStateMomentum;
    }

    public void ClearAllCoroutines()
    {
        
    }
}

public class CMomentum : MonoBehaviour {

    /*
    What breaks momemtum?
    - collision with an enemy when not attacking 

    What drains momentum?
    - staying at vel = 0
    */
    public MomentumLevel[] momentumLevels;
    private bool breakMomentum = false; // Occurs on an event
    public float drainDelayTime = 0;

    // Use this for initialization
    void Start () {
        momentumLevels = new MomentumLevel[4]; // [5]
        //starts 20
        MomentumGlobals.curMomentumLevel = 20;
        momentumLevels[0].Init(0,2,2); // Only Waiting State.
        momentumLevels[1].Init(20, drainDelayTime, 1);
        momentumLevels[2].Init(40, drainDelayTime, 2);
        momentumLevels[3].Init(60, drainDelayTime, 2);

        momentumLevels[0].EnterState();
    }

    public void OnBreakMomentum()
    {
        breakMomentum = true;
    }

    IEnumerator CalcActiveSpeed()
    {
        while (!breakMomentum)
        {

        }
        yield return null;
    }
}
