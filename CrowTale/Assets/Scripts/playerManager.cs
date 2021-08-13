using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerManager : MonoBehaviour
{
    public float maxSpeed;      //최대 속도
    public float jumpSpeed;     //점프 가속도
    public float horizontalFriction;    //가로축 공기저항
    public float runStopSpeed;          //가로 이동 정지 속도

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
        if (Input.GetButtonDown("Jump"))        //점프, 날기
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

        if (Input.GetAxisRaw("Vertical") == (-1))        //하강
        {
            animator.SetBool("isDown", true);
            rigidBody.drag = 3;
            rigidBody.gravityScale = 2;
        }
        
        if (Input.GetButtonUp("Horizontal"))        //좌우 정지시 마찰
        {
            rigidBody.velocity = new Vector2(rigidBody.velocity.normalized.x * horizontalFriction, rigidBody.velocity.y);
        }

        if (Mathf.Abs(rigidBody.velocity.x) < runStopSpeed)     //좌우 이동 애니메이션
            animator.SetBool("isRun", false);
        else
            animator.SetBool("isRun", true);
    }

    void FixedUpdate()
    {
        //좌우 이동
        float horizontalInput = Input.GetAxisRaw("Horizontal");

        rigidBody.AddForce(Vector2.right * horizontalInput * 2, ForceMode2D.Impulse);

        //스프라이트 방향 조정
        if (horizontalInput < 0)
            spriteRenderer.flipX = true;
        else if (horizontalInput > 0)
            spriteRenderer.flipX = false;

        //좌우 최대 속도
        if (rigidBody.velocity.x > maxSpeed)
            rigidBody.velocity = new Vector2(maxSpeed, rigidBody.velocity.y);
        else if (rigidBody.velocity.x < maxSpeed * (-1))
            rigidBody.velocity = new Vector2(maxSpeed * (-1), rigidBody.velocity.y);

        //바닥에 닿았는지 계산
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
