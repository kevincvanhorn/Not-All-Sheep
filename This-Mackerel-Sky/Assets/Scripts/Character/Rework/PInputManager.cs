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

public class PInputManager : MonoBehaviour
{
    
    public HashSet<PInput> curInputs;       // Local to the consuming controller (ex: Movement Behaviour).
    public HashSet<PInput> reservedInputs;  // For UI and foreign events.
    private Queue<PInput> inputQueue;       // 


    public void ConsumeInput(List<PInput> inputFilter)
    {
        FilterInput(inputFilter);
        /* Call Appropriate State actions.*/

        // Should maintain held until a release is buffered
    }

    private void Update()
    {
        // Populate inputQueue
        inputQueue.Enqueue(PInput.Vertical);
    }

    /* Remove duplicates, set lateral and vertical values */
    private void FilterInput(List<PInput> inputFilter)
    {
        /* Remove Duplicates, if not in inputFilter - remove */

        /* Set Lateral/Vertical values by controller or keyboard. */
        PInput curInput;
        while (inputQueue.Count > 0)
        {
            curInput = inputQueue.Dequeue();
        }
    }
}