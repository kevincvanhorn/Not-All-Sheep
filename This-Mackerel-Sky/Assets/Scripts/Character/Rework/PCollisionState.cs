using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Responsible for persistent collision detection. */
public class PCollisionState : MonoBehaviour
{
    public HashSet<GameObject> objectsTouching = new HashSet<GameObject>(); // The objects that the player is currently touching.
    public LayerMask collisionLayer;        // Collision layer for determining overlaps.
    public float curSlopeAngle = 0;
    public float curWallAngle = 0;
    public float curSteepSlopeAngle = 0;
    public int slopeDir = 1;
    public int steepSlopeDir;

    private ContactFilter2D contactFilter = new ContactFilter2D();
    private new Collider2D collider; // The collider of the player.

    /* Local State Variables: */
    private bool none, top, bot, left, right, slope, topSlope, steepSlope;

    /* Accessor State Variables: */
    public bool None { get { return none; } }
    public bool Top { get { return top; } }
    public bool Bot { get { return bot; } }
    public bool Left { get { return left; } }
    public bool Right { get { return right; } }
    public bool Slope { get { return slope; } }
    public bool TopSlope { get { return topSlope; } }
    public bool SteepSlope { get { return steepSlope; } }
    public bool Grounded { get { return bot || slope; } }

    /* Debugging  Variables: */
    public float debugSlopeAngle = 0;


    void Start()
    {
        collider = GetComponent<Collider2D>();
        contactFilter.layerMask = collisionLayer;
    }

    /* Checks and clears collision overlaps. */
    public void OnFixedUpdate()
    {
        ClearOverlaps();
        CheckOverlaps();
    }

    /* Checks if a given GameObject is touching the player. */
    public bool isTouchingPlayer(GameObject gameObject)
    {
        return objectsTouching.Contains(gameObject);
    }

    public void CheckOverlaps()
    {
        Collider2D[] collidersTouching = new Collider2D[4];
        Physics2D.OverlapCollider(collider, contactFilter, collidersTouching);
        float slopeAngle;

        for (int e = 0; e < collidersTouching.Length; e++)
        {
            if (collidersTouching[e])
            {
                objectsTouching.Add(collidersTouching[e].gameObject);
            }

        }

        foreach (Collider2D coll in collidersTouching)
        {

            if (coll != null)
            {
                //Debug.LogError(coll);

                ContactPoint2D[] contactsIn = new ContactPoint2D[8]; // 2 when side collides (each corner) || 1 when on slope
                coll.GetContacts(contactsIn);

                /* Call Collider Enter Functions */
                for (int i = 0; i < contactsIn.Length; i++)
                {
                    /* If contact exists (entries are zero in larger alocated ContactPoint2D[])*/
                    if (contactsIn[i].normal != Vector2.zero)
                    {
                        //Debug.DrawLine(contactsIn[i].point, contactsIn[i].point + contactsIn[i].normal, Color.yellow, 20);
                        //Debug.DrawLine(contactsIn[i].point, contactsIn[i].point + contactsIn[i].normal * -1, Color.green, 20);
                        //Debug.DrawLine(contactsIn[i].point, contactsIn[i].point + Vector2.down, Color.green, 20);
                        slopeAngle = Vector2.Angle(Vector2.down, contactsIn[i].normal);
                        //debugSlopeAngle = slopeAngle;
                        //debugSlopeAngle = Vector2.Angle(Vector2.up, contactsIn[i].normal * -1);

                        /* Flat Ground */
                        if (slopeAngle == CStats.botAngle)
                        { // == 0 // contactsIn[i].normal.y == -1
                            bot = true;
                        }
                        /* Wall Collision */
                        else if (slopeAngle <= CStats.wallAngleMax && slopeAngle >= CStats.wallAngleMin)
                        {
                            if (contactsIn[i].normal.x < 0)
                            {
                                left = true;
                            }
                            else if (contactsIn[i].normal.x > 0)
                            {
                                right = true;
                            }
                            else
                            {
                                Debug.LogError("ERROR: Invalid Angle.");
                            }
                            slopeDir = (contactsIn[i].normal.x > 0) ? 1 : -1;
                            curWallAngle = slopeAngle;
                        }
                        /* Top Collision*/
                        else if (slopeAngle >= CStats.topAngleMin && slopeAngle <= CStats.topAngleMax)
                        {
                            top = true;
                        }
                        /* Top Slope Collision. */
                        else if (slopeAngle > CStats.wallAngleMax && slopeAngle < CStats.topAngleMin)
                        {
                            topSlope = true;
                            slopeDir = (contactsIn[i].normal.x > 0) ? 1 : -1;
                        }
                        /* Steep Slope Collision. */
                        else if (slopeAngle > CStats.slopeAngleMax && slopeAngle < CStats.topAngleMin)
                        {
                            steepSlope = true;
                            curSteepSlopeAngle = slopeAngle;
                            steepSlopeDir = (contactsIn[i].normal.x > 0) ? 1 : -1;
                        }
                        /* Slope Collision */
                        else
                        { // This is now bot.
                            slope = true;
                            curSlopeAngle = slopeAngle;
                            slopeDir = (contactsIn[i].normal.x > 0) ? 1 : -1;
                        }
                    }

                }
            }
        }



        if (bot || top || left || right || slope || topSlope || steepSlope)
        {
            none = false;
        }
    }

    private void ClearOverlaps()
    {
        none = true;
        top = false;
        bot = false;
        left = false;
        right = false;
        slope = false;
        topSlope = false;
        steepSlope = false;

        objectsTouching.Clear();
    }

    /* -- Debugging Methods: */

    public void printStates()
    {
        print("TOP : " + top);
        print("BOT : " + bot);
        print("LEFT : " + left);
        print("RIGHT : " + right);
        print("SLOPE : " + slope);
        print("NONE : " + none);
    }

    public void printStatesError()
    {
        Debug.LogError("--------- T" + top + " B" + bot + " L" + left + " R" + right + " S" + slope + " N" + none + " TS" + topSlope);
    }

    public void printStatesShort()
    {
        print("--------- T" + top + " B" + bot + " L" + left + " R" + right + " S" + slope + " N" + none);
    }
}