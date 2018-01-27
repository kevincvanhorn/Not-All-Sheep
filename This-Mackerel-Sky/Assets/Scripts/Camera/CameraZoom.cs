using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Should be the sole class in charge of the main camera zoom/size.
    - CameraTriggers request changes using the provided functions.
    - Smoothing happens here (only!) using Coroutines. 
    - Avoid using update b/c only triggers are starting events.*/
public class CameraZoom : MonoBehaviour {

    public float defaultSize;
    private float curSize;
    private float targetSize;

    private Camera camera;

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
        while (Mathf.Abs(curSize) <= Mathf.Abs(targetSize))
        {
            Debug.LogError("WHOAOHHH "+ targetSize);
            Mathf.SmoothDamp(curSize, targetSize, ref smoothVel, smoothTime);
            camera.orthographicSize = targetSize;
            yield return new WaitForEndOfFrame();
        }

        yield return null;
    }
}
