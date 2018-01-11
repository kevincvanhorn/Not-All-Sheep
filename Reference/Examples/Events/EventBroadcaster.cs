using UnityEngine;
using System.Collections;

public class EventBroadcaster : MonoBehaviour {

	//A delegate is a type that allows you to create a method like a parameter
	//essentially it allows you to then create an instance of the delegate you can
	//pass around and call as if it were a regular method.
	//For this situation we're going to use it to attach other methods to
	//it will be the method that's called and thus calls the methods of our listeners
	public delegate void EventAction();

	//Static means that this EventAction is going to be accessable without needing to instantiate
	//an instance of this object. 
	//The event keyword is also key here. It allows us to notify the listener classes when the 
	//event action method is called.
	public static event EventAction OnEventAction;

	void OnMouseDown() {
		//When this object is clicked, it will activate its event
		//this will then notify all listeners that the event has happened.
		OnEventAction();
	}
}
