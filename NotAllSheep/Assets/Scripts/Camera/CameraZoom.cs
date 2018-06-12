using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Should be the sole class in charge of the main camera zoom/size.
    - CameraTriggers request changes using the provided functions.
    - Smoothing happens here (only!) using Coroutines. 
    - Avoid using update b/c only triggers are starting events.*/
public class CameraZoom : MonoBehaviour {

    public float defaultSize;
    public float curSize;
    private float targetSize;

    private new Camera camera;

    // Calculation Variables:
    private float smoothVel;
    public float smoothTime;
    public float smoothSpeed;

	// Use this for initialization
	void Start () {
        camera = GetComponent<Camera>();

        /* Set Defaults*/
        defaultSize = 14;
        curSize = defaultSize;
        targetSize = curSize;

        smoothSpeed = 10;
        smoothTime = 0.1f;

        /* Set Camera*/
        camera.orthographicSize = curSize;
    }

    // Immediately Sets the camera to a specific size.
    public void SnapToSize(float snapSize)
    {
        camera.orthographicSize = snapSize;
    }

    public void SetDefaultSize()
    {
        RequestSizeChange(defaultSize);
    }

    public void RequestSizeChange(float targetSize)
    {
        this.targetSize = targetSize;
        //smoothTime = Mathf.Abs(curSize - targetSize) / smoothSpeed ;
        StartCoroutine(ChangeSizeSmooth());
    }
    IEnumerator ChangeSizeSmooth()
    {
        while (Mathf.Abs(targetSize - curSize) > 0.01f)
        {
            Debug.LogError(curSize + " WHOAOHHH "+ targetSize); 
            curSize = Mathf.SmoothDamp(curSize, targetSize, ref smoothVel, smoothTime);
            camera.orthographicSize = curSize;
            yield return new WaitForEndOfFrame();
        }

        yield return null;
    }
}
