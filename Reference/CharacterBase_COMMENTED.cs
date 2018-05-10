[RequireComponent(typeof(CCollisionState))]
[RequireComponent(typeof(CActionsBase))]
public class CharacterBase : MonoBehaviour
{
    /* Collisions Vars */
 

    private float jumpVelRatio; // Vxmax = (Vxmax * Vymin) / Vymin

    private CActionsBase cActionsBase;

    //public CCollisionState collisionState;
    //public HashSet<CollisionType> enterCollisionTypes = new HashSet<CollisionType>(); // For use in that frame. // Should be virtual
    //HashSet<CollisionType> collisionTypes; // For use in that frame.

    public StateMachine<CStatesBase> fsm;

    /* Private State-Specific Vars */
    // Steep Slopes
    private float steepSlopeSpeed;
    private Vector2 steepSlopeHitNormal;
    private float wallFrictionDown;

    private bool jumpWaiting = false;

    public bool hasLateralInput; // For camera smoothing.

    CAnimationController animController;

    public void Start()
    {
        /* Set collision defaults. */
        wallHitSpeed.x = activeSpeed;

        jumpVelRatio = jumpVelocityMin / jumpVelocityMax; // Vxmax = (Vxmax * Vymin) / Vymin

        wallFrictionDown = 1;
        wallStickTime = 0;

        animController = (CAnimationController)FindObjectOfType(typeof(CAnimationController));
    }
    void Idle_Enter()
    {
        string value = EventRelay.RelayEvent(EventRelay.EventMessageType.CStateEnter, this);
        Debug.Log("Enter Event was seen by: " + value);
    }
    void Idle_Exit()
    {
        string value = EventRelay.RelayEvent(EventRelay.EventMessageType.CStateExit, this);
        Debug.LogWarning("Exit Event was seen by: " + value);
    }   

    /* Simulate is for the 4-5 frames after a jump/transition away from an object into empty space occurs.
     Needed for the collision state to catch up so that actions like airborne checking if it's touching the floor
     in an update does not occur immediately at the first frame of up pressed out of the grounded state while the object is 
     still "grounded" by the bounding check.*/
    void Simulate_Update()
    {


        /* From Idle State. */
        else if (fsm.LastState == CStatesBase.Idle)
        {
            if (!collisionState.Bot)
            {
                fsm.ChangeState(CStatesBase.Airborne);
            }
            else if (!collisionState.Slope)
            {
                fsm.ChangeState(CStatesBase.Airborne);
            }
        }

        /* From Wall State. */
        else if (fsm.LastState == CStatesBase.OnWall)
        {
            if (collisionState.top)
            {
                //velocity.y = 0;
            }
            if (!collisionState.Left && !collisionState.Right)
            {
                fsm.ChangeState(CStatesBase.Airborne);
            }
            else if (collisionState.Bot)
            {
                fsm.ChangeState(CStatesBase.Idle);
            }
            else if (collisionState.TopSlope)
            {
                if (slopeAngle > CStats.wallAngleMax && slopeAngle < CStats.topAngleMin)
                {
                    topSlopeSpeedCur = velocity;
                    fsm.ChangeState(CStatesBase.TopSlope);
                }
                else { Debug.LogError("TopCollision - Invalid Angle"); }
            }
            else if (collisionState.Slope)
            {
                if (slopeAngle > CStats.slopeAngleMin && slopeAngle <= CStats.slopeAngleMax)
                {
                    fsm.ChangeState(CStatesBase.ClimbingSlope, StateTransition.Safe);
                }
                else { Debug.LogError("TopCollision - Invalid Angle"); }
            }
        }

        /* From Slope State. */
        else if (fsm.LastState == CStatesBase.ClimbingSlope)
        {
            if (!collisionState.Slope)
            { // TODO: Fix this b/c topslopes by adding slope angle aspect
                fsm.ChangeState(CStatesBase.Airborne);
            }
        }

        /* From Top Slope State. */
        else if (fsm.LastState == CStatesBase.TopSlope)
        {
            if (!collisionState.TopSlope)
            { //TODO - above ex. what if comeout of stop slope into a bot slope = stuck.
                if (collisionState.None)
                {
                    fsm.ChangeState(CStatesBase.Airborne);
                }
                else
                {
                    Debug.LogError("ERROR: Simulate - Top Slope - Can't transition to airborne. ");
                }
            }
            else if (collisionState.Bot || collisionState.Slope)
            {
                fsm.ChangeState(CStatesBase.Idle);
            }
        }

        else if (fsm.LastState == CStatesBase.SteepSlope)
        {
            if (!collisionState.SteepSlope)
            {
                if (collisionState.None)
                {
                    fsm.ChangeState(CStatesBase.Airborne);
                }
                else
                {
                    Debug.LogError("ERROR: Simulate - Top Slope - Can't transition to airborne. ");
                }
            }
        }

        /* From Undefined State. */
        else
        {
            Debug.LogWarning("Simulate_Update: State Simulate not defined from " + fsm.LastState);
        }
    }

