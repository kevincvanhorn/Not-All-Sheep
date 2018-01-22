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

public class MomentumState : MonoBehaviour
{
    float drainDelayTime;
    float maxStateMomentum;
    //bool canTransition = false; // cannot transition when locked.
    float increaseRate;

    private bool isStateActive = false;
    private bool isWaiting = true; // No new calculations will occur when the system is waiting.
    private bool canTransition = false; // True when the system can go to the next level up.

    public delegate void OnTransition();
    public event OnTransition onTransition;

    public void Init(float maxMomentum, float drainDelayTime, float increaseRate)
    {
        this.drainDelayTime = drainDelayTime;
        this.increaseRate = increaseRate;
        this.maxStateMomentum = maxMomentum;
    }

    public void EnterState()
    {
        Debug.LogError("ENTER STATE");
        isStateActive = true;
        StartCoroutine(WaitForDrainDelay());
        StartCoroutine(MomentumUpdate());
    }

    // A delay occurs when an event occurs and draining should start, but a delay has to finish first: gives room for error.
    IEnumerator WaitForDrainDelay()
    {
        isWaiting = true;
        yield return new WaitForSeconds(drainDelayTime);
        isWaiting = false;
        Debug.LogError("Done WaitFOrDrainDelay");
    }

    IEnumerator MomentumUpdate()
    {
        Debug.LogError("MomentumUpdate - Enter");
        while (isStateActive)
        {
            Debug.LogError("MomentumUpdate " + isStateActive + " waiting " + isWaiting + "max" + maxStateMomentum);
            if (!isWaiting)
            {
                if (MomentumGlobals.curMomentum < maxStateMomentum)
                {
                    MomentumGlobals.curMomentum += increaseRate * Time.deltaTime; // 2 should be increaseRate
                }
                else if (MomentumGlobals.curMomentum >= maxStateMomentum)
                {
                    /* Transition Event */ 
                    if (onTransition != null)
                    {
                        isStateActive = false;
                        onTransition();// Transition Event
                        ClearAllCoroutines();
                    }
                }
            }
            yield return new WaitForEndOfFrame();
        }
    }

    /*IEnumerator IncreaseMomentum()
    {
        while (!canTransition)
        {
            while (MomentumGlobals.curMomentum < maxStateMomentum)
            {
                MomentumGlobals.curMomentum += increaseRate; // 2 should be increaseRate
                yield return new WaitForSeconds(0.1f);
            }
            if (MomentumGlobals.curMomentum >= maxStateMomentum)
            {
                if (onTransition != null)
                {
                    onTransition();// Transition Event
                }
            }
        }
        
    }*/

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
        isWaiting = false;
    }
}

public class CMomentum : MonoBehaviour {

    /*
    What breaks momemtum?
    - collision with an enemy when not attacking 

    What drains momentum?
    - staying at vel = 0
    */

    private bool breakMomentum = false; // Occurs on an event
    public float drainDelayTime = 0;
    public int curState = 0;
    //private int numStates = 4;

    public MomentumState[] momentumStates = new MomentumState[3];


    void Start()
    {
        //starts 20
        MomentumGlobals.curMomentumLevel = 20;

        /* Create state components. */
        for (int e = 0; e < momentumStates.Length; e++)
        {
            momentumStates[e] = gameObject.AddComponent<MomentumState>() as MomentumState; ;
        }

        MomentumGlobals.curMomentum = 20;

        momentumStates[0].Init(0, 4, 2); // Only Waiting State. Should be 20 as base. // TODO:  next: should only increase when grounded
        momentumStates[1].Init(40, drainDelayTime, 1);
        momentumStates[2].Init(60, drainDelayTime, 2);

        /* Listen for state events: */
        for (int i = 0; i < momentumStates.Length; i++)
        {
            momentumStates[i].onTransition += OnStateTransition;
        }

        // Begin active state at first state.
        momentumStates[0].EnterState();
    }

    private void Update()
    {
        Debug.LogError(MomentumGlobals.curMomentum);
    }

    public void OnStateTransition()
    {
        Debug.LogError("ON STATE TRANSITION");
        if(curState+1 < momentumStates.Length)
        {
            // Set and activate next state.
            curState++;
            momentumStates[curState].EnterState();
        }
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

    public void OnDestroy()
    {
        // To Prevent Memory leaks:
        for (int i = 0; i < momentumStates.Length; i++)
        {
            momentumStates[i].onTransition -= OnStateTransition;
        }
    }
}