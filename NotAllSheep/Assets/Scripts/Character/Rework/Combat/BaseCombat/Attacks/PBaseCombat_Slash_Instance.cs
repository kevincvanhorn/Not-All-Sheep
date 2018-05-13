using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PBaseCombat_Slash_Instance : PAttackInstance
{
    public Vector3 offset;
    public Vector3 positionPrev;
    //public float dragFactor = 1f; // 1 = translate exactly with player, 0 = translate none 

    // Inherited: LayerMask layer
    // Inherited: bool destroyOnAwake
    // Inherited: float destroyDelay
    // Inherited: float damage

    public override void OnFixedUpdate()
    {
        base.OnFixedUpdate();

        /* Move translate attack with player. */
        //transform.localScale *= Player.directionFacing;
        //offset = new Vector3(3 * Player.directionFacing, 1);
        //transform.position = Player.Instance.transform.position + offset;
        //Debug.LogError(Player.Instance.transform.position - positionPrev);
        transform.Translate(Player.Instance.transform.position - positionPrev, Space.World);
        positionPrev = Player.Instance.transform.position;
    }


}
