using UnityEngine;
using System.Collections;


/* Send Events from any script w/o having to attach an additional sender script. */
public class AdvancedEventRelay : MonoBehaviour {

	public delegate string EventAction(EventMessageType type, MonoBehaviour sender);
	public static event EventAction OnEventAction;

	public enum EventMessageType {
		StructureDamage,
		StructureBuilt,
		StructureDestroyed,
		UnitDamage,
		UnitBuilt,
		UnitDestroyed
	}

	/*public static string RelayEvent(EventMessageType messageType, MonoBehaviour sender) {
		return OnEventAction(messageType, sender);
	}*/
}
