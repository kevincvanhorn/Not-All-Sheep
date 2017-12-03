using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformController : RaycastController {

    public LayerMask passengerMask;
    public Vector3 move;

    List<PassengerMovement> passengerMovement;

    // Use this for initialization
    public override void Start() {
        base.Start();
    }

    // Update is called once per frame
    void Update() {
        UpdateRaycastOrigins();

        Vector3 velocity = move * Time.deltaTime;

        CalculatePassengerMovement(velocity);

        MovePassengers(true);
        transform.Translate(velocity);
        MovePassengers(false);
    }

    void MovePassengers(bool beforeMovePlatform) {
        foreach (PassengerMovement passenger in passengerMovement) {
            if(passenger.moveBeforePlatform == beforeMovePlatform) {
                passenger.transform.GetComponent<Controller2D>().Move(passenger.velocity);
            }
        }
    }

    void CalculatePassengerMovement(Vector3 velocity) {
        HashSet<Transform> movedPassengers = new HashSet<Transform>(); // fast at adding and checking if contains
        passengerMovement = new List<PassengerMovement>();

        float directionX = Mathf.Sign(velocity.x);
        float directionY = Mathf.Sign(velocity.y);

        // Vertically moving platform
        if (velocity.y != 0) {
            float rayLength = Mathf.Abs(velocity.x) + skinWidth;

            for (int i = 0; i < horizontalRayCount; i++) {
                // Fire rays from top or bot depending on direction moving.
                Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
                // Draw for each in rayCount, draws at next position bc velocity.x
                rayOrigin += Vector2.right * (verticalRaySpacing * i);

                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, passengerMask);

                if (hit) {
                    if (!movedPassengers.Contains(hit.transform)) {
                        movedPassengers.Add(hit.transform); // move each passenger once per frame

                        // let gap between passenger and platform close
                        float pushY = velocity.y - (hit.distance - skinWidth) * directionY;
                        float pushX = (directionY == 1) ? velocity.x : 0; // only when on platform moving up

                        hit.transform.Translate(new Vector3(pushX, pushY)); // move the passenger
                        passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), directionY == 1, true));

                    }
                }
            }
        }

        // Horizontally moving platform - pushing player from side
        if (velocity.x != 0) {
            float rayLength = Mathf.Abs(velocity.x) + skinWidth;

            for (int i = 0; i < horizontalRayCount; i++) {
                // Fire rays from top or bot depending on direction moving.
                Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
                // Draw for each in rayCount, draws at next position bc velocity.x
                rayOrigin += Vector2.up * (horizontalRaySpacing * i);

                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, passengerMask);

                if (hit) {
                    if (!movedPassengers.Contains(hit.transform)) {
                        movedPassengers.Add(hit.transform); // move each passenger once per frame

                        // let gap between passenger and platform close
                        float pushY = 0;
                        float pushX = velocity.x - (hit.distance - skinWidth) * directionX; // only when on platform moving up

                        passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), false, true));
                    }
                }
            }



        }

        // If  Passenger on top of a horizontally or downward moving playform
        if(directionY == -1 || velocity.y == 0 && velocity.x != 0) {
            float rayLength = skinWidth * 2 ;

            for (int i = 0; i < horizontalRayCount; i++) {
                // Fire rays from top or bot depending on direction moving.
                Vector2 rayOrigin = raycastOrigins.topLeft + Vector2.right * (verticalRaySpacing * i);

                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up, rayLength, passengerMask);

                if (hit) {
                    if (!movedPassengers.Contains(hit.transform)) {
                        movedPassengers.Add(hit.transform); // move each passenger once per frame

                        // let gap between passenger and platform close
                        float pushY = velocity.y;
                        float pushX = velocity.x;

                        passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), true, false));
                    }
                }
            }
        }

    }

    struct PassengerMovement {
        public Transform transform;
        public Vector3 velocity;
        public bool standingOnPlatform;
        public bool moveBeforePlatform;

        public PassengerMovement(Transform _transform, Vector3 _velocity, bool _standingOnPlatform, bool _moveBeftPlatform) {
            transform = _transform;
            velocity = _velocity;
            standingOnPlatform = _standingOnPlatform;
            moveBeforePlatform = _moveBeftPlatform;
        }
    }
}
