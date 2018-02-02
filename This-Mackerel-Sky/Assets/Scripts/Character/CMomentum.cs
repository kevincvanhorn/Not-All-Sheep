using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* The live vars shared between individual momentem levels and CMomentum class. 
 - can probably combine all 3 of these into 1. */
public static class MomentumGlobals // TODO: May need to check if this persists between levels.
{
    public static CharacterBase character = GameObject.FindObjectOfType<CharacterBase>(); // Needs to be set in the Editor.

    private static float curMomentum;
    public static float CurMomentum {
        get { return curMomentum; }
        set {
            curMomentum = value;
            //Debug.LogWarning("AHHHHHHHHHH: " + character.activeSpeed);
            if (character)
            {

                character.activeSpeed = curMomentum;
            }
        }
    }
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
        //Debug.LogError("ENTER STATE");
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
        //Debug.LogError("Done WaitFOrDrainDelay");
    }

    IEnumerator MomentumUpdate()
    {
        //Debug.LogError("MomentumUpdate - Enter");
        while (isStateActive)
        {
            //Debug.LogError("MomentumUpdate " + isStateActive + " waiting " + isWaiting + "max" + maxStateMomentum);
            if (!isWaiting)
            {
                if (MomentumGlobals.CurMomentum < maxStateMomentum)
                {
                    MomentumGlobals.CurMomentum += increaseRate * Time.deltaTime; // 2 should be increaseRate
                }
                else if (MomentumGlobals.CurMomentum >= maxStateMomentum)
                {
                    if(maxStateMomentum !=0) SetMaxMomentum();
                    /* Transition Event */
                    if (onTransition != null)
                    {
                        isStateActive = false;
                        onTransition();// Transition Event
                        ClearAllCoroutines();
                    }
                }
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    /*IEnumerator IncreaseMomentum()
    {
        while (!canTransition)
        {
            while (MomentumGlobals.CurMomentum < maxStateMomentum)
            {
                MomentumGlobals.CurMomentum += increaseRate; // 2 should be increaseRate
                yield return new WaitForSeconds(0.1f);
            }
            if (MomentumGlobals.CurMomentum >= maxStateMomentum)
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
        MomentumGlobals.CurMomentum = maxStateMomentum;
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
        MomentumGlobals.CurMomentum = 20;

        /* Create state components. */
        for (int e = 0; e < momentumStates.Length; e++)
        {
            momentumStates[e] = gameObject.AddComponent<MomentumState>() as MomentumState; ;
        }

        MomentumGlobals.CurMomentum = 20;

        momentumStates[0].Init(0, 0, 2); // Only Waiting State. Should be 20 as base. // TODO:  next: should only increase when grounded
        momentumStates[1].Init(40, drainDelayTime, 40);
        momentumStates[2].Init(60, drainDelayTime, 40);

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
        Debug.LogError(MomentumGlobals.CurMomentum);
        
    }

    public void OnStateTransition()
    {
        ///Debug.LogError("ON STATE TRANSITION");
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