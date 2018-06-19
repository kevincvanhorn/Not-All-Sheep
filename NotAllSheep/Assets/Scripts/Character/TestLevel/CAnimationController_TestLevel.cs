using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CAnimationController_TestLevel : MonoBehaviour
{

    private PScytheMovement character;
    private Animator animator;
    public ParticleSystem draggingParticle;

    // Use this for initialization
    private void Awake()
    {
        animator = GetComponent<Animator>();
        //character = GetComponent<CharacterBase>();
        character = gameObject.GetComponentInParent<PScytheMovement>();
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
