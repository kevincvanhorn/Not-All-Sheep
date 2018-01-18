using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CMomentum : MonoBehaviour {

    /*
    What breaks momemtum?
    - collision with an enemy when not attacking 

    What drains momentum?
    - staying at vel = 0
    */
    private bool breakMomentumLevel = false; // Occurs on an event

    struct MomemtumLevel{
        float 
        float drainDelayTime;
        
    }

	// Use this for initialization
	void Start () {
	    	
	}

    IEnumerator CalcActiveSpeed()
    {

        yield return null;
    }

    IEnumerator OnWaitingAtZero()
    {
        /*for (float f = 0; f <= waitTime; f += 0.1f)
        {
            timeSinceAction = f;
            yield return new WaitForSeconds(0.1f);
        }
        isWaiting = false;
        yield return null;*/
    }
}
