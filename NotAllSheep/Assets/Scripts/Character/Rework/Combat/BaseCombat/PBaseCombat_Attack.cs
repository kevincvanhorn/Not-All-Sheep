using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* A base for Attacks spawned from the player while in the BaseCombat Configuration. */
public class PBaseCombat_Attack{

    public PBaseCombat config;// The parent configuration that the attack belongs to - contains all of the shared variables for states in a behaviour.

    //TODO: Make these Hashsets 
    public PAttackInstance attackPrefab;
    public PAttackInstance attackInstance;

    /* Constructor: PBaseCombat is the monobehaviour, need to pass it as config for transform etc. */
    public PBaseCombat_Attack(PBaseCombat config, PAttackInstance attackPrefab)
    {
        this.config = config;
        this.attackPrefab = attackPrefab;
    }

    public virtual void DoHit()
    {

    }

    public virtual void OnFixedUpdate()
    {
        
    }
}
