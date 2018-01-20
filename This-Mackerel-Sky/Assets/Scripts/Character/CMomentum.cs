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

    public void Init(float maxMomentum, float drainDelayTime)
    {
        this.drainDelayTime = drainDelayTime;
    }

    IEnumerator WaitForDelay()
    {
        yield return null;
    }

    IEnumerator IncreaseMomentum()
    {
        
        if(!lockedState && MomentumGlobals.curMomentum == maxStateMomentum)
        {
            // Fire Transition Event
        }
        yield return null;
    }

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
    public float curMomentum = 0;
    public float maxMomentum;
    public float drainDelayTime;

    // Use this for initialization
    void Start () {
        momentumLevels = new MomentumLevel[3]; // [5]
        momentumLevels[0].Init(0, 0);
        momentumLevels[1].Init(maxMomentum, drainDelayTime);
        momentumLevels[2].Init(maxMomentum, drainDelayTime);
    }

    public void OnBreakMomentum()
    {
        breakMomentum = true;
    }

    IEnumerator CalcActiveSpeed()
    {

        yield return null;
    }
}
