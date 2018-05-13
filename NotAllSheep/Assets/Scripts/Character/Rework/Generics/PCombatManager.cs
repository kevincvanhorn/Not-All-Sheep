using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CollisionLayers
{
    Enemy = 11
}

public class PCombatManager : MonoBehaviour
{
    private PBehaviour curConfiguration;

    /* Combat Configurations: */
    private PBehaviour configuration_BaseCombat;
    private PBehaviour configuration_FerretCombat;

    //LayerMask enemyLayer = (int)CollisionLayers.Enemy;

    /* Singleton: */
    private static PCombatManager _instance = null;
    public static PCombatManager Instance { get { return _instance; } }
    private void Awake()
    {
        /* Enforce Singleton: */
        if (_instance != null && _instance != this)
            Destroy(this.gameObject);
        else
            _instance = this;
    }
    /* Different instance each scene. */
    private void OnDestroy() { if (this == _instance) { _instance = null; } }

    /* Called from UpdateDriver. */
    public void OnStart()
    {
        /* Create Configurations: */
        configuration_BaseCombat = gameObject.GetComponent<PBaseCombat>();
        CallConfigOnStarts();

        /* Set current Configuration: */
        curConfiguration = configuration_BaseCombat;
    }

    /* Called from UpdateDriver. */
    public void OnFixedUpdate()
    {
        curConfiguration.OnFixedUpdate();
    }

    /* Responsible for termination and allocation of new variables upon state switching.*/
    public void SwitchBehaviour(PBehaviour nextBehaviour)
    {

    }

    private void CallConfigOnStarts()
    {
        configuration_BaseCombat.OnStart();
    }
}
