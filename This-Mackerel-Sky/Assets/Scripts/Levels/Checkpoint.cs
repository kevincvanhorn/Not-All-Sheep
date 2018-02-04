using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour {

    public float maxActivateSpeed;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        
        if (MomentumGlobals.CurMomentum < maxActivateSpeed && collision.gameObject.GetComponent<CharacterBase>())
        {
            string value = EventRelay.RelayEvent(EventRelay.EventMessageType.MomentumTrigger, this);
            Debug.LogWarning("MomentumTrigger Event was seen by: " + value);
        }
    }
}
