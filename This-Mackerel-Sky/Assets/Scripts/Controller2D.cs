using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Controller2D : MonoBehaviour {

    public LayerMask collisionMask; // Which layers we want Object to collide with.

    /* Need to inset by a skin width, so that there is room for the
     * raycast to fire even when the object is grounded/colliding. */
    const float skinWidth = .015f;

    float horizontalRaySpacing;
    float verticalRaySpacing;

    BoxCollider2D collider;
    RaycastOrigins raycastOrigins;

    // Use this for initialization
    void Start() {
        collider = GetComponent<BoxCollider2D>();

        CalculateRaySpacing();
    }

    /**
     * Handles collisions and modifies velocity to prevent going through objects 
     * by responding to a request in move distance (x = v*dt).
     * @param moveDist the requested change in distance (v*dt).
     */
    public void Move(Vector2 moveDist) {
        UpdateRaycastOrigins(); // Set origins in correct location since last frame translate.

        VerticalCollisions(ref moveDist); // Any change to moveDist in VertCollisions will change here (ref).

        //transform.Translate(moveDist);
    }

    void VerticalCollisions(ref Vector2 moveDist) {
        float directionY = Mathf.Sign(moveDist.y); // Moving Up = 1, Down = -1.
        float rayLength = Mathf.Abs(moveDist.y) + skinWidth;

        /* Draw vertical rays. */
        for (int i = 0; i < 2; i++) {
            /* Update rayOrigin based on direction moving. */
            Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
            /* Use the moveDist from Player Update to update rayOrigins this tick. */
            rayOrigin += Vector2.right * (verticalRaySpacing * i + moveDist.x);

            /* Perform raycast in direction of movement with rayLength */
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);

            Debug.DrawRay(rayOrigin, Vector2.up * directionY * skinWidth*20, Color.red);

            if (hit) {
                // Ray distance to hit.
                moveDist.y = (hit.distance - skinWidth) * directionY;
                rayLength = hit.distance; // Udpate rayLength to shortest coll hit.
            }
        }
    }

    void UpdateRaycastOrigins() {
        /* Get and shrink bounds of collider*/
        Bounds bounds = collider.bounds;
        bounds.Expand(skinWidth * -2); // 2 for skin width top and bot.

        /* Set the origins based on inset bounds. */
        raycastOrigins.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
        raycastOrigins.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
        raycastOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
        raycastOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y);
    }

    void CalculateRaySpacing() {
        /* Get and shrink bounds of collider*/
        Bounds bounds = collider.bounds;
        bounds.Expand(skinWidth * -2); // 2 for skin width top and bot.

        /* Calc Spacing between each ray */
        horizontalRaySpacing = bounds.size.y;
        verticalRaySpacing = bounds.size.x;
    }

    /* Easily get corners of box collider */
    struct RaycastOrigins {
        public Vector2 topLeft, topRight;
        public Vector2 bottomLeft, bottomRight;
    }
}