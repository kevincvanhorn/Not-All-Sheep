using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Buttons {
    Vertical,
    Horizontal,
    Up,
    Down,
    Left,
    Right,
    Sprint,
    Dash
}

public class CInputManager : MonoBehaviour {

    public bool ActionKeyPressed() {
        if (Input.GetKey(KeyCode.LeftShift)) {
            return true;
        }
        return false;
    }



}
