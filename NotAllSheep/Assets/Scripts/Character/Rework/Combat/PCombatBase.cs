using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PCombatBase : MonoBehaviour {
    public float health;

    // Use this for initialization
    void Start () {
                
	}

    public void DoAttack(float value)
    {

    }

    /* @damage is subtracted from Enemy health. */
    public void OnHit(float damage, GameObject attacker)
    {
        health -= damage;
        if (health <= 0)
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
