using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CAnimationController : MonoBehaviour {

    private SpriteRenderer spriteRenderer;
    private CharacterBase character;
    int charState;
    private Animator animator;

    // Use this for initialization
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        //character = GetComponent<CharacterBase>();
        character = gameObject.GetComponentInParent<CharacterBase>();
    }

    public void Update()
    {
        charState = (int)character.fsm.State;

        bool flipSprite = (spriteRenderer.flipX ? (character.directionFacing == 1) : (character.directionFacing == -1));
        if (flipSprite)
        {
            spriteRenderer.flipX = !spriteRenderer.flipX;
        }

        animator.SetBool("running", (Mathf.Abs(character.velocity.x) > 0) && (character.collisionState.Bot || character.collisionState.Slope));
        animator.SetBool("falling", character.collisionState.None);
        //animator.SetFloat("velocityX", (Mathf.Abs(character.velocity.x)));
        animator.SetFloat("velocityY", character.velocity.y);
        animator.SetBool("isGrounded", (character.collisionState.Bot || character.collisionState.Slope));
    }
}
