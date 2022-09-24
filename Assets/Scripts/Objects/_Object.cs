using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum _ObjectType
{
    Dummy, Snake, Spike, WoodenBox, MovingPlatform, CircleBlade, Summoner
}

public class _Object : MonoBehaviour
{
    public bool isHit = true;       //Whether the player is attacked
    public int angerAmount = 3;     //Anger Charges When Hit by Player Attacks
    public int maxHealth = 100;     //maximum HP
    public int health = 100;        //current HP

    public _ObjectType objType;         //object type
    public GameObject deathEffect;      //object created upon destruction

    protected SpriteRenderer spriteRenderer;

    protected virtual void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    protected virtual void Update()
    {
        if (transform.position.y < -64)
        {
            Destroy(gameObject);
        }
    }

    //damage handling
    public virtual void TakeDamage(int damage, float hitDir)
    {
        health -= damage;

        if(angerAmount > 0)
            GameManager.Instance.increaseAngerLevel(angerAmount);

        if (health <= 0)
        {
            Die();
        }

        CheckHP();
    }

    //Function called whenever HP changes
    public virtual void CheckHP()
    {

    }
    
    //Death
    protected virtual void Die()
    {
        Instantiate(deathEffect, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}