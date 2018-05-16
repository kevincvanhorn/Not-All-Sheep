using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour {

    public PBaseMovement player;
    public Vector2 focusAreaSize;

    public float verticalCameraOffset;

    /* Lookahead*/
    // Smoothing - doesn't automatically change position. Takes a smoothed delay
    public float lookAheadDistX;  // How far to look ahead.
    public float xLookSmoothTime; // Smoothing Time. 
    public float yLookSmoothTime; // Smoothing Time.

    float curLookAheadX;          // Smoothing calc dist to move.
    float targetLookAheadX;       // Flat target distance to move.
    float lookAheadDirX;          // Direction of look ahead.
    float smoothLookVelocityX;    // Mathf.Damping variable modified every frame by func. 
    float smoothLookVelocityY;        // 

    bool lookAheadStopped;
    public float xStoppingFraction = 3; // 1 is no limiting on lookahead when stopping, 100 is heavy limiting
    //---------------------- *

    FocusArea focusArea;

    void Start()
    {
        /* Get Player Object. */
        GameObject playerObj = GameObject.Find("Player");
        if (playerObj != null)
        {
            //player = playerObj.GetComponent<CharacterBase>();
            //player = Player.Instance.gameObject.GetComponent<PBaseMovement>();
            player = playerObj.GetComponent<PBaseMovement>();
        }
        //player = Player.Instance.gameObject.GetComponent<PBaseMovement>();


        //focusArea = new FocusArea(player.collider.bounds, focusAreaSize);

    }

    /* When all of the Player movemement has been used. */
    public void OnFixedUpdate()
    {
        focusArea.UpdatePos(player.collider.bounds);
        Vector2 focusPosition = focusArea.center + Vector2.up * verticalCameraOffset;

        /* LookAhead */ 
        if(focusArea.moveDist.x != 0)
        {
            //Debug.LogError(player.hasLateralInput + " : " + lookAheadStopped +" "+player.directionFacing + " " + focusArea.moveDist.x);
            lookAheadDirX = Mathf.Sign(focusArea.moveDist.x);
            // Player has decell smoothing already so focus area is always moving same dir as player
            // -- else could be moving left while player moving right but decell right.
            // Only set if input is in same direction as focus area is moving.
            if (Mathf.Sign(player.directionFacing) == Mathf.Sign(focusArea.moveDist.x) && player.hasLateralInput) // Sin(0) = 1
            {
                lookAheadStopped = false;
                targetLookAheadX = lookAheadDirX * lookAheadDistX;
            }
            else
            {
                if (!lookAheadStopped) {
                    lookAheadStopped = true;
                    // Apply only a slight smoothing when condition is not met
                    targetLookAheadX = curLookAheadX + (lookAheadDirX * lookAheadDistX - curLookAheadX)/xStoppingFraction; // 4: the fraction of smoothing - can be adjusted
                }
            }
        }
        //targetLookAheadX = lookAheadDirX * lookAheadDistX;

        curLookAheadX = Mathf.SmoothDamp(curLookAheadX, targetLookAheadX, ref smoothLookVelocityX, xLookSmoothTime);

        focusPosition.y = Mathf.SmoothDamp(transform.position.y, focusPosition.y, ref smoothLookVelocityY, yLookSmoothTime);
        focusPosition += Vector2.right * curLookAheadX;

        transform.position = (Vector3)focusPosition + Vector3.forward * -10; // -10 so camera is always in front.
    }

    private void OnDrawGizmos()
    {
        //Gizmos.color = new Color(1, 0, 0, 0.5f);
        //Gizmos.DrawCube(focusArea.center, focusAreaSize);
    }

    struct FocusArea
    {
        public Vector2 center;
        public Vector2 moveDist;
        float left, right;
        float top, bot;

        public FocusArea(Bounds playerBounds, Vector2 size)
        {
            /* Create a bounding box with width size.x and height size.y */
            left = playerBounds.center.x - size.x/2;
            right = playerBounds.center.x + size.x / 2;
            bot = playerBounds.min.y;
            top = playerBounds.min.y + size.y;

            moveDist = Vector2.zero;
            center = new Vector2((left + right) *0.5f, (top+bot) *0.5f);
        }

        /* Update position when player near edges. */
        public void UpdatePos(Bounds playerBounds)
        {
            float shiftX = 0;
            /* When Player goes out of bounds Laterally*/
            if(playerBounds.min.x < left)
            {
                shiftX = playerBounds.min.x - left;
            }
            else if(playerBounds.max.x > right)
            {
                shiftX = playerBounds.max.x - right;
            }
            left += shiftX;
            right += shiftX;

            float shiftY = 0;
            /* When Player goes out of bounds Laterally*/
            if (playerBounds.min.y < bot)
            {
                shiftY = playerBounds.min.y - bot;
            }
            else if (playerBounds.max.y > top)
            {
                shiftY = playerBounds.max.y - top;
            }
            top += shiftY;
            bot += shiftY;

            center = new Vector2((left + right) * 0.5f, (top + bot) * 0.5f);
            moveDist = new Vector2(shiftX, shiftY); // How much has moved this frame.
        }
    }
	
}
