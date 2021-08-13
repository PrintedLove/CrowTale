using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerManager : MonoBehaviour
{
    public float maxSpeed;      //�ִ� �ӵ�
    public float jumpSpeed;     //���� ���ӵ�
    public float horizontalFriction;    //������ ��������
    public float runStopSpeed;          //���� �̵� ���� �ӵ�

    private bool isGrounded;

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
        if (Input.GetButtonDown("Jump"))        //����, ����
        {
            if (isGrounded)
            {
                rigidBody.AddForce(Vector2.up * jumpSpeed, ForceMode2D.Impulse);
                animator.SetBool("isJump", true);
            } else
            {
                if (!animator.GetBool("isJump"))
                {
                    rigidBody.AddForce(Vector2.up * jumpSpeed * 0.8f, ForceMode2D.Impulse);
                    animator.SetBool("isFly", true);
                    rigidBody.drag = 8;
                    rigidBody.gravityScale = 1;
                }
            }
        }

        if (Input.GetButtonUp("Jump"))
        {
            animator.SetBool("isJump", false);
            animator.SetBool("isFly", false);
            animator.SetBool("isDown", false);
        }

        if (Input.GetAxisRaw("Vertical") == (-1))        //�ϰ�
        {
            animator.SetBool("isDown", true);
            rigidBody.drag = 3;
            rigidBody.gravityScale = 2;
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
        //�¿� �̵�
        float horizontalInput = Input.GetAxisRaw("Horizontal");

        rigidBody.AddForce(Vector2.right * horizontalInput * 2, ForceMode2D.Impulse);

        //��������Ʈ ���� ����
        if (horizontalInput < 0)
            spriteRenderer.flipX = true;
        else if (horizontalInput > 0)
            spriteRenderer.flipX = false;

        //�¿� �ִ� �ӵ�
        if (rigidBody.velocity.x > maxSpeed)
            rigidBody.velocity = new Vector2(maxSpeed, rigidBody.velocity.y);
        else if (rigidBody.velocity.x < maxSpeed * (-1))
            rigidBody.velocity = new Vector2(maxSpeed * (-1), rigidBody.velocity.y);

        //�ٴڿ� ��Ҵ��� ���
        bool platformHit = Physics2D.OverlapBox(new Vector2(transform.position.x, transform.position.y - 0.72f)
            , new Vector2(0.71f, 0.1f), 0, LayerMask.GetMask("Platform"));

        if (platformHit)
        {
            isGrounded = true;
            animator.SetBool("isGrounded", true);
            animator.SetBool("isFly", false);
            rigidBody.drag = 3;
            rigidBody.gravityScale = 2;
        }
        else
        {
            isGrounded = false;
            animator.SetBool("isGrounded", false);
        }
    }

    //void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.red;
    //    Gizmos.DrawWireCube(new Vector2(transform.position.x, transform.position.y - 0.72f), new Vector2(0.71f, 0.1f));
    //}
}
