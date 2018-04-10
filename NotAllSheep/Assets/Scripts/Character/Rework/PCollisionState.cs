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
    public Vector2 wallHitNormal;
    public Vector3 debugWallHitLoc;

    /* OnCollisionEnter Emulation: */
    public HashSet<CollisionType> enterCollisionTypes = new HashSet<CollisionType>(); // Set of collisions that entered this fixed frame.
    public HashSet<CollisionType> prevCollisionTypes = new HashSet<CollisionType>();  // Set of collisions present in the previous fixed frame.
    public HashSet<CollisionType> curCollisionTypes = new HashSet<CollisionType>();
    int collisionTypeCount = CollisionType.GetNames(typeof(CollisionType)).Length;

    /* Collision Variables: */
    private ContactFilter2D contactFilter = new ContactFilter2D();
    private new Collider2D collider; // The collider of the player.

    /* Accessor State Variables: */
    public bool None { get { return curCollisionTypes.Count == 0; } }
    public bool Top { get { return curCollisionTypes.Contains(CollisionType.Top); } }
    public bool Bot { get { return curCollisionTypes.Contains(CollisionType.Bot); } }
    public bool Left { get { return curCollisionTypes.Contains(CollisionType.Left); } }
    public bool Right { get { return curCollisionTypes.Contains(CollisionType.Right); } }
    public bool Slope { get { return curCollisionTypes.Contains(CollisionType.Slope); } }
    public bool TopSlope { get { return curCollisionTypes.Contains(CollisionType.TopSlope); } }
    public bool SteepSlope { get { return curCollisionTypes.Contains(CollisionType.SteepSlope); } }
    public bool Grounded { get { return curCollisionTypes.Contains(CollisionType.Bot) || curCollisionTypes.Contains(CollisionType.Slope); } }

    /* Accessor Enter State Variables: */
    public bool None_Enter { get { return enterCollisionTypes.Count == 0; } }
    public bool Top_Enter { get { return enterCollisionTypes.Contains(CollisionType.Top); } }
    public bool Bot_Enter { get { return enterCollisionTypes.Contains(CollisionType.Bot); } }
    public bool Left_Enter { get { return enterCollisionTypes.Contains(CollisionType.Left); } }
    public bool Right_Enter { get { return enterCollisionTypes.Contains(CollisionType.Right); } }
    public bool Slope_Enter { get { return enterCollisionTypes.Contains(CollisionType.Slope); } }
    public bool TopSlope_Enter { get { return enterCollisionTypes.Contains(CollisionType.TopSlope); } }
    public bool SteepSlope_Enter { get { return enterCollisionTypes.Contains(CollisionType.SteepSlope); } }
    public bool Grounded_Enter { get { return enterCollisionTypes.Contains(CollisionType.Bot) || enterCollisionTypes.Contains(CollisionType.Slope); } }

    /* Debugging  Variables: */

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
        SetEnterCollisions();
        
        //Debugging:
        //printStatesShort();
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
                ContactPoint2D[] contactsIn = new ContactPoint2D[8]; // 2 when side collides (each corner) || 1 when on slope
                coll.GetContacts(contactsIn);

                /* Call Collider Enter Functions */
                for (int i = 0; i < contactsIn.Length; i++)
                {
                    /* If contact exists (entries are zero in larger alocated ContactPoint2D[])*/
                    if (contactsIn[i].normal != Vector2.zero)
                    {
                        slopeAngle = Vector2.Angle(Vector2.down, contactsIn[i].normal);

                        /* Flat Ground */
                        if (slopeAngle == CStats.botAngle)
                        { // == 0 // contactsIn[i].normal.y == -1
                            curCollisionTypes.Add(CollisionType.Bot);
                        }
                        /* Wall Collision */
                        else if (slopeAngle <= CStats.wallAngleMax && slopeAngle >= CStats.wallAngleMin)
                        {
                            if (contactsIn[i].normal.x < 0)
                            {
                                curCollisionTypes.Add(CollisionType.Left);
                            }
                            else if (contactsIn[i].normal.x > 0)
                            {
                                curCollisionTypes.Add(CollisionType.Right);
                            }
                            else
                            {
                                Debug.LogError("ERROR: Invalid Angle.");
                            }
                            wallHitNormal = contactsIn[i].normal;
                            wallHitNormal *= -1;
                            debugWallHitLoc = contactsIn[i].point;
                            slopeDir = (contactsIn[i].normal.x > 0) ? 1 : -1;
                            curWallAngle = slopeAngle;
                        }
                        /* Top Collision*/
                        else if (slopeAngle >= CStats.topAngleMin && slopeAngle <= CStats.topAngleMax)
                        {
                            curCollisionTypes.Add(CollisionType.Top);
                        }
                        /* Top Slope Collision. */
                        else if (slopeAngle > CStats.wallAngleMax && slopeAngle < CStats.topAngleMin)
                        {
                            curCollisionTypes.Add(CollisionType.TopSlope);
                            slopeDir = (contactsIn[i].normal.x > 0) ? 1 : -1;
                        }
                        /* Steep Slope Collision. */
                        else if (slopeAngle > CStats.slopeAngleMax && slopeAngle < CStats.topAngleMin)
                        {
                            curCollisionTypes.Add(CollisionType.SteepSlope);
                            curSteepSlopeAngle = slopeAngle;
                            steepSlopeDir = (contactsIn[i].normal.x > 0) ? 1 : -1;
                        }
                        /* Slope Collision */
                        else
                        { // This is now bot.
                            curCollisionTypes.Add(CollisionType.Slope);
                            curSlopeAngle = slopeAngle;
                            slopeDir = (contactsIn[i].normal.x > 0) ? 1 : -1;
                        }
                    }
                }
            }
        }
    }

    private void ClearOverlaps()
    {
        curCollisionTypes.Clear();
        objectsTouching.Clear();
        enterCollisionTypes.Clear();
    }

    /* Assigns appropriate values to the collisions entering just this fixed frame Hash Set
     * Precondition: enterCollisionTypes is empty, curCollisionTypes is up to date, prev is last frame.
     */
    private void SetEnterCollisions()
    {
        /* Set collisions unique to this frame. */
        enterCollisionTypes.UnionWith(curCollisionTypes);
        enterCollisionTypes.ExceptWith(prevCollisionTypes);

        /* Set previous collisions. */
        prevCollisionTypes.Clear();
        prevCollisionTypes.UnionWith(curCollisionTypes);
    }

    /* -- Debugging Methods: */

    public void printStates()
    {
        print("TOP : " + Top);
        print("BOT : " + Bot);
        print("LEFT : " + Left);
        print("RIGHT : " + Right);
        print("SLOPE : " + Slope);
        print("NONE : " + None);
    }

    public void printStatesError()
    {
        Debug.LogError("--------- T" + Top + " B" + Bot + " L" + Left + " R" + Right + " S" + Slope + " N" + None + " TS" + TopSlope);
    }

    public void printStatesWarning()
    {
        Debug.LogWarning("--------- T" + Top + " B" + Bot + " L" + Left + " R" + Right + " S" + Slope + " N" + None + " TS" + TopSlope);
    }

    public void printStatesShort()
    {
        print("--------- T" + Top + " B" + Bot + " L" + Left + " R" + Right + " S" + Slope + " N" + None);
    }
}

public enum CollisionType
{
    Top,
    Bot,
    Left,
    Right,
    Slope,
    TopSlope,
    SteepSlope
};