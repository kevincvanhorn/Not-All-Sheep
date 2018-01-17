using UnityEngine;
using System.Collections;

public class AdvancedEventSender : MonoBehaviour {

	public bool mouseIsOverThis = false;

	// Update is called once per frame
	void Update () {
		if(mouseIsOverThis) {
			/*if(Input.GetMouseButtonDown((int)MouseUtils.Button.Left)) {
				string value = AdvancedEventRelay.RelayEvent(
					AdvancedEventRelay.EventMessageType.StructureBuilt, this);
				Debug.Log("StructureBuilt Event was seen by: " + value);
			}
			if(Input.GetMouseButtonDown((int)MouseUtils.Button.Right)) {
				string value = AdvancedEventRelay.RelayEvent(
					AdvancedEventRelay.EventMessageType.UnitDestroyed, this);
				Debug.Log("UnitDestroyed Event was seen by: " + value);
			}*/
		}
	}

	public void OnMouseEnter() {
		mouseIsOverThis = true;
	}

	public void OnMouseExit() {
		mouseIsOverThis = false;
	}
}

/*
In this video, I'm going to cover a common control structure in game event systems. Event systems are used to communicate between objects, systems, and game managers. These systems typically have events and listeners. The listeners will subscribe to the events, also called messages, of a certain type. When an event is broadcast of a certain type, its sent to each of the listeners for that type. What each listener does with that event depends on that event. Event message could be something like this building has been damaged.

Then we can have a gooey listener that will display a message on screen about the building being damaged or where it is, a worker listener that will try to repair the building. finally, prehaps the score listener that deduct points for letting your building get damaged. The primary benefit of these systems is flexibility. It's simple to have a object start listening for vent and have it respond appropriately without needing to touch the code that is generating the event. Let's look at a few more concrete examples with code. Here I have an EventBroadcaster that's going to create a delegate.

Each of the event listeners is going to create a method that matches the signature of this delegate where it returns void and takes no parameters. And here I'm going to create an instance of that EventAction, with the event keyword named OnEventAction. This is essentially a hub, where all event listeners can attach to. So that their event action delegates can be activated when this one is. In this case, in the on-mouse-down method, we're going to activate this method. And thus, send an event to all the listeners.

Let's take a look at a listener. Here we have a GUI listener. When the object this script is attached to is enabled it's going to attach itself to the on event action of the event broadcaster. It's going to attach its event received method, which is found here and we can see that it matches the delegate signature, where it returns void and takes no arguments. So when this method is activated here, we're also going to activate this listener method, and we're going to show a time message, which simply starts a coroutine, and toggles a boolean from true, waits for a couple seconds, and toggles it back to false.

And the ongoing message, we're going to wait for that boolean to turn true. If it is, we're going to show a label displaying the message to display. And if it's not, we're not going to display anything. Next we have an event move listener. Once again, when the object discript is attached too, is activated, it's going to attach itself to the on event action with its respond to event method. In this case the respond to event method is fairly simple. It's just going to translate the object it's attached to in the event move direction which is defined here.

So let's take a look at this in unity. Here I have an event broadcaster when this object is clicked, it's going to send an event. And here I have a few listeners. Here's an event move listener that's going to move down when activated, here's an event move listener that's going to move up when activated. And finally, a GUI event listener, which is going to display the message, event received, for three seconds, after it receives an event. So, let's activate the onMouseDown method, and see this in action. So, there we see the even received message, and our sphere's moved up or down depending on how they were defined in their event move direction.

Let's take a look at a slightly more advanced example. In this example, I'm using an event relay. This is an event relay class, so that we can send events from any script without having to attach an additional sender script. This will relay events between a sender a listeners. The delegate for this method is slightly different than the last. It's going to return a string. And it takes two arguments; a event message type and a sender, which is a mono behaviour. So this is the script. Let's take a look at the sender. In this case, when the mouse is over this object, if we click with the left mouse button we're going to relay an event.

Of event type StructureBuilt. We're going to send us as the sender. And then we're going to log the value returned from that event. Likewise, if the right mouse button is pressed, we're going to relay an even of type UnitDestroyed with ourselves as the sender and log the value of the returned string from that event. So let's take a look at a listener. Listeners are going to define a list of events that they handle, and they're going to attach themselves to the onEvent action of the advanced event relay when they are enabled.

And when handling an event, they check to see if this is an event type that they handle. If it is, they're going to log a message that says they handled this event from a sender, and then we're going to output this distance from the sender just to show that we can get some more information from this event system. Finally, they're going to return their name as the even handler, so let's see this in action. Again, I have an event broadcaster here, but it's actually just going to broadcast events through our event relay. And we have two event listeners, this guy is going to listen for structure built and unit destroyed, and this guy, we'll just have him listen to structure built.

So we can see now that left clicking is going to send us structure built. Which both of our event listeners listen for. The first guy is this one here, he says he's found an event from a center that's 6.6 meters away, and this guy here received an event that's 8 units away. On the return side, we see that the structure event was seen by event listener B. And that's because the last event listener to return an event is going to override any other previous event senders.

We can overcome this by adding a reference string to our parameters and adding ourselves to that reference string instead of a return string. Now let's see what a right click does. And we see that this event was only handled by one listener. And it's 6.6 meters away, this guy, event listener A. And we can see that it was still seen by event listener B. Because we're still going to return ourselves as a listener. This is useful for debugging, or if we don't want to return that we can just return an empty string.

In this video, we've covered event systems, their implementation, and common uses. Event systems are a great flexible way to communicate between objects without the objects even needing to know about each other in advance, or at all. It's important to note that there's not just one way to create event systems. There are dozens of ways to implement event systems and many different ways to use each implementation. However, event systems, in some form or another, are a very important aspect of game design. Without events, game objects come to rely heavily on each other, and changing one object can have a cascade effect throughout a number of objects.

Messaging systems help reduce special case coding, improve separation of game logic and user interface, and keep objects independent of each other.
*/
