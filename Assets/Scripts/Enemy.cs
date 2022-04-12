using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyType
{
    Dummy, Snake, Spike, WoodenBox
}

public class Enemy : MonoBehaviour
{
    public bool isHit = true;       //플레이어 공격 피격 여부
    public int angerAmount = 3;     //플레이어 공격 피격시 분노 충전량
    public int maxHealth = 100;     //최대 체력
    public int health = 100;        //현재 체력

    public Sprite[] sprite;
    public EnemyType enemyType;
    public GameObject deathEffect;
    
    private short hitAction;
    private bool hitDirection;

    private int[] val = new int[5] {0, 0, 0, 0, 0};
    
    Animator animator;
    SpriteRenderer spriteRenderer;

    void Awake()
    {
        if (GetComponent<Animator>() != null)
            animator = GetComponent<Animator>();

        spriteRenderer = GetComponent<SpriteRenderer>();
        
        hitAction = -1;
    }

    void Update()
    {
        //더미
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

    //데미지 처리
    public void TakeDamage(int damage, float hitDir)
    {
        health -= damage;

        if(angerAmount > 0)
            GameManager.Instance.increaseAngerLevel(angerAmount);

        if (health <= 0)
        {
            Die();
        } else
        {
            hitAction = 1;

            if (hitDir == 1f)
                hitDirection = true;
            else
                hitDirection = false;
        }

        CheckHP();
    }

    //HP변동 시마다 호출되는 함수
    private void CheckHP()
    {
        //나무 상자
        if (enemyType == EnemyType.WoodenBox)
        {
            if (health <= maxHealth * 30 / 100) 
                spriteRenderer.sprite = sprite[1];
            else if (health <= maxHealth * 60 / 100)
                spriteRenderer.sprite = sprite[0];
        }
    }
    
    //죽음
    private void Die()
    {
        Instantiate(deathEffect, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}