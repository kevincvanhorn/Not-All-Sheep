using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Controller2D : MonoBehaviour {

    public LayerMask collisionMask; // Which layers we want Object to collide with.

    /* Need to inset by a skin width, so that there is room for the
     * raycast to fire even when the object is grounded/colliding. */
    const float skinWidth = .015f;
    /* How many rays are being fired off horiz/vert */
    public int horizontalRayCount = 4;
    public int verticalRayCount = 4;

    float maxClimbAngle = 80;
    float maxDescendAngle = 75;

    float horizontalRaySpacing;
    float verticalRaySpacing;

    BoxCollider2D collider;
    RaycastOrigins raycastOrigins;
    public CollisionInfo collisions;

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
        collisions.Reset();     // Reset info on what the object is touching - to update this frame w/ new info.

        if (moveDist.y < 0) {
            DescendSlope(ref moveDist);
        }

        if (moveDist.x != 0) {
            HorizontalCollisions(ref moveDist); // Any change to moveDist in VertCollisions will change here (ref).
        }

        if (moveDist.y != 0) {
            VerticalCollisions(ref moveDist);   // Any change to moveDist in VertCollisions will change here (ref).
        }

        transform.Translate(moveDist);
    }

    void HorizontalCollisions(ref Vector2 moveDist) {
        float directionX = Mathf.Sign(moveDist.x); // Moving Right = 1, Left = -1.
        float rayLength = Mathf.Abs(moveDist.x) + skinWidth;

        /* Draw horizontal rays. */
        for (int i = 0; i < horizontalRayCount; i++) {
            /* Update rayOrigin based on direction moving. */
            Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
            /* Use the moveDist from Player Update to update rayOrigins this tick. */
            rayOrigin += Vector2.up * (horizontalRaySpacing * i); // Casting from current loc before applying new moveDist.

            /* Perform raycast in direction of movement with rayLength */
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

            Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength, Color.red);

            if (hit) {
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up); // Angle from slope normal.

                /* Handle Slopes */
                if (i == 0 && slopeAngle <= maxClimbAngle) {
                    float distanceToSlopeStart = 0;
                    /* If on a new slope - move up to slope then go up. */
                    if (slopeAngle != collisions.slopeAngleOld) {
                        distanceToSlopeStart = hit.distance - skinWidth;
                        moveDist.x -= distanceToSlopeStart * directionX;
                    }
                    ClimbSlope(ref moveDist, slopeAngle);            // On new slope: the distance to the slope is left out.
                    moveDist.x += distanceToSlopeStart * directionX; // Add left out dist when done climbing slope.
                }

                /* Check rest of rays only when not climbing slope or when running into maxslope. */
                if (!collisions.climbingSlope || slopeAngle > maxClimbAngle) {
                    // Ray distance to hit.
                    moveDist.x = (hit.distance - skinWidth) * directionX;
                    rayLength = hit.distance; // Update rayLength to shortest coll hit.

                    /* On slope, lateral collision. */
                    if (collisions.climbingSlope) {
                        moveDist.y = Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(moveDist.x); // Use collisions b/c local slopeAngle is updated with rays (ex: 90deg)
                    }

                    collisions.left = directionX == -1; // If touching left.
                    collisions.right = directionX == 1; // If touching right.
                }
            }
        }
    }

    void VerticalCollisions(ref Vector2 moveDist) {
        float directionY = Mathf.Sign(moveDist.y); // Moving Up = 1, Down = -1.
        float rayLength = Mathf.Abs(moveDist.y) + skinWidth;

        /* Draw vertical rays. */
        for (int i = 0; i < verticalRayCount; i++) {
            /* Update rayOrigin based on direction moving. */
            Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
            /* Use the moveDist from Player Update to update rayOrigins this tick. */
            rayOrigin += Vector2.right * (verticalRaySpacing * i + moveDist.x); // ~ Add moveDist to cast from where will be since moveDist.X is updated already.

            /* Perform raycast in direction of movement with rayLength */
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);

            Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength, Color.red);

            if (hit) {
                // Ray distance to hit.
                moveDist.y = (hit.distance - skinWidth) * directionY;
                rayLength = hit.distance; // Udpate rayLength to shortest coll hit.

                /* On slope, collision above. */
                if (collisions.climbingSlope) {
                    moveDist.x = moveDist.y / Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Sign(moveDist.x);
                }

                collisions.top = directionY == 1; // If touching top.
                collisions.bot = directionY == -1; // If touching bot.
            }
        }

        /* In a slope (overshot) - When changing slopes. */
        if (collisions.climbingSlope) {
            /* Check if new slope from height of projected movement. */
            float directionX = Mathf.Sign(moveDist.x);
            rayLength = Mathf.Abs(moveDist.x) + skinWidth;
            /* Horizontal cast from new height (of the coming transform) */
            Vector2 rayOrigin = ((directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight) + Vector2.up * moveDist.y;
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

            if (hit) {
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                /* Have collided with new slope. */
                if (slopeAngle != collisions.slopeAngle) {
                    moveDist.x = (hit.distance - skinWidth) * directionX;
                    // TODO: ~ feel like moveDist.y should be set like in Horiz method when changing slopes
                    collisions.slopeAngle = slopeAngle;
                }
            }
        }
    }

    void ClimbSlope(ref Vector2 moveDist, float slopeAngle) {
        /* Set speed of slope climb same as on flat ground. */
        float slopeDist = Mathf.Abs(moveDist.x); // The hypotenuse, or distance up slope
        float slopeDistY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * slopeDist; // Y component

        /* Moving up Slope -
         * else Only Jumping off Slope if (moveDist.y > slopeMoveDistY)
         * Assume requested jump if greater moveDist than slope y component. */
        if (moveDist.y <= slopeDistY) {
            moveDist.y = slopeDistY;
            moveDist.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * slopeDist * Mathf.Sign(moveDist.x);
            collisions.bot = true; // Assume that object is grounded when onSlope.
            collisions.climbingSlope = true;
            collisions.slopeAngle = slopeAngle;
        }
    }

    void DescendSlope(ref Vector2 moveDist) {
        float directionX = Mathf.Sign(moveDist.x);
        Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomRight : raycastOrigins.bottomLeft;
        /* Cast ray down. */
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, -Vector2.up, Mathf.Infinity, collisionMask);

        if (hit) {
            float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
            /* Not on flat surface or maxSlope*/
            if (slopeAngle != 0 && slopeAngle <= maxDescendAngle) {
                /* Moving down slope. */
                if (Mathf.Sign(hit.normal.x) == directionX) {
                    /* Check if close enough for slope to come into effect. Distance to slope < how far to move on y-axis for angle & xDist*/
                    if (hit.distance - skinWidth <= Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(moveDist.x)) {
                        float displacementX = Mathf.Abs(moveDist.x);
                        float slopeDistY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * displacementX; // Y component
                        moveDist.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * displacementX * Mathf.Sign(moveDist.x);
                        moveDist.y -= slopeDistY;

                        collisions.slopeAngle = slopeAngle;
                        collisions.descendingSlope = true;
                        collisions.bot = true;
                    }
                }
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

        /* Clamp one ray for each corner */
        horizontalRayCount = Mathf.Clamp(horizontalRayCount, 2, int.MaxValue);
        verticalRayCount = Mathf.Clamp(verticalRayCount, 2, int.MaxValue);

        /* Calc Spacing between each ray */
        horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
        verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
    }

    /* Easily get corners of box collider */
    struct RaycastOrigins {
        public Vector2 topLeft, topRight;
        public Vector2 bottomLeft, bottomRight;
    }

    /* Information as to where collisions are occuring. */
    public struct CollisionInfo {
        public bool top, bot;    // bot = isGrounded
        public bool left, right; // left = isTouchingLeft, right = isTouchingRight 

        public bool climbingSlope;
        public bool descendingSlope;
        public float slopeAngle, slopeAngleOld;

        /* Reset collision information */
        public void Reset() {
            top = bot = false;
            left = right = false;
            climbingSlope = false;
            descendingSlope = false;

            slopeAngleOld = slopeAngle;
            slopeAngle = 0;
        }
    }
}

//TODO: 12:33 ep 5