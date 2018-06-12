using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class EnemyBase : MonoBehaviour {
    public float health;
    public new Collider2D collider;
    public Renderer rend;

    public void Start()
    {
        collider = GetComponent<Collider2D>();
        rend = GetComponent<Renderer>();
    }

    /* @damage is subtracted from Enemy health. */
    public void OnHit(float damage)
    {
        health -= damage;
        if(health <= 0)
        {
            health = 0;
            StartCoroutine(OnDeath());
        }
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Attack"))
        {
            PAttackInstance attack = collision.gameObject.GetComponent<PAttackInstance>();
            OnHit(attack.damage);
        }
        
    }

    IEnumerator OnDeath()
    {
        collider.enabled = false;
        //rend.material.color = new Color(255,236,236);
        yield return new WaitForSeconds(.1f);
        Destroy(gameObject, 0.0f);
    }
}
