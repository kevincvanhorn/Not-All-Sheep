using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Start & Update for classes should only come from UpdateDriver for smooth motion and determinism.
 * Awake/Destory not called for others in this class bc the order doesn't matter for initialization.  */
public class UpdateDriver : MonoBehaviour {

    private static UpdateDriver _instance = null;
    public static UpdateDriver Instance { get { return _instance; } }

    /* Awake for this Driver script. NOT awake for other classes connected to this. */
    private void Awake()
    {
        /* Enforce Singleton: */
        if (_instance != null && _instance != this)
            Destroy(this.gameObject);
        else
            _instance = this;
    }

    /* Different instance each scene. */
    private void OnDestroy() {if (this == _instance) { _instance = null; } }

    void Start()
    {
        PBehaviourManager.Instance.OnStart(); // Update PlayerMvt
        PCombatManager.Instance.OnStart();
    }

    void FixedUpdate()
    {
        PBehaviourManager.Instance.OnFixedUpdate(); // Update PlayerMvt.
        PCombatManager.Instance.OnFixedUpdate();
        Player.input.OnFixedUpdate(); // Resets all keyDown events in input controller.

        //CameraFollow cam = GameObject.FindObjectOfType<CameraFollow>();
        //cam.OnFixedUpdate();
    }

    // Update is called once per frame
    void Update () {
		
	}

    void LateUpdate()
    {
        
    }
}
