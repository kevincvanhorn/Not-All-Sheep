using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Controller2D : RaycastController {

    float maxClimbAngle = 80;
    float maxDescendAngle = 75;

    public CollisionInfo collisions;

    public override void Start() {
        base.Start();
    }

    /* Translate the Object with a given velocity*Time.deltaTime */
    public void Move(Vector3 velocity) {
        UpdateRaycastOrigins();           // Set raycast origins based on location.
        collisions.Reset();
        collisions.velocityOld = velocity;

        if (velocity.y < 0) {
            DescendSlope(ref velocity);
        }

        if(velocity.x != 0) {
            HorizontalCollisions(ref velocity);
        }
        if(velocity.y !=0) {
            VerticalCollisions(ref velocity); // pass reference of velocity var.
        }
        

        transform.Translate(velocity);    // Move object.
    }

    void HorizontalCollisions(ref Vector3 velocity) {
        float directionX = Mathf.Sign(velocity.x); // Down = -1, Up = 1
        float rayLength = Mathf.Abs(velocity.x) + skinWidth;

        for (int i = 0; i < horizontalRayCount; i++) {
            // Fire rays from top or bot depending on direction moving.
            Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
            // Draw for each in rayCount, draws at next position bc velocity.x
            rayOrigin += Vector2.up * (horizontalRaySpacing * i);

            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);
            Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength, Color.red);

            if (hit) {

                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up); // Slope angle from bottomMost

                if(i == 0 && slopeAngle <= maxClimbAngle) {
                    if (collisions.descendingSlope) {
                        collisions.descendingSlope = false;
                        velocity = collisions.velocityOld;
                    }

                    float distanceToSlopeStart = 0;
                    if(slopeAngle != collisions.slopeAngleOld) {
                        distanceToSlopeStart = hit.distance - skinWidth;
                        velocity.x -= distanceToSlopeStart * directionX; // only use velX when actually reaches slope
                    }
                    ClimbSlope(ref velocity, slopeAngle);
                    velocity.x -= distanceToSlopeStart * directionX;
                }

                /* Check rest of rays if not on slope */
                if(!collisions.climbingSlope || slopeAngle > maxClimbAngle) {
                    // Move to point where ray intersected
                    velocity.x = (hit.distance - skinWidth) * directionX;
                    rayLength = hit.distance; // rayLength shortens to nearest detected object.

                    if (collisions.climbingSlope) {
                        velocity.y = Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x);
                    }

                    collisions.left = directionX == -1;
                    collisions.right = directionX == 1;
                }

                
            }
        }
    }

    void VerticalCollisions(ref Vector3 velocity) {
        float directionY = Mathf.Sign(velocity.y); // Down = -1, Up = 1
        float rayLength = Mathf.Abs(velocity.y) + skinWidth;

        for (int i = 0; i < verticalRayCount; i++) {
            // Fire rays from top or bot depending on direction moving.
            Vector2 rayOrigin = (directionY == -1)? raycastOrigins.bottomLeft:raycastOrigins.topLeft;
            // Draw for each in rayCount, draws at next position bc velocity.x
            rayOrigin += Vector2.right * (verticalRaySpacing * i + velocity.x);

            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);
            Debug.DrawRay(rayOrigin, Vector2.up * directionY *rayLength, Color.red);

            if (hit) {
                // Move to point where ray intersected
                velocity.y = (hit.distance - skinWidth) * directionY;
                rayLength = hit.distance; // rayLength shortens to nearest detected object.

                // For collisions above when climbing slope.
                if (collisions.climbingSlope) {
                    velocity.x = velocity.y / Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Sign(velocity.x);
                }

                collisions.below = directionY == -1;
                collisions.above = directionY == 1;
            }
        }

        // When Changing slopes - check if new slope at that height
        if (collisions.climbingSlope) {
            float directionX = Mathf.Sign(velocity.x);
            rayLength = Mathf.Abs(velocity.x) + skinWidth;
            Vector2 rayOrigin = ((directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight) + Vector2.up * velocity.y;
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

            if (hit) {
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                if(slopeAngle != collisions.slopeAngle) {
                    // collided with new slope
                    velocity.x = (hit.distance - skinWidth) * directionX;
                    collisions.slopeAngle = slopeAngle;
                }
            }
        }
    }

    void ClimbSlope(ref Vector3 velocity, float slopeAngle) {
        // Make moving velocity up slope same as normal x movespeed
        float moveDistance = Mathf.Abs(velocity.x);
        float climbVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance; // So velY does not overwrite

        if(velocity.y <= climbVelocityY) {
            velocity.y = climbVelocityY;
            velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x);
            collisions.below = true;
            collisions.climbingSlope = true;
            collisions.slopeAngle = slopeAngle;
        }
    }

    void DescendSlope(ref Vector3 velocity) {
        float directionX = Mathf.Sign(velocity.x);

        // Cast ray down
        Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft:raycastOrigins.bottomLeft;
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, -Vector2.up, Mathf.Infinity, collisionMask);

        if (hit) {
            float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
            if(slopeAngle != 0 && slopeAngle <= maxDescendAngle) {
                 //If moving down the slope
                 if(Mathf.Sign(hit.normal.x) == directionX) {
                    if(hit.distance - skinWidth <= Mathf.Tan(slopeAngle*Mathf.Deg2Rad)* Mathf.Abs(velocity.x)) {
                        float moveDistance = Mathf.Abs(velocity.x);
                        float descendVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance; // So velY does not overwrite
                        velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x);
                        velocity.y -= descendVelocityY;

                        collisions.slopeAngle = slopeAngle;
                        collisions.descendingSlope = true;
                        collisions.below = true; 
                    }
                }
            }
        }

    }
	
    public struct CollisionInfo {
        public bool above, below;
        public bool left, right;

        public bool climbingSlope;
        public bool descendingSlope;
        public float slopeAngle, slopeAngleOld;
        public Vector3 velocityOld;

        public void Reset() {
            above = below = false;
            left = right = false;
            climbingSlope = false;
            descendingSlope = false; 

            slopeAngleOld = slopeAngle;
            slopeAngle = 0;
        }
    }
}
