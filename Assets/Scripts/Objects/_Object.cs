using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum _ObjectType
{
    Dummy, Snake, Spike, WoodenBox, MovingPlatform, CircleBlade
}

public class _Object : MonoBehaviour
{
    public bool isHit = true;       //�÷��̾� ���� �ǰ� ����
    public int angerAmount = 3;     //�÷��̾� ���� �ǰݽ� �г� ������
    public int maxHealth = 100;     //�ִ� ü��
    public int health = 100;        //���� ü��

    public _ObjectType objType;         //������Ʈ ����
    public GameObject deathEffect;      //�ı��� ���� ������Ʈ
    
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

    //������ ó��
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

    //HP���� �ø��� ȣ��Ǵ� �Լ�
    public virtual void CheckHP()
    {

    }
    
    //����
    protected virtual void Die()
    {
        Instantiate(deathEffect, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}