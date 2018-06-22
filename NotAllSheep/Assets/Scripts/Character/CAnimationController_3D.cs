using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CAnimationController_3D : MonoBehaviour {

    protected PBehaviour character;
    protected int charState= 0; // Idle default
    protected int prevState;
    protected Animator animator;

    private Vector3 LeftFlip, RightFlip;

    // Use this for initialization  
    public void Awake()
    {
        animator = GetComponent<Animator>();
    }

    /* Prerequisites: Start in PBehaviourManager has run. */
    public void Start()
    {
        PBehaviourManager manager = gameObject.GetComponentInParent<PBehaviourManager>();
        character = manager.curBehaviour;

        RightFlip = transform.localScale;
        LeftFlip = new Vector3(transform.localScale.x, transform.localScale.y, transform.localScale.z * -1);
    }

    public virtual void Update()
    {
        prevState = charState;
        charState = character.curState.stateID;

        //        bool canFlip = (flipX ? (character.directionFacing == 1) : (character.directionFacing == -1));
        //bool flipSprite = (flipX ? (character.directionFacing == -1) : (character.directionFacing == 1));



        //Debug.LogError("flipX " + flipX + " flipSprite " + flipSprite + " directionFacing: " + character.directionFacing);
        /*
        if (flipSprite) {
            gameObject.transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, transform.localScale.z * character.directionFacing);
            flipX = !flipX;
        }*/
        transform.localScale = (character.directionFacing == 1) ? RightFlip : LeftFlip;

        animator.SetFloat("velocityX", character.velocity.x);
        animator.SetFloat("velocityY", character.velocity.y);
        animator.SetInteger("charState", charState);
        animator.SetInteger("prevState", prevState);

        animator.SetFloat("velocityXAbs", Mathf.Abs(character.velocity.x));


        //Debug.LogError(IsJumpFromGrounded());
    }

    public void AnimTrigger()
    {
        animator.SetTrigger("jumpTrigger");
    }

    public bool IsJumpFromGrounded()
    {
        return animator.GetCurrentAnimatorStateInfo(0).IsName("Running.JumpFromGround");
    }

    
}
