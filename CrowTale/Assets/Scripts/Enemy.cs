using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyType
{
    Dummy, Snake
}

public class Enemy : MonoBehaviour
{
    public int hp = 100;
    public EnemyType enemyType;
    public GameObject deathEffect;

    private int health;
    private short hitAction;
    private bool hitDirection;

    Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();

        health = hp;
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
