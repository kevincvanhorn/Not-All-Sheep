using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CCollisionState : MonoBehaviour {

    public LayerMask collisionLayer;
    public float maxAngle = 80;
    public float curSlopeAngle = 0;
    public float curWallAngle = 0;

    public float debugX = 0;
    public float debugY = 0;

    private Collider2D collider;
    private ContactFilter2D contactFilter = new ContactFilter2D();

    public bool none;
    public bool top;
    public bool bot;
    public bool left;
    public bool right;
    public bool slope;
    public bool topSlant;

    public bool None { get { return none; } }
    public bool Top { get { return top; }  }
    public bool Bot { get { return bot; } }
    public bool Left { get { return left; } }
    public bool Right { get { return right; } }
    public bool Slope { get { return slope; } }
    public bool topSlant { get { return slope; } }

    // Use this for initialization
    void Start () {
        collider = GetComponent<Collider2D>();
        contactFilter.layerMask = collisionLayer;
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    private void FixedUpdate() {
        ClearOverlaps();
        CheckOverlaps();
        //printStatesShort();
    }

    private void ClearOverlaps() {
        none = true;
        top = false;
        bot = false;
        left = false;
        right = false;
        slope = false;
    }

    public void CheckOverlaps() {
        Collider2D[] collidersTouching = new Collider2D[4];      
        Physics2D.OverlapCollider(collider, contactFilter, collidersTouching);
        float slopeAngle;
        foreach (Collider2D coll in collidersTouching) {
            
            if (coll != null) {
                //Debug.LogError(coll);

                ContactPoint2D[] contactsIn = new ContactPoint2D[8]; // 2 when side collides (each corner) || 1 when on slope
                coll.GetContacts(contactsIn);

                /* Call Collider Enter Functions */
                for (int i = 0; i < contactsIn.Length; i++) {
                    /* If contact exists (entries are zero in larger alocated ContactPoint2D[])*/
                    if (contactsIn[i].normal != Vector2.zero) {
                        slopeAngle = Vector2.Angle(contactsIn[i].normal, Vector2.down);
                        
                        /* Wall Collision */
                        if(slopeAngle < 91 && slopeAngle > 89) {
                            if(contactsIn[i].normal.x < 0) {
                                left = true;
                            }
                            if (contactsIn[i].normal.x > 0) {
                                right = true;
                            }
                        }
                        //Horizontal Collision
                        else if (slopeAngle >) { // contactsIn[i].normal.y == 0
                            if (contactsIn[i].normal.x > 0) { // contactsIn[i].normal.x == 1
                                right = true;
                            }
                            else if (contactsIn[i].normal.x < 0) { // contactsIn[i].normal.x == -1
                                left = true;
                            }
                        /* Slope Collision */
                            else {
                            slope = true;
                            curSlopeAngle = slopeAngle;
                        }


                        //curSlopeAngle = slopeAngle; // DEBUG
                        //debugX = contactsIn[i].normal.x; //DEBUG
                        //debugY = contactsIn[i].normal.y; //DEBUG
                        /*if (contactsIn[i].normal.x == 0) { // contactsIn[i].normal.x == 0
                            if (contactsIn[i].normal.y > 0.9) { // 12.28.16b03 (== 1)
                                top = true;
                            }
                            else if (contactsIn[i].normal.y < -.99) { // contactsIn[i].normal.y == -1
                                bot = true;
                            }
                        }
                        //Horizontal Collision
                        else if (slopeAngle > maxAngle) { // contactsIn[i].normal.y == 0
                            if (contactsIn[i].normal.x > 0) { // contactsIn[i].normal.x == 1
                                right = true;
                            }
                            else if (contactsIn[i].normal.x < 0) { // contactsIn[i].normal.x == -1
                                left = true;
                            }
                        }*/
                    }
                }
            }
        }
        

        
        if(bot || top || left || right || slope) {
            none = false;
        }
    }

    public void printStates() {
        print("TOP : "+ top);
        print("BOT : " + bot);
        print("LEFT : " + left);
        print("RIGHT : " + right);
        print("SLOPE : " + slope);
        print("NONE : " + none);
    }

    public void printStatesError() {
        Debug.LogError("--------- T" + top + " B" + bot + " L" + left + " R" + right + " S" + slope + " N" + none);
    }

    public void printStatesShort() {
        print("--------- T" + top + " B"+bot+" L"+left+ " R"+right + " S"+slope + " N"+none);
    }
}
