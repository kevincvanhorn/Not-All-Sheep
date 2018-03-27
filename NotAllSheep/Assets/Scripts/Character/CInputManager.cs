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

public class ButtonState : MonoBehaviour
{
    public float waitTime;
    public float value = 0;
    public float timeHeld = 0;
    public float timeSinceAction;

    public bool isWaiting = false;

    public void Start()
    {
        timeSinceAction = waitTime;
    }

    /*public ButtonState(float waitTime)
    {
        this.waitTime = waitTime;
        timeSinceAction = waitTime;
    }*/

    public void StartWaiting()
    {
        if (!isWaiting)
        {
            isWaiting = true;
            StartCoroutine(OnPress());
        }
    }

    IEnumerator OnPress()
    {
        for (float f = 0; f <= waitTime; f += 0.1f)
        {
            timeSinceAction = f;
            yield return new WaitForSeconds(0.1f);
        }
        isWaiting = false;
        yield return null;
    }
}

public class CInputManager : MonoBehaviour {

    //private Dictionary<Buttons, ButtonState> buttonStates = new Dictionary<Buttons, ButtonState>();

    ButtonState dash;

    public void Awake()
    {
        dash = gameObject.AddComponent<ButtonState>() as ButtonState;

        dash.waitTime = 0.5f;

        //buttonStates.Add(Buttons.Dash, new ButtonState(3));
        //dash = buttonStates[Buttons.Dash];
    }

    /* Contains all of the keys that differ from normal movement to trigger the action state change in CharacterBase. */
    public bool ActionKeyPressed() {
        if (Input.GetKey(KeyCode.LeftControl)) {
            if (!dash.isWaiting)
            {
                dash.StartWaiting();
                return true;
            }
        }
        return false;
    }
}
