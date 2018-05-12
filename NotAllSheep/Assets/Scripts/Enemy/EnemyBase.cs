using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class EnemyBase : MonoBehaviour {
    public float health;

    /* @damage is subtracted from Enemy health. */
    public void OnHit(float damage, GameObject attacker)
    {
        health -= damage;
        if(health <= 0)
        {
            health = 0;
            OnDeath();
        }
    }

    protected void OnDeath()
    {
        Destroy(gameObject, 0.0f);
    }
}
