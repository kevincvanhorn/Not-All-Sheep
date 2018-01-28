using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Designed to interact with the CameraZoom class to request changes in the orthographic camera size.
   - Calls Functions, does not modify the values directly. */
public class CameraTrigger : MonoBehaviour {

    public CameraZoomType zoomType;

    public CameraZoom camera; // **Set in Editor
    private float prevSize;
    public float zoomSize = 20;

    public enum CameraZoomType
    {
        ZoomTo_Default,
        ZoomTo_WhileContained,
        ZoomTo_Value
    };

    private void Start()
    {
        if (!camera)
        {
            Debug.LogError("ERROR: No camera set/found.");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<CharacterBase>())
        {
            if(zoomType == CameraZoomType.ZoomTo_Default)
            {
                camera.SetDefaultSize();
            }
            else if (zoomType == CameraZoomType.ZoomTo_Value || zoomType == CameraZoomType.ZoomTo_WhileContained)
            {
                prevSize = camera.curSize;
                camera.RequestSizeChange(zoomSize);
            }   
            //prevSize = camera.orthographicSize;
            //camera.orthographicSize = zoomSize;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<CharacterBase>())
        {
            if (zoomType == CameraZoomType.ZoomTo_WhileContained)
            {
                camera.RequestSizeChange(prevSize);
            }
            //camera.SetDefaultSize();
            //camera.orthographicSize = prevSize;
        }
    }
}
