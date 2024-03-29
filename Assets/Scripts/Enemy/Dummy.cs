using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dummy : _Object
{
    private short hitAction;
    private bool hitDirection;

    protected Animator animator;
    protected SpriteRenderer spriteRenderer;

    private void Awake()
    {
        if (GetComponent<Animator>() != null)
            animator = GetComponent<Animator>();

        spriteRenderer = GetComponent<SpriteRenderer>();
        
        hitAction = -1;
        objType = _ObjectType.Dummy;
    }

    private void Update()
    {
        if (transform.position.y < -64)
        {
            Destroy(gameObject);
        }

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

    //Damage handle
    public override void TakeDamage(int damage, float hitDir)
    {
        health -= damage;

        if(angerAmount > 0)
            GameManager.Instance.increaseAngerLevel(angerAmount);

        if (health <= 0 && !isDie)
        {
            base.Die();
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
