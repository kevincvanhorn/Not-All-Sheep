using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PInput
{
    Vertical,
    Horizontal,
    Up,
    Down,
    Left,
    Right,
    Sprint,
    Dash
}

/*  Need to store keyDown events until consumed in next fixed update (uuuf) ie. held down until addressed in fixedUpdate.
    Need to store keyHeld events until next Update (ufffu) ie. held through fixed until next update.

    |   |   |   |   |   |   |
      u       u  uuu  u   u
 */
public class PInputManager : MonoBehaviour
{
    private HashSet<PInput> keyDownInputs;
    private HashSet<PInput> keyHeldInputs;

    /* Accessor variables. */
    public bool KeyDown_Up { get { return keyDownInputs.Contains(PInput.Up); } }
    public bool KeyDown_Down { get { return keyDownInputs.Contains(PInput.Down); } }
    public bool KeyDown_Left { get { return keyDownInputs.Contains(PInput.Left); } }
    public bool KeyDown_Right { get { return keyDownInputs.Contains(PInput.Right); } }
    public bool KeyDown_Sprint { get { return keyDownInputs.Contains(PInput.Sprint); } }
    public bool KeyDown_Dash { get { return keyDownInputs.Contains(PInput.Dash); } }

    public bool KeyHeld_Up { get { return keyHeldInputs.Contains(PInput.Up); } }
    public bool KeyHeld_Down { get { return keyHeldInputs.Contains(PInput.Down); } }
    public bool KeyHeld_Left { get { return keyHeldInputs.Contains(PInput.Left); } }
    public bool KeyHeld_Right { get { return keyHeldInputs.Contains(PInput.Right); } }
    public bool KeyHeld_Sprint { get { return keyHeldInputs.Contains(PInput.Sprint); } }
    public bool KeyHeld_Dash { get { return keyHeldInputs.Contains(PInput.Dash); } }

    public void Awake()
    {
        keyDownInputs = new HashSet<PInput>();
        keyHeldInputs = new HashSet<PInput>();
    }

    /* Reset keyDown Inputs every fixed update. */
    public void OnFixedUpdate()
    {
        /* Consume input in keyDownInputs. */
        keyDownInputs.Clear();
    }

    private void Update()
    {
        /* Populate Input Queues: */
        QueueKeyDownInputs();
        QueueKeyHeldInputs();
        keyHeldInputs.Clear(); // Clear held inputs every Update.
    }

    /* Populates the keyDownInputs to be reset each fixedUpdate (in current behaviour).
 * TODO: Make buttons with "string" mapping instead of GetKeyDown(). */
    private void QueueKeyDownInputs()
    {
        // Populate inputQueue
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            keyDownInputs.Add(PInput.Up);
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            keyDownInputs.Add(PInput.Down);
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            keyDownInputs.Add(PInput.Left);
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            keyDownInputs.Add(PInput.Down);
        }
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            keyDownInputs.Add(PInput.Sprint);
        }
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            keyDownInputs.Add(PInput.Dash);
        }
    }

    /* Populates the keyHeldInputs to be reset each Update (in this Input Manager).
     * TODO: Make buttons with "string" mapping instead of GetKey(). */
    private void QueueKeyHeldInputs()
    {
        // Populate inputQueue
        if (Input.GetKey(KeyCode.UpArrow))
        {
            keyHeldInputs.Add(PInput.Up);
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            keyHeldInputs.Add(PInput.Down);
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            keyHeldInputs.Add(PInput.Left);
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            keyHeldInputs.Add(PInput.Down);
        }
        if (Input.GetKey(KeyCode.LeftShift))
        {
            keyHeldInputs.Add(PInput.Sprint);
        }
        if (Input.GetKey(KeyCode.LeftControl))
        {
            keyHeldInputs.Add(PInput.Dash);
        }
    }

}