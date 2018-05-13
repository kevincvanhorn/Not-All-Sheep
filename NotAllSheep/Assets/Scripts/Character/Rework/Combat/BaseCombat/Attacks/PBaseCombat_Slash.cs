using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* A class for spawning a specific attack in the PBaseCombat attack configuration */
public class PBaseCombat_Slash : PBaseCombat_Attack{

    //public GameObject[] attackInstances; // TODO make these hashsets
    //WORKING: 5.13.18: Making Hashsets in PBaseCombat_Attack to call OnFixedUpdate for changing the scale& transform of each instance.

    public PBaseCombat_Slash(PBaseCombat config, PAttackInstance attackPrefab) : base(config, attackPrefab)
    {
        ;
    }

    public override void DoHit()
    {
        base.DoHit();

        // Spawn Attack Instance:
        attackInstance = GameObject.Instantiate(attackPrefab, config.transform.position + new Vector3(3 * Player.directionFacing, 1), attackPrefab.transform.rotation);

        ((PBaseCombat_Slash_Instance)attackInstance).positionPrev = Player.Instance.transform.position;
        /*Flip direction w/ dirFacing*/
        Vector3 tempAngle = attackInstance.transform.eulerAngles;
        tempAngle.y = (Player.directionFacing == -1) ? 0 : 180;
        attackInstance.transform.eulerAngles = tempAngle;
    }

    public override void OnFixedUpdate()
    {
        base.OnFixedUpdate();
        if (attackInstance) {
            attackInstance.OnFixedUpdate();
        }
    }
}
