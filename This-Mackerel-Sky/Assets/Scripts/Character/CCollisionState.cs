using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CCollisionState : MonoBehaviour
{

    public LayerMask collisionLayer;
    public float maxAngle = 80;
    public float curSlopeAngle = 0;
    public float curWallAngle = 0;
    public float curSteepSlopeAngle = 0;

    public float debugX = 0;
    public float debugY = 0;
    public float debugSlopeAngle = 0;
    public float debugSlopeAngle2 = 0;
    public int slopeDir = 1;

    private Collider2D collider;
    private ContactFilter2D contactFilter = new ContactFilter2D();

    public bool none;
    public bool top;
    public bool bot;
    public bool left;
    public bool right;
    public bool slope;
    public bool topSlope;
    public bool steepSlope;

    public bool None { get { return none; } }
    public bool Top { get { return top; } }
    public bool Bot { get { return bot; } }
    public bool Left { get { return left; } }
    public bool Right { get { return right; } }
    public bool Slope { get { return slope; } }
    public bool TopSlope { get { return topSlope; } }
    public bool SteepSlope { get { return steepSlope; } }

    // Use this for initialization
    void Start()
    {
        collider = GetComponent<Collider2D>();
        contactFilter.layerMask = collisionLayer;
    }

    private void FixedUpdate()
    {
        ClearOverlaps();
        CheckOverlaps();
        //printStatesShort();
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
    }

    public void CheckOverlaps()
    {
        Collider2D[] collidersTouching = new Collider2D[4];
        Physics2D.OverlapCollider(collider, contactFilter, collidersTouching);
        float slopeAngle;
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
                                Debug.Log("ERROR: Invalid Angle.");
                            }
                            slopeDir = (contactsIn[i].normal.x > 0) ? 1 : -1;
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
                            slopeDir = (contactsIn[i].normal.x > 0) ? 1 : -1;
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
        Debug.LogError("--------- T" + top + " B" + bot + " L" + left + " R" + right + " S" + slope + " N" + none + " TS"+topSlope);
    }

    public void printStatesShort()
    {
        print("--------- T" + top + " B" + bot + " L" + left + " R" + right + " S" + slope + " N" + none);
    }
}