    void Simulate_OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("Simulate - OnCollisionEnter from " + fsm.LastState);
        BaseCollisionEnter2D(collision);

        /*collisionState.printStatesError();
        Debug.LogError("TOPSLOPE " + enterCollisionTypes.Contains(CollisionType.TopSlope) + " SLOPE " + enterCollisionTypes.Contains(CollisionType.Slope));
        foreach (CollisionType e in enterCollisionTypes) {
            Debug.LogError("---- " + e);
        }
        foreach (ContactPoint2D c in collision.contacts) {
            Debug.LogError("THIS - " + c.normal);
        }*/


        /* These are the new collisions this frame from this specific collision. */
        // ? Iterate for all combinations not needed with contains.
        if (enterCollisionTypes.Count > 0)
        {
            /* Top Collision. */
            if (enterCollisionTypes.Contains(CollisionType.Top))
            {
                velocity.y = 0;
                if (!collisionState.Slope && !collisionState.Left && !collisionState.Right && !collisionState.Bot)
                {
                    fsm.ChangeState(CStatesBase.Airborne);
                }
                // Case1.04: climbing slope topcollision (touching top == true, touching slope == true) skips airborne, 
                // directly from slope to top in simulation, skipping the airborne that would be there with finer calculations..
                else if (collisionState.Slope)
                {
                    fsm.ChangeState(CStatesBase.ClimbingSlope); //TODO: Check this with topSlopes
                }
                else { Debug.LogError("ERROR: State Transition Top Corner"); }

                /*if (fsm.LastState == States.ClimbingSlope) {
                    velocity.x = 0;
                    fsm.ChangeState(States.ClimbingSlope);
                }*/
                enterCollisionTypes.Remove(CollisionType.Top);
            }

            /* Top Slope Collision. */
            else if (enterCollisionTypes.Contains(CollisionType.TopSlope)) // Heavy Check this.
            {
                if (slopeAngle > CStats.wallAngleMax && slopeAngle < CStats.topAngleMin)
                {
                    topSlopeSpeedCur = velocity;
                    //fsm.ChangeState(States.TopSlope);
                }
                else { Debug.LogError("TopCollision - Invalid Angle"); }

                velocity.y = 0;

                if (!collisionState.Slope && !collisionState.Left && !collisionState.Right && !collisionState.Bot)
                {
                    fsm.ChangeState(CStatesBase.Airborne);
                }
                // Case1.04: climbing slope topcollision (touching top == true, touching slope == true) skips airborne, 
                // directly from slope to top in simulation, skipping the airborne that would be there with finer calculations..
                else if (collisionState.Bot)
                {
                    fsm.ChangeState(CStatesBase.Idle);
                }
                else if (collisionState.Slope)
                {
                    fsm.ChangeState(CStatesBase.TopSlope); // Case 1.06
                }
                else { Debug.LogError("ERROR: State Transition Top Corner"); }

                enterCollisionTypes.Remove(CollisionType.TopSlope);
            }

        }
        DoCollision(collision);
    }

    IEnumerator Action_Enter()
    {
        /* 1. Halts other calculations and states by switching to this state. 
           2. Sets cActionsBase.fsm from Waiting to FindState where the respective actionState is calculated.
           3. When the action is carried out or interrupted, switches to Waiti
           // Must have control of this class's vars
           4. Action continues from previous state.*/

        // TODO: Should wait for all exit functions to finish executing.
        Debug.Log("ACTION - Enter.");
        yield return new WaitForEndOfFrame();// WaitforEndofFrame();
        Debug.LogError("Should be end of frame.");
        cActionsBase.fsm.ChangeState(CActionsBase.CStatesActionsBase.FindState, StateTransition.Safe); // Access the enum of CActionBase.
    }

    void Action_Update()
    {
        Debug.Log("ACTION - Update. ");
        PreStateUpdate();
    }

    /* Action bases whether or not it should continue the action based on whether or not the state
     is currently Action. If Collision transitions to a different state, the action will halt and go
     to a waiting state, continuing from that new state's update. */
    void Action_OnCollisionEnter2D(Collision2D collision)
    {
        CStatesBase curState = GetCurState();

        // DoCollisionFrom(curState, shouldTransition);

        // Will automatically transition out of this state if the collision from the curState needs it, halting the action
        // Else will perform the collision and continue Action Execution.
        if (curState == CStatesBase.Airborne)
        {
            Airborne_OnCollisionEnter2D(collision);
        }
        else if (curState == CStatesBase.ClimbingSlope)
        {
            ClimbingSlope_OnCollisionEnter2D(collision);
        }
        else if (curState == CStatesBase.Idle)
        {
            Idle_OnCollisionEnter2D(collision);
        }
        else if (curState == CStatesBase.OnWall)
        {
            OnWall_OnCollisionEnter2D(collision);
        }
        else if (curState == CStatesBase.Running)
        {
            Running_OnCollisionEnter2D(collision);
        }
        else if (curState == CStatesBase.SteepSlope)
        {
            SteepSlope_OnCollisionEnter2D(collision);
        }
        else if (curState == CStatesBase.TopSlope)
        {
            TopSlope_OnCollisionEnter2D(collision);
        }
        else
        {
            Debug.LogError("ERROR: State not found");
        }
    }

    void Action_Finally()
    {
        Debug.Log("ACTION - Finally. ");

    }

    void FindState_Enter()
    {
        if (fsm.LastState != CStatesBase.Action)
        {
            Debug.LogWarning("FINDSTATE - Enter from " + fsm.LastState);
        }

    }

    /* Identify current state then switch to that state. */
    void FindState_Update()
    {
        Debug.LogWarning("FINDSTATE - UPDATE");
        PreStateUpdate();
        fsm.ChangeState(GetCurState(), StateTransition.Safe);
    }

    /* Identify the current state. */
    CStatesBase GetCurState()
    {
        // Order does matter here, some collisions take precedence
        if (collisionState.Slope)
        {
            return CStatesBase.ClimbingSlope;
        }
        else if (collisionState.Bot)
        {
            if (velocity.x == 0) return CStatesBase.Idle;
            else return CStatesBase.Running;
        }
        else if (collisionState.SteepSlope)
        {
            return CStatesBase.SteepSlope;
        }

        // implied not on ground or slope
        else if (collisionState.Left && collisionState.Right)
        {
            return CStatesBase.OnWall;
        }
        else if (collisionState.Top)
        {
            return CStatesBase.Airborne;
        }
        else if (collisionState.None)
        {
            return CStatesBase.Airborne;
        }

        Debug.LogError("ERROR: State not found.");
        collisionState.printStatesError();
        return CStatesBase.FindState;
    }