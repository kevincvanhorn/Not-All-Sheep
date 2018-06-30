﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parallax_Base : MonoBehaviour {

    public Transform[] layers;
    public float[] parrallaxScales;
    public float smoothing = 1f;

    private Transform camera;
    private Vector3 prevCamPosition;

    private void Awake()
    {
        camera = Camera.main.transform;
    }

    // Use this for initialization
    void Start () {
        prevCamPosition = camera.position;
        //parrallaxScales = new float[layers.Length];


        /*for(int i = 0; i < layers.Length; i++)
        {
            parrallaxScales[i] = layers[i].position.z * -1;
        }*/
	}
	
	// Update is called once per frame
	void Update () {
        for(int i = 0; i < layers.Length; i++)
        {
            float parrallax = (prevCamPosition.x - camera.position.x) * parrallaxScales[i];
            float backgroundTargetPosX = layers[i].position.x + parrallax;

            Vector3 backgroundTargetPos = new Vector3(backgroundTargetPosX, layers[i].position.y, layers[i].position.z);

            layers[i].position = Vector3.Lerp(layers[i].position, backgroundTargetPos, smoothing + Time.deltaTime);

        }

        prevCamPosition = camera.position;
	}
}