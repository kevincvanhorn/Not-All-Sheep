using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MonsterLove.StateMachine; // State-Machine Package.

public class CActionsBase : MonoBehaviour {

    public StateMachine<CStatesActionsBase> fsm;

    /*Reference movement vars from characterBase. */
    private CharacterBase character; // 1.4.18 - character Base should be a virtual class so that the three forms can each overwrite and add to it.


    private bool onInitialRun = true;

    /* Define States */
    public enum CStatesActionsBase
    {
        Waiting,
        FindState,
        Dash
    }


    /* 1. Halts other calculations and states by switching to this state. 
           2. Sets cActionsBase.fsm from Waiting to FindState where the respective actionState is calculated.
           3. When the action is carried out or interrupted, switches to Waiti
           // Must have control of this class's vars
           4. Action continues from previous state.*/
    private void Awake()
    {
        // Initialize State Machine Engine		
        fsm = StateMachine<CStatesActionsBase>.Initialize(this, CStatesActionsBase.Waiting);
        character = GetComponent<CharacterBase>();
        Debug.Log("Action Base Awake");
    }

    /* Precondition: Action is interrupted or completed and CharacterBase is switched out of Action State back to 
     *      previous or appropriate state. 
     * Postcondition: All calculations of Action are finished.
     */
    IEnumerator Waiting_Enter()
    {
        Debug.Log("CActionsBase: WAITING - Enter.");
        yield return new WaitForEndOfFrame(); // Wait so all calculations of this state are finished then switch over.
                                              // Dont switch states on Awake of this whoel class
        if (!onInitialRun) { 
            character.fsm.ChangeState(character.fsm.LastState, StateTransition.Safe);
        }
    }

    public void Waiting_Update()
    {
        Debug.Log("CActionsBase - WAITING - Update. ");
    }

    public void Waiting_Finally()
    {
        onInitialRun = false;
    }

    /* Started when an Action is triggered.
     * Precondition: CharacterBase: All Movement States are suspended, CActionsBase just came out of Waiting State.*/
    IEnumerator FindState_Enter()
    {
        Debug.Log("CActionsBase: FINDSTATE - Enter.");
        //yield return new WaitForEndOfFrame(); // State Update doesn't start until end of Frame (? Before or after Update Main?).
        yield return null;

        if (Input.GetKey(KeyCode.LeftControl)) // TODO: Change to a pulse set in CInputManager, set false in ActionBase when finished.
        {
            fsm.ChangeState(CStatesActionsBase.Dash, StateTransition.Safe);
        }
    }

    public void FindState_Update()
    {
        Debug.Log("CActionsBase: FINDSTATE - Update.");

    }

    IEnumerator Dash_Enter()
    {
        Debug.LogError("CActionsBase: DASH - Enter.");
        yield return new WaitForEndOfFrame(); // State Update doesn't start until end of Frame (? Before or after Update Main?). 
        Vector2 velPrev = character.velocity;
        character.velocity.y = 0;
        character.velocity.x = character.activeSpeed * 4f * character.directionFacing;
        yield return new WaitForSeconds(.1f);
        character.velocity.x = velPrev.x;
        fsm.ChangeState(CStatesActionsBase.Waiting, StateTransition.Safe);
    }

    public void Dash_Update()
    {

    }


    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
