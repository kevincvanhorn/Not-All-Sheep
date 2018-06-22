using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CAnimationController_WhiteOblivion : MonoBehaviour
{

    private PBehaviour character;
    private Animator animator;
    public ParticleSystem draggingParticle;

    // Use this for initialization
    private void Awake()
    {
        animator = GetComponent<Animator>();
        //character = GetComponent<CharacterBase>();
        character = gameObject.GetComponentInParent<PBehaviour>();
    }

    public void Update()
    {
        animator.SetFloat("Speed", character.velocity.x);
        Debug.Log(character.velocity.x);

        if(character.velocity.x <= 0)
        {
            draggingParticle.enableEmission = false;
        }
        else
        {
            draggingParticle.enableEmission = true;
        }
    }

}
