using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerManager : MonoBehaviour
{
    public float maxSpeed;
    public float horizontalFriction;
    public float runStopSpeed;

    Rigidbody2D rigidBody;
    SpriteRenderer spriteRenderer;
    Animator animator;

    void Awake()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (Input.GetButtonUp("Horizontal"))        // �¿� ������ ����
        {
            rigidBody.velocity = new Vector2(rigidBody.velocity.normalized.x * horizontalFriction, rigidBody.velocity.y);
        }

        if (Mathf.Abs(rigidBody.velocity.x) < runStopSpeed)     //�¿� �̵� �ִϸ��̼�
            animator.SetBool("isRun", false);
        else
            animator.SetBool("isRun", true);
    }

    void FixedUpdate()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");

        rigidBody.AddForce(Vector2.right * horizontalInput * 2, ForceMode2D.Impulse);   // �÷��̾� �¿� �̵�

        if (horizontalInput < 0)            //�÷��̾� ����
            spriteRenderer.flipX = true;
        else if (horizontalInput > 0)
            spriteRenderer.flipX = false;

        if (rigidBody.velocity.x > maxSpeed)    //�¿� �ִ� �ӵ�
        {
            rigidBody.velocity = new Vector2(maxSpeed, rigidBody.velocity.y);
        } else if (rigidBody.velocity.x < maxSpeed * (-1))
        {
            rigidBody.velocity = new Vector2(maxSpeed * (-1), rigidBody.velocity.y);
        }
    }
}
