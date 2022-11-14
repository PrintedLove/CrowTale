using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Snake : _Object
{
    private short hitAction;
    private bool hitDirection;

    protected Animator animator;
    protected SpriteRenderer spriteRenderer;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        objType = _ObjectType.Dummy;
    }

    private void Update()
    {
        
    }

    //Damage handle
    public override void TakeDamage(int damage, float hitDir)
    {
        health -= damage;

        if (angerAmount > 0)
            GameManager.Instance.increaseAngerLevel(angerAmount);

        if (health <= 0)
        {
            base.Die();
        }
        else
        {
            hitAction = 1;

            if (hitDir == 1f)
                hitDirection = true;
            else
                hitDirection = false;
        }
    }
}
