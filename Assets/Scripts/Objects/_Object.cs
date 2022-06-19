using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum _ObjectType
{
    Dummy, Snake, Spike, WoodenBox, MovingPlatform, CircleBlade
}

public class _Object : MonoBehaviour
{
    public bool isHit = true;       //플레이어 공격 피격 여부
    public int angerAmount = 3;     //플레이어 공격 피격시 분노 충전량
    public int maxHealth = 100;     //최대 체력
    public int health = 100;        //현재 체력

    public _ObjectType objType;         //오브젝트 종류
    public GameObject deathEffect;      //파괴시 생성 오브젝트
    
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

    //데미지 처리
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

    //HP변동 시마다 호출되는 함수
    public virtual void CheckHP()
    {

    }
    
    //죽음
    protected virtual void Die()
    {
        Instantiate(deathEffect, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}