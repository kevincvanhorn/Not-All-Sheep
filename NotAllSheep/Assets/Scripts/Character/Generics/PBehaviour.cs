using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PInputManager))]
[RequireComponent(typeof(PCollisionState))]
public class PBehaviour : MonoBehaviour {

    [HideInInspector]
    public PInputManager pInputManager;
    [HideInInspector]
    public List<PInput> inputFilter; // Inputs that this state accepts - any others should be discarded.
    [HideInInspector]
    public PCollisionState collisionState;
    [HideInInspector]
    public PState curState;
    [HideInInspector]
    public bool isTransitioning = false;

    /* Base Movement: */
    public Vector2 velocity = Vector2.zero;
    public sbyte directionFacing = 1; // @sybte of size -128 to 127
    public sbyte directionMoving = 1; // @sybte of size -128 to 127 

    /* Camera Variables: */
    public bool hasLateralInput;

    public virtual void Awake()
    {
        //pInputManager = GetComponent<PInputManager>();
        collisionState = GetComponent<PCollisionState>();
    }

    // Use this for initialization
    public virtual void OnStart()
    {
        
    }

    public virtual void OnFixedUpdate()
    {
        isTransitioning = false;
        curState.OnFixedUpdate();   
    }

    public virtual void Transition(PState nextState)
    {
        /* Only Allow one transition per Fixed Update.*/
        if (!isTransitioning)
        {
            isTransitioning = true;
            curState.OnStateExit();
            nextState.OnStateEnter();
            curState = nextState;
        }
    }

    /* Set Lateral Input Vars: directionFacing, directionMoving, hasLateralInput
     * NOTE: This should be called in the subclass, so that the state update order is maintained (ie. collisionstate before lateralinput).
     */
    public void UpdateLateralInputVars()
    {
        /* Update Direction Moving. */
        directionMoving = (velocity.x >= 0) ? (sbyte)1 : (sbyte)-1; // @sbyte an explicit cast. //((PBaseMovement_State)curState).

        /* Update Direction Facing. */
        if (pInputManager.KeyDown_Right)
        {
            directionFacing = 1;
        }
        else if (pInputManager.KeyDown_Left)
        {
            directionFacing = -1;
        }
        else if (pInputManager.KeyHeld_Left && !pInputManager.KeyHeld_Right)
        {
            directionFacing = -1;
        }
        else if (pInputManager.KeyHeld_Right && !pInputManager.KeyHeld_Left)
        {
            directionFacing = 1;
        }

        /* Check if there is Lateral input: For Camera Manager. */
        if (!pInputManager.KeyHeld_Right && !pInputManager.KeyHeld_Left)
        {
            hasLateralInput = false;
        }
        else
        {
            hasLateralInput = true;
        }

        /* Update Player Manager: */
        Player.directionFacing = directionFacing;
    }
}
