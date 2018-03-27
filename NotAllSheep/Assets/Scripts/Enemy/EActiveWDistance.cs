using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Deactivates an Object if out of range of the player by a certain distance. */
public class EActiveWDistance : MonoBehaviour {
    /*
    float deactivateDist = 100;
    float waitTime = 2; // sec between each distance check.
    bool _enabled;
    bool isParentActive;

    Transform playerTrans;

    private void OnEnable()
    {
        playerTrans = GameObject.Find("Player").transform;
        _enabled = true;
        CheckAndSet();
        StartCoroutine(CheckDistance());
    }

    IEnumerator CheckDistance()
    {
        while (_enabled)
        {
            CheckAndSet();
            yield return new WaitForSeconds(waitTime);
        }
        
    }

    void CheckAndSet()
    {
        float dist = Vector3.Distance(transform.position, playerTrans.position);
        Debug.LogError(dist);
        if (dist <= deactivateDist){
            gameObject.SetActive(true);
            isParentActive = true;
        }
        else
        {
            gameObject.SetActive(false);
            isParentActive = false;
        }
    }

    void OnDisable()
    {
        _enabled = false;
        StopCoroutine(CheckDistance());
    }
    */
}
