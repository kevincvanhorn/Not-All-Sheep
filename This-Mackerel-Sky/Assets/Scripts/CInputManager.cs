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
    public bool OtherKeyPressed() {
        return false;
    }

}
