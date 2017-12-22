using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*public enum Buttons {
    Right,
    Left,
    Up,
    Down,
    A,
    B,
    X,
    Y
}*/

/* Specifies what value comparision triggers this button. */
public enum Condition {
    GreaterThan,
    LessThan
}

/* The state value of an axis from Unity's input manager*/
[System.Serializable]
public class InputAxisState {
    public string axisName;
    public float offValue;
    public Buttons button;
    public Condition condition;

    public bool value {

        get {
            float val = Input.GetAxis(axisName);

            switch (condition) {
                case Condition.GreaterThan:
                    return val > offValue;
                case Condition.LessThan:
                    return val < offValue;
            }
            return false;
        }

    }
}

public class ButtonState {
    public bool pressed;
    public float holdTime = 0;
}

public class CInputState : MonoBehaviour {

    private Dictionary<Buttons, ButtonState> buttonStates = new Dictionary<Buttons, ButtonState>();

    private InputAxisState[] inputs = new InputAxisState[(Buttons.GetNames(typeof(Buttons)).Length)];

    private void Awake() {

        inputs[(int)Buttons.Up].axisName = "Right";
        inputs[(int)Buttons.Up].offValue = 0;
        inputs[(int)Buttons.Up].button = Buttons.Right;
        inputs[(int)Buttons.Up].condition = Condition.GreaterThan;

        inputs[(int)Buttons.Up].axisName = "Left";
        inputs[(int)Buttons.Up].offValue = 0;
        inputs[(int)Buttons.Up].button = Buttons.Up;
        inputs[(int)Buttons.Up].condition = Condition.LessThan;

        inputs[(int)Buttons.Up].axisName = "Up";
        inputs[(int)Buttons.Up].offValue = 0;
        inputs[(int)Buttons.Up].button = Buttons.Up;
        inputs[(int)Buttons.Up].condition = Condition.GreaterThan;

    }

    void UpdateInputs() {
        foreach (InputAxisState input in inputs) {
            SetButtonPressed(input.button, input.value); // Sets inputState true if button pressed.
        }
    }

    public void SetButtonPressed(Buttons key, bool value) {
        if (!buttonStates.ContainsKey(key))
            buttonStates.Add(key, new ButtonState());

        var state = buttonStates[key];

        if (state.pressed && !value) {
            state.holdTime = 0; // When key is released.
        }
        else if (state.pressed && value) {
            state.holdTime += Time.deltaTime; // When key is held.
        }

        state.pressed = value;

    }

    public bool GetButton(Buttons key) {
        if (buttonStates.ContainsKey(key))
            return buttonStates[key].pressed;
        else
            return false;
    }

    public float GetButtonHoldTime(Buttons key) {
        if (buttonStates.ContainsKey(key))
            return buttonStates[key].holdTime;
        else
            return 0;
    }

}
