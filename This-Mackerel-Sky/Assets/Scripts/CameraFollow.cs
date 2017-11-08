using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour {
    public Transform player;
    public Vector3 offset;
    float width, height, playerHeight;
    Vector3 worldDimensions, center, focalpoint;
    Character myPlayer;

    Transform cam;

	// Use this for initialization
	void Start () {
        GameObject myPlayerObj = GameObject.Find("Player");
        if(myPlayerObj != null){
            myPlayer = myPlayerObj.GetComponent<Character>();
        }

        /* Camera Properties*/
        worldDimensions = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, -10));
        width = worldDimensions.x;
        height = worldDimensions.y;

        cam = this.transform;
        player = myPlayer.transform;

        print(cam.position + " " + player.position);

        // TODO: Current Offset adds additional values somehow to the offset ontop of the below calculations.
        offset.x = 0;//offset.x = cam.position.x - player.position.x; // Allow for player starting at any point to maintain displacement.
        offset.y = 0;//offset.y = cam.position.y - player.position.y;
        offset.z = 0;
	}
	
	// Update is called once per frame
	void Update () {
        Vector3 camPos = cam.position; // Instantiating can be costly, copy the struct and reapply to the object.

        camPos.x = player.position.x + offset.y;
        camPos.y = player.position.y + offset.y;

        cam.position = camPos;
	}




}

