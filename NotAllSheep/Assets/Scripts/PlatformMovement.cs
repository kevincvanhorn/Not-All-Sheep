using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformMovement : MonoBehaviour {

    public bool isActive = true;
    public float speed = 0;
    public float moveDist;

    private float waitTime = 0;
    private float dir = 1;

    public CharacterBase player;

    private void Awake()
    {
        player = GameObject.FindObjectOfType<CharacterBase>();
    }

    // Use this for initialization
    void Start () {
        StartCoroutine(SwitchDirection());
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        waitTime = moveDist / speed;
        transform.Translate(Vector2.up * speed * dir * Time.deltaTime);

        if (player.collisionState.isTouchingPlayer(this.gameObject))
        {
            Debug.LogError("Translate");
            player.transform.Translate(Vector2.up * speed * dir * Time.deltaTime); 
        }
    }

    IEnumerator SwitchDirection()
    {
        while (isActive)
        {
            yield return new WaitForSeconds(waitTime);
            dir *= -1;
        }
    }
}
