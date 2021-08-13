using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerManager : MonoBehaviour
{
    public float maxSpeed;
    public float jumpSpeed;
    public float horizontalFriction;
    public float runStopSpeed;

    private bool isGrounded;
    private bool isFly;

    Rigidbody2D rigidBody;
    SpriteRenderer spriteRenderer;
    Animator animator;

    void Awake()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        isFly = false;
    }

    void Update()
    {
        if (Input.GetButtonDown("Jump"))        //����
        {
            if (isGrounded)
            {
                rigidBody.AddForce(Vector2.up * jumpSpeed, ForceMode2D.Impulse);
                animator.SetBool("isJump", true);
            } else
            {
                if (isFly)
                {
                    rigidBody.AddForce(Vector2.up * jumpSpeed * 0.75f, ForceMode2D.Impulse);
                    animator.SetBool("isFly", true);
                    rigidBody.drag = 10;
                    rigidBody.gravityScale = 1;
                }
            }
        }

        if (Input.GetButtonUp("Jump"))
        {
            animator.SetBool("isJump", false);
            animator.SetBool("isFly", false);
            isFly = true;
        }

        if (Input.GetButtonUp("Horizontal"))        //�¿� ������ ����
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

        rigidBody.AddForce(Vector2.right * horizontalInput * 2, ForceMode2D.Impulse);   //�÷��̾� �¿� �̵�

        if (horizontalInput < 0)            //�÷��̾� ����
            spriteRenderer.flipX = true;
        else if (horizontalInput > 0)
            spriteRenderer.flipX = false;

        if (rigidBody.velocity.x > maxSpeed)    //�¿� �ִ� �ӵ�
            rigidBody.velocity = new Vector2(maxSpeed, rigidBody.velocity.y);
        else if (rigidBody.velocity.x < maxSpeed * (-1))
            rigidBody.velocity = new Vector2(maxSpeed * (-1), rigidBody.velocity.y);

        //�ٴڿ� ��Ҵ��� ���

        RaycastHit2D rayHit = Physics2D.Raycast(rigidBody.position, Vector3.down, 1, LayerMask.GetMask("Platform"));

        if (rayHit.collider != null && rayHit.distance < 0.75f)
        {
            isGrounded = true;
            animator.SetBool("isGrounded", true);
            isFly = false;
            rigidBody.drag = 3;
            rigidBody.gravityScale = 2;
        }
        else
        {
            isGrounded = false;
            animator.SetBool("isGrounded", false);
        }
    }
}
