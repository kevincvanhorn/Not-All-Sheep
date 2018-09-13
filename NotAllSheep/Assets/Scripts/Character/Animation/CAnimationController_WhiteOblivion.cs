using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CAnimationController_WhiteOblivion : CAnimationController_3D
{
    public ParticleSystem draggingParticle;
    private PBaseCombat combatConfig;


    public override void Start()
    {
        base.Start();
        if (PCombatManager.Instance)
        {
            combatConfig = ((PBaseCombat)(PCombatManager.Instance.curConfiguration));
        }
    }

    public override void Update()
    {
        base.Update();

        //ParticleSystem.EmissionModule em = m_particleSystem.emission;
        //em.enabled = true;

        if (character.velocity.x <= 0)
        {
            draggingParticle.enableEmission = false;
        }
        else
        {
            draggingParticle.enableEmission = false;
        }
    }


    private void PSlashEvent()
    {
        Debug.LogError("SLASH");
        combatConfig.DoSlashLight();
    }
}
