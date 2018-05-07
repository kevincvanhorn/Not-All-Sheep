[RequireComponent(typeof(CCollisionState))]
[RequireComponent(typeof(CActionsBase))]
public class CharacterBase : MonoBehaviour
{
    /* Collisions Vars */
    private float slideFactor = 1;

    private float jumpVelRatio; // Vxmax = (Vxmax * Vymin) / Vymin

    public Vector3 debugSlopeHitLoc;

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

    void TopSlope_Enter()
    {
        velocity = topSlopeSpeedCur;
        Debug.Log("TOPSLOPE - Enter");
    }

    void TopSlope_Update()
    {
        PreStateUpdate();
        Debug.Log("TOPSLOPE - Update");

        float platformSlippiness = 20;
        slideFactor = 1 / (Mathf.Abs(velocity.x)) * 100 + platformSlippiness;
        if (slideFactor < 1)
        {
            slideFactor = 1;
        }

        velocity = topSlopeSpeedCur;

        Vector2 slopeVector = (Vector2)(Quaternion.Euler(0, 0, 180 - slopeAngle) * Vector2.right); //PFEF
        float hitAngle = Vector2.Angle(velocity, slopeVector);
        //velocity.x = slopeHitSpeed.x * Mathf.Cos((180 - slopeAngle) * Mathf.Deg2Rad); // - velocity?
        velocity.y = Mathf.Abs(topSlopeSpeedCur.x) * Mathf.Sin((180 - slopeAngle) * Mathf.Deg2Rad);

        Debug.Log("SlideFactor " + slideFactor);
        Debug.Log("Slope Angle: " + (180 - slopeAngle));
        //Debug.LogError("Slope Vector: " + slopeVector);
        Debug.Log("Hit Angle : " + hitAngle);
        //Debug.DrawLine(new Vector3(0,0,0), slopeVector, Color.yellow, 20);
        //Debug.DrawLine(new Vector3(0, 0, 0), velocity, Color.red, 20);
        Debug.DrawLine(debugSlopeHitLoc, debugSlopeHitLoc + (Vector3)slopeVector, Color.green, 20);
        Debug.DrawLine(debugSlopeHitLoc, debugSlopeHitLoc + velocity, Color.red, 20);

        if (velocity.x == 0)
        {
            topSlopeSpeedCur.y = 0;
        }
        else
        {
            topSlopeSpeedCur.y += gravity * slideFactor * Time.deltaTime; // Apply Gravity until grounded
        }

        if (topSlopeSpeedCur.y <= 0)
        {
            fsm.ChangeState(CStatesBase.Simulate, StateTransition.Safe);
        }

        Debug.Log("slopeHitSpeed" + topSlopeSpeedCur);
        Debug.Log("velocity" + velocity);

        if (!collisionState.Slope)
        {
            fsm.ChangeState(CStatesBase.Simulate, StateTransition.Safe);
        }
    }

    void TopSlope_OnCollisionEnter2D(Collision2D collision)
    {
        Debug.LogError("TOPSLOPE - OnCollisionEnter2D - TODO Address this.");
        BaseCollisionEnter2D(collision);

        DoCollision(collision);
    }

    void SteepSlope_Enter()
    {
        Debug.Log("STEEPSLOPE - Enter");

        /* Sprint Calc ------------------------------------------------- */
        if (Input.GetKey(KeyCode.LeftShift))
        {
            activeSpeed = sprintSpeed;
        }
        else
        {
            activeSpeed = moveSpeed;
        }

        if (velocity.y < 0 && !collisionState.Bot && !collisionState.Slope)
        {
            //Debug.DrawLine(debugSlopeHitLoc, debugSlopeHitLoc + (Vector3)steepSlopeHitNormal * 5, Color.blue, 10f);
            //Debug.DrawLine(debugSlopeHitLoc, debugSlopeHitLoc + velocity * -1, Color.yellow, 10f);
            float steepNormalAngle = Vector2.Angle(velocity * -1, steepSlopeHitNormal);
            steepSlopeSpeed = Mathf.Sin(steepNormalAngle * Mathf.Deg2Rad) * velocity.magnitude * Mathf.Sign(velocity.y);

            //Debug.LogError("Velocity   " + velocity.magnitude);
            //Debug.LogError("Angle      " + steepNormalAngle);
            //Debug.LogError("Speed Calc " + steepSlopeSpeed);
        }
        else
        {
            steepSlopeSpeed = velocity.magnitude;
        }
    }

