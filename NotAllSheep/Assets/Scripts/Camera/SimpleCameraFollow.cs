using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleCameraFollow : MonoBehaviour {

    private void FixedUpdate()
    {
        transform.position = Player.Instance.transform.position;
    }
}
