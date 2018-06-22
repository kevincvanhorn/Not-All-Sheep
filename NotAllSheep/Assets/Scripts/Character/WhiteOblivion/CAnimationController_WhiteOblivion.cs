using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CAnimationController_WhiteOblivion : CAnimationController_3D
{
    public ParticleSystem draggingParticle;

    public void Update()
    {
        base.Update();

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
