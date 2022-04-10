using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyType
{
    Dummy, Snake, Spike
}

public class Enemy : MonoBehaviour
{
    public bool isHit = true;
    public int maxHealth = 100;
    public int health = 100;
    public EnemyType enemyType;
    public GameObject deathEffect;
    
    private short hitAction;
    private bool hitDirection;
    
    Animator animator;

    void Awake()
    {
        try{
            animator = GetComponent<Animator>();
        } catch {
        }
        
        hitAction = -1;
    }

    void Update()
    {
        if (enemyType == EnemyType.Dummy)
        {
            if (hitAction == 1)
            {
                animator.SetBool("isHit", true);
                animator.SetBool("Direction", hitDirection);

                hitAction = 0;
            } else if (hitAction == 0)
            {
                animator.SetBool("isHit", false);
                hitAction = -1;
            }
        }
    }

    public void TakeDamage(int damage, float hitDir)
    {
        health -= damage;

        GameManager.Instance.increaseAngerLevel(3);

        // Á×À½ Ã³¸®
            if (health <= 0)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
            Destroy(gameObject);
        } else
        {
            hitAction = 1;

            if (hitDir == 1f)
                hitDirection = true;
            else
                hitDirection = false;
        }
    }
}
