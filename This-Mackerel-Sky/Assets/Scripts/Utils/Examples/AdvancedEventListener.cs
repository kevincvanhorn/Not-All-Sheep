using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AdvancedEventListener : MonoBehaviour {

	public List<AdvancedEventRelay.EventMessageType> eventsHandled =
		new List<AdvancedEventRelay.EventMessageType>();

	void OnEnable() {
		AdvancedEventRelay.OnEventAction += HandleEvent;
	}

	void OnDisable() {
		AdvancedEventRelay.OnEventAction -= HandleEvent;
	}

	//This method matches the signature of:
	//public delegate string EventAction(EventMessageType type, MonoBehaviour sender);
	//This means we can add it to the OnEventAction

	string HandleEvent(AdvancedEventRelay.EventMessageType messageType, MonoBehaviour sender) {
		if(eventsHandled.Contains(messageType)) {
		Debug.Log("Handled event: " + messageType + " from sender: " + sender
			          + " " + Vector3.Distance(this.transform.position, sender.transform.position)
			          + " units away from me");
		return this.ToString();
		} else {
			//ignore event
			return this.ToString();
		}
	}
}
