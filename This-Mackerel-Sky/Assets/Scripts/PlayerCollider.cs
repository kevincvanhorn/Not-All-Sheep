using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCollider : MonoBehaviour {
    public bool isTouching; // True if collider is touching an object.

    public delegate void TriggerEvent();
    public event TriggerEvent OnEdgeEnter;
    public event TriggerEvent OnEdgeExit;

    // Use this for initialization
    void Start() {
        isTouching = false;
    }

    // Update is called once per frame
    void Update() {
    }

    /** Called on Player collision with object. **/
    void OnTriggerEnter2D(Collider2D coll) {
        OnEdgeEnter();
        isTouching = true;
        //print("Trigger from: "+this.name); 
    }

    /** Called on Player collision with object. **/
    void OnTriggerExit2D() {
        OnEdgeExit();
        isTouching = false;
    }
}
