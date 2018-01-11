using UnityEngine;
using System.Collections;

public class EventMoveListener : MonoBehaviour {

	public Vector3 eventMoveDirection = new Vector3(0,1,0);

	//When this component is first enabled, we want to subscribe to events
	//from the eventbroadcaster.
	void OnEnable()
	{
		//We're adding our notification method `RespondToEvent` to the OnEventAction
		//now, whenever OnEventAction is called, our method RespondToEvent will also be called
		EventBroadcaster.OnEventAction += RespondToEvent;
	}
	
	//When this component is disabled, we want to "unsubscribe" from events.
	void OnDisable()
	{
		//Now we're removing our notification method, no longer taking any action when OnEventAction is called
		EventBroadcaster.OnEventAction -= RespondToEvent;
	}

	//Our RespondToEvent method is used to respond to events 
	//In this case, we'll just move ourselves to show that we've responded.
	void RespondToEvent() {
		this.transform.Translate(eventMoveDirection);
	}
}
