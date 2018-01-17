using UnityEngine;
using System.Collections;

public class EventGUIListener : MonoBehaviour {

	public string messageToDisplay = "Event recieved!";
	private bool showMessage = false;
	public float secondsToShowMessage = 3f;

	//When this component is first enabled, we want to subscribe to events
	//from the eventbroadcaster.
	void OnEnable()
	{
		//We're adding our notification method `RespondToEvent` to the OnEventAction
		//now, whenever OnEventAction is called, our method RespondToEvent will also be called
		EventBroadcaster.OnEventAction += EventRecieved;
	}
	
	//When this component is disabled, we want to "unsubscribe" from events.
	void OnDisable()
	{
		//Now we're removing our notification method, no longer taking any action when OnEventAction is called
		EventBroadcaster.OnEventAction -= EventRecieved;
	}
	
	void OnGUI() {
		if(showMessage) {
			GUI.Label(new Rect(100,100,300,20), messageToDisplay);
		}
	}

	//This method matches the signature of
	//public delegate void EventAction();
	//Returns void and takes no arguments
	// This means we can add it to the OnEventAction event
	void EventRecieved() {
		ShowTimedMessage();
	}
	 
	void ShowTimedMessage() {
		StartCoroutine(TimedMessage());
	}

	IEnumerator TimedMessage() {
		showMessage = true;
		yield return new WaitForSeconds(secondsToShowMessage);
		showMessage = false;
	}

}
