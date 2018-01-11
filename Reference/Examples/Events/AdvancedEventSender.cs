using UnityEngine;
using System.Collections;

public class AdvancedEventSender : MonoBehaviour {

	public bool mouseIsOverThis = false;

	// Update is called once per frame
	/*void Update () {
		if(mouseIsOverThis) {
			if(Input.GetMouseButtonDown((int)MouseUtils.Button.Left)) {
				string value = AdvancedEventRelay.RelayEvent(
					AdvancedEventRelay.EventMessageType.StructureBuilt, this);
				Debug.Log("StructureBuilt Event was seen by: " + value);
			}
			if(Input.GetMouseButtonDown((int)MouseUtils.Button.Right)) {
				string value = AdvancedEventRelay.RelayEvent(
					AdvancedEventRelay.EventMessageType.UnitDestroyed, this);
				Debug.Log("UnitDestroyed Event was seen by: " + value);
			}
		}
	}*/

	public void OnMouseEnter() {
		mouseIsOverThis = true;
	}

	public void OnMouseExit() {
		mouseIsOverThis = false;
	}
}
