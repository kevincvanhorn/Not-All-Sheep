using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Buttons {
    Right,
    Left,
    Up,
    Down
}

/* The state value of an axis from Unity's input manager*/
[System.Serializable] // Acessible to editor
public class InputAxisState {
    public string axisName;
    public float offValue; // Value of the axis state itself.
    public Buttons button;

    public bool value {
        get {
            var val = Input.GetAxis(axisName);
            if(val > offValue)
        }
    }
}

public class CInputManager : MonoBehaviour {

    public InputAxisState[] inputs;
    
    // Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
