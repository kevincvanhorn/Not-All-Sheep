using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EventListener : MonoBehaviour
{

    public List<EventRelay.EventMessageType> eventsHandled =
        new List<EventRelay.EventMessageType>();

    void OnEnable()
    {
        EventRelay.OnEventAction += HandleEvent;
    }

    void OnDisable()
    {
        EventRelay.OnEventAction -= HandleEvent;
    }

    //This method matches the signature of:
    //public delegate string EventAction(EventMessageType type, MonoBehaviour sender);
    //This means we can add it to the OnEventAction

    string HandleEvent(EventRelay.EventMessageType messageType, MonoBehaviour sender)
    {
        if (eventsHandled.Contains(messageType))
        {
            Debug.LogWarning("Handled event: " + messageType + " from sender: " + sender
                          + " " + Vector3.Distance(this.transform.position, sender.transform.position)
                          + " units away from me");
            CallRespectiveEvents(messageType, sender);
            return this.ToString();
        }
        else
        {
            //ignore event
            return this.ToString();
        }
    }

    private void CallRespectiveEvents(EventRelay.EventMessageType messageType, MonoBehaviour sender)
    {
        if(messageType == EventRelay.EventMessageType.MomentumTrigger)
        {
            CMomentum momentum = GetComponent<CMomentum>();
            if (sender.GetComponent<Checkpoint>() == true)
            {
                momentum.OnEventCheckpoint();
            }
        }
    }
}
