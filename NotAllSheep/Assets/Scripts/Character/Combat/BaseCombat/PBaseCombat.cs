 using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* The base combat configuration with a set of attacks (as states). */
public class PBaseCombat : PBehaviour {

    //LayerMask enemyLayer = (int)CollisionLayers.Enemy;

    //public new PAttackState curState;

    /* Declare Attacks for this Configuration: */
    public PBaseCombat_Slash SSlash;
    public PBaseCombat_Attack SDownAttack;

    /* Declare Prefabs for Attacks: */
    public PAttackInstance SSlash_prefab;

    public override void Awake () {
        base.Awake();

        /* Get components: */
        pInputManager = GetComponent<PInputManager>();
    }

    public override void OnStart()
    {
        base.OnStart();

        /* Create Attacks: */
        SSlash = new PBaseCombat_Slash(this, SSlash_prefab);
    }

    public override void OnFixedUpdate()
    {
        //base.OnFixedUpdate();
        /*if (Player.input.KeyDown_AttackLight)
        {
            SSlash.DoHit();
        }*/
        UpdateAttackInstances();
    }

    /* Called via CAnimationController*/
    public void DoSlashLight()
    {
        SSlash.DoHit();
    }

    /* @damage is subtracted from health. */
    public void OnHit(float damage, GameObject attacker)
    {
        Player.health -= damage;
        if (Player.health <= 0)
        {
            Player.health = 0;
            Player.Instance.OnDeath();
        }
    }

    private void UpdateAttackInstances()
    {
        SSlash.OnFixedUpdate();
    }
}
