using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Contains all of the constant vars for Player Behaviours. */
public class PStats : MonoBehaviour {

    /* -- PBaseMovement ------------ */
    public const float jumpHeightMax = 5;
    public const float jumpHeightMin = .9f;
    public const float timeToJumpApex = .4f;

    public const float steepSlopeMinEnterSpeed = 20;

    /* Collision Manager. */
    public const float botAngle = 0;       // [] ==
    public const float slopeAngleMin = 0;  // ()
    public const float slopeAngleMax = 55; // []
    public const float wallAngleMin = 85;  // [Wall]
    public const float wallAngleMax = 95;  // [Wall](TopSlope)
    public const float topAngleMin = 175;  // (TopSlope)[Top]     
    public const float topAngleMax = 180;  // [Top]


}
