using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/* Event Listener for Player. */
public class EventListener : MonoBehaviour
{

    public List<EventRelay.EventMessageType> eventsHandled =
        new List<EventRelay.EventMessageType>();

    private CharacterBase player;
    CMomentum momentum;

    void OnEnable()
    {
        EventRelay.OnEventAction += HandleEvent;
    }

    private void Start()
    {
        player = GetComponent<CharacterBase>();
        momentum = GetComponent<CMomentum>();
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
            //Debug.LogError("1818181818");
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
        if (messageType == EventRelay.EventMessageType.MomentumTrigger)
        {
            if (sender.GetComponent<Checkpoint>() == true)
            {
                momentum.OnEventCheckpoint();
            }
        }
        /* Idle Events for Momentum Drain. Need to keep moving to maintain momentum. */
        else if(messageType == EventRelay.EventMessageType.CStateEnter)
        {
            CStatesBase state = player.fsm.State;
            if(state == CStatesBase.Idle)
            {
                //momentum.OnIdleEnter();
            }
        }
        else if (messageType == EventRelay.EventMessageType.CStateExit)
        {
            CStatesBase state = player.fsm.State;
            if (state == CStatesBase.Idle)
            {
                //momentum.OnIdleExit();
            }
        }
    }
}
