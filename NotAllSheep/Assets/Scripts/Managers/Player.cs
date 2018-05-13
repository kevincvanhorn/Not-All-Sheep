using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Player Manager: use sparingly - try to keep variables local to behaviours. */
public class Player : MonoBehaviour
{

    private float _health = 1;
    private sbyte _directionFacing = 1;
    //private Transform _transform;

    public PInputManager _input;
    

    /*Accessor Variables: for easy access e.g: Player.directionFacing. */
    public static float health { get { return _instance._health; } set { _instance._health = value; } }
    public static sbyte directionFacing { get { return _instance._directionFacing; } set { _instance._directionFacing = value; } }
    public static PInputManager input { get { return _instance._input; } set { _instance._input = value; } }
    //public static new Transform transform { get { return _instance._transform; }}

    /*Singleton Initialization: */
    private static Player _instance = null;
    public static Player Instance { get { return _instance; } }
    private void Awake()
    {
        /* Enforce Singleton: */
        if (_instance != null && _instance != this)
            Destroy(this.gameObject);
        else
            _instance = this;

        _input = GetComponent<PInputManager>();
    }
    /* Different instance each scene. */
    private void OnDestroy() { if (this == _instance) { _instance = null; } }


    public void OnDeath()
    {

    }

}
