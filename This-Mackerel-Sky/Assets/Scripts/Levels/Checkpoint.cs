using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour {


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<CharacterBase>())
        {
            string value = EventRelay.RelayEvent(EventRelay.EventMessageType.MomentumTrigger, this);
            Debug.LogWarning("MomentumTrigger Event was seen by: " + value);
        }
    }
}
