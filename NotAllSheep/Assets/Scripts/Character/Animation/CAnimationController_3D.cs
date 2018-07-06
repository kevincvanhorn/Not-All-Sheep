using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CAnimationController_3D : MonoBehaviour {

    protected PBehaviour character;
    protected int charState= 0; // Idle default
    protected int prevState = -1;
    protected Animator animator;

    private Vector3 LeftFlip, RightFlip;

    // Use this for initialization  
    public void Awake()
    {
        animator = GetComponent<Animator>();
    }

    /* Prerequisites: Start in PBehaviourManager has run. */
    public virtual void Start()
    {
        PBehaviourManager manager = gameObject.GetComponentInParent<PBehaviourManager>();
        character = manager.curBehaviour;

        RightFlip = transform.localScale;
        LeftFlip = new Vector3(transform.localScale.x, transform.localScale.y, transform.localScale.z * -1);

        charState = character.curState.stateID;
    }

    public virtual void Update()
    {
        if (charState != character.curState.stateID) { prevState = charState; } // Only set on change of state.

        charState = character.curState.stateID;

        transform.localScale = (character.directionFacing == 1) ? RightFlip : LeftFlip;

        animator.SetFloat("velocityX", character.velocity.x);
        animator.SetFloat("velocityY", character.velocity.y);
        animator.SetInteger("charState", charState);
        animator.SetInteger("prevState", prevState);

        animator.SetFloat("velocityXAbs", Mathf.Abs(character.velocity.x));

        if ((Player.input.KeyDown_AttackLight)) //TODO: Move to attack class via behaviour manager.
        {
            animator.SetBool("isAttacking", true);
            StartCoroutine(attackDelay());
        }
        //Debug.LogError(IsJumpFromGrounded());
    }

    private IEnumerator attackDelay()
    {
        yield return new WaitForSeconds(1f);
        animator.SetBool("isAttacking", false);
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
