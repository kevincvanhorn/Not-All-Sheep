using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
/* The actual projectile, slash, manifestation of the attack. */
public class PAttackInstance : MonoBehaviour {

    // Get Player/Parent spawning transform from Player.Instance.transform bc is attached to the main. 

    public LayerMask layer = (int)CollisionLayers.Enemy;
    public bool destroyOnAwake = true;
    public float destroyDelay = 0.0f;
    public float damage = 1;

    public virtual void Start()
    {
        if (destroyOnAwake)
        {
            StartCoroutine(DestroyAfterDelay());
        }
    }

    public virtual void OnCollisionEnter(Collision collision)
    {

    }

    public virtual void OnFixedUpdate()
    {

    }

    public virtual IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(destroyDelay);
        Destroy(gameObject);
    }
}
