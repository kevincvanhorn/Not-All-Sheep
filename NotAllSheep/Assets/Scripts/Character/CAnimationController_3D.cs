using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CAnimationController_3D : MonoBehaviour {

    private PBehaviour character;
    int charState= 2; // Idle default
    int prevState;
    private Animator animator;
    private bool flipX = true;

    // Use this for initialization  
    private void Awake()
    {
        animator = GetComponent<Animator>();
        character = gameObject.GetComponentInParent<PBehaviour>();
    }

    public void Update()
    {
        prevState = charState;
        charState = character.curState.stateID;

        bool flipSprite = (flipX ? (character.directionFacing == 1) : (character.directionFacing == -1));
        if (flipSprite)
        {
            gameObject.transform.Rotate(0, character.directionFacing, 0);
            //spriteRenderer.flipX = !spriteRenderer.flipX;
        }

        animator.SetFloat("velocityY", character.velocity.y);
        animator.SetInteger("charState", charState);
        animator.SetInteger("prevState", prevState);

        Debug.LogError(IsJumpFromGrounded());
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
