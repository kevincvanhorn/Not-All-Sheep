using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Designed to interact with the CameraZoom class to request changes in the orthographic camera size.
   - Calls Functions, does not modify the values directly. */
public class CameraTrigger : MonoBehaviour {

    public bool enterZoom;
    public bool maintainZoom; // if false then set back when leave
    public bool setDefault;

    public CameraZoom camera; // **Set in Editor
    private float prevSize;
    public float zoomSize = 20;

    private void Start()
    {

        setDefault = false;
        zoomSize = 20;

        if (!camera)
        {
            Debug.LogError("ERROR: No camera set/found.");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<CharacterBase>())
        {
            camera.RequestSizeChange(zoomSize);
            //prevSize = camera.orthographicSize;
            //camera.orthographicSize = zoomSize;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<CharacterBase>())
        {
            camera.SetDefaultSize();
            //camera.orthographicSize = prevSize;
        }
    }
}