    void SteepSlope_Update()
    {
        PreStateUpdate();
        Debug.Log("STEEPSLOPE - Update");

        slopeAngle = collisionState.curSteepSlopeAngle; // CollisionState update is before this.

        /* Lateral Calc -------------------------------------------------- */
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow))
        {
            if (collisionState.Right && Input.GetKey(KeyCode.RightArrow))
            {
                velocity.x = 0;
                velocity.y = 0;
            }
            else if (collisionState.Left && Input.GetKey(KeyCode.LeftArrow))
            {
                velocity.x = 0;
                velocity.y = 0;
            }
        }

        steepSlopeSpeed += gravity * Time.deltaTime; // Slide down Slope

        velocity.x = steepSlopeSpeed * Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * collisionState.steepSlopeDir; // steepSlopeSpeed
        velocity.y = steepSlopeSpeed * Mathf.Sin(slopeAngle * Mathf.Deg2Rad);

        if (collisionState.Top || collisionState.TopSlope)
        { // NOTE: Do not else with above, uses calculated velocity.
            if (velocity.y > 0)
            {
                velocity.x = 0;
                velocity.y = 0;
            }
        }

        /* Priority Cases (Change the actual state). */
        if (inputManager.ActionKeyPressed())
        { // Trigger Action.
            fsm.ChangeState(CStatesBase.Action);
        }
        else if (collisionState.None)
        { // Case - slide off edge
            Debug.Log("STEEPSLOPE - Transition 5");
            fsm.ChangeState(CStatesBase.Simulate, StateTransition.Safe);
        }
        else if ((collisionState.Left || collisionState.Right) && !collisionState.steepSlope) // TODO: Test this. 
        { // Ran up slope and skid up wall
            // Case1.03: not on slope - just above at corner of slope and wall. 
            if (!collisionState.Top && !collisionState.TopSlope)
            {
                if (Input.GetKeyDown(KeyCode.UpArrow))
                {
                    Debug.Log("STEEPSLOPE - Transition 4");
                    //NOTE! Remember to copy this jump behavior to the Case1.03 Above
                    velocity.y = jumpVelocityMax;
                    fsm.ChangeState(CStatesBase.Simulate, StateTransition.Safe);
                }
                else if (velocity.y >= 0 && (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow)))
                {
                    Debug.Log("STEEPSLOPE - Transition 7");
                    velocity.y = jumpVelocityMax;
                    fsm.ChangeState(CStatesBase.Simulate, StateTransition.Safe);
                }

            }
        }

        /* Vertical JUMP Calc ------------------------------------------ */
        // Jump if pressed && not touchingTop (ex: sandwiched between two platforms).
        else if (!collisionState.Top && !collisionState.TopSlope)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                Debug.Log("STEEPSLOPE - Transition 3");
                //NOTE! Remember to copy this jump behavior to the Case1.03 Above
                velocity.y = jumpVelocityMax;
                fsm.ChangeState(CStatesBase.Simulate, StateTransition.Safe);
            }
            else if (velocity.y >= 0 && (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow)))
            {
                Debug.Log("STEEPSLOPE - Transition 6");
                velocity.y = jumpVelocityMax;
                fsm.ChangeState(CStatesBase.Simulate, StateTransition.Safe);
            }

        }
        Debug.Log(velocity);
    }

    void SteepSlope_OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("STEEPSLOPE - OnCollisionEnter2D");
        BaseCollisionEnter2D(collision);

        if (enterCollisionTypes.Count > 0)
        {
            /* Wall Collision (Including Wall Slopes). */
            if (enterCollisionTypes.Contains(CollisionType.Right))
            {
                wallHitSpeed = velocity;
                // TouchingWall.
                velocity.x = 0;
                velocity.y = 0;
                enterCollisionTypes.Remove(CollisionType.Right);
            }
            else if (enterCollisionTypes.Contains(CollisionType.Left))
            {
                wallHitSpeed = velocity;
                // TouchingWall.
                velocity.x = 0;
                velocity.y = 0;
                enterCollisionTypes.Remove(CollisionType.Left);
            }

            /* Top Collision. */
            else if (enterCollisionTypes.Contains(CollisionType.Top))
            {
                velocity.x = 0;
                velocity.y = 0; // Redundancy case - addressed in this.Update.
                enterCollisionTypes.Remove(CollisionType.Top);
            }

            /* Top Slope Collision. */
            else if (enterCollisionTypes.Contains(CollisionType.TopSlope))
            {
                // 01.01.18b01 ?
                if (slopeAngle > CStats.wallAngleMax && slopeAngle < CStats.topAngleMin)
                { // TODO: Address this. Should stop moving if hits topSlope.
                    velocity.x = 0;
                    velocity.y = 0; // Redundancy case - addressed in this.Update.
                }
                else { Debug.LogError("TopCollision - Invalid Angle"); }

                enterCollisionTypes.Remove(CollisionType.TopSlope);
            }

            /* Bot Collision. */
            else if (enterCollisionTypes.Contains(CollisionType.Bot))
            {
                //velocity.x = activeSpeed; //1.3.18
                velocity.y = 0;
                fsm.ChangeState(CStatesBase.Running, StateTransition.Safe);
                enterCollisionTypes.Remove(CollisionType.Bot);
            }

            /* Steep Slope Collision. */
            else if (enterCollisionTypes.Contains(CollisionType.SteepSlope))
            {
                // TODO: Add the if statements and else to be sure of angle.
            }

            /* Slope Collision. */
            else if (enterCollisionTypes.Contains(CollisionType.Slope))
            {
                //fsm.ChangeState(States.ClimbingSlope, StateTransition.Safe);
                if (slopeAngle > CStats.slopeAngleMin && slopeAngle <= CStats.slopeAngleMax)
                {
                    fsm.ChangeState(CStatesBase.ClimbingSlope, StateTransition.Safe);
                }
                else { Debug.LogError("TopCollision - Invalid Angle " + slopeAngle); }
                enterCollisionTypes.Remove(CollisionType.Slope);
            }


            /* Undefined State. */
            else
            {
                fsm.ChangeState(CStatesBase.FindState, StateTransition.Safe);
            }
        }
    }

    /* Simulate is for the 4-5 frames after a jump/transition away from an object into empty space occurs.
     Needed for the collision state to catch up so that actions like airborne checking if it's touching the floor
     in an update does not occur immediately at the first frame of up pressed out of the grounded state while the object is 
     still "grounded" by the bounding check.*/
    void Simulate_Enter()
    {
        Debug.Log("SIMULATE - Enter from" + fsm.LastState);
        //collisionState.printStatesError();
    }

    void Simulate_Update()
    {
        Debug.Log("Simulate_Update");
        PreStateUpdate();

        /* Lateral Calc -------------------------------------------*/
        if (Input.GetKey(KeyCode.RightArrow) && velocity.x < activeSpeed)
        { // in-air lateral move right
            velocity.x += lateralAccelAirborne * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.LeftArrow) && velocity.x > -activeSpeed)
        { // in-air lateral move left
            velocity.x -= lateralAccelAirborne * Time.deltaTime;
        }

        /* Vertical Calc ----------------------------------------- */
        if (Input.GetKeyUp(KeyCode.UpArrow))
        {  // Variable jump - When Up is released in this frame.
            if (velocity.y > jumpVelocityMin)
            {
                velocity.x = (jumpVelocityMin * velocity.x) / velocity.y;
                velocity.y = jumpVelocityMin;
            }
        }


        velocity.y += gravity * Time.deltaTime; // Apply Gravity until grounded

        // Trigger Action.
        if (inputManager.ActionKeyPressed())
        {
            fsm.ChangeState(CStatesBase.Action);
        }
        /* From Running State */
        else if (fsm.LastState == CStatesBase.Running)
        {
            if (!collisionState.Bot)
            {
                fsm.ChangeState(CStatesBase.Airborne);
            }
        }

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