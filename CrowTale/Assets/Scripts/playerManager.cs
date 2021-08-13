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
        if (Input.GetButtonDown("Jump"))        //점프
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
        float horizontalInput = Input.GetAxisRaw("Horizontal");

        rigidBody.AddForce(Vector2.right * horizontalInput * 2, ForceMode2D.Impulse);   //플레이어 좌우 이동

        if (horizontalInput < 0)            //플레이어 방향
            spriteRenderer.flipX = true;
        else if (horizontalInput > 0)
            spriteRenderer.flipX = false;

        if (rigidBody.velocity.x > maxSpeed)    //좌우 최대 속도
            rigidBody.velocity = new Vector2(maxSpeed, rigidBody.velocity.y);
        else if (rigidBody.velocity.x < maxSpeed * (-1))
            rigidBody.velocity = new Vector2(maxSpeed * (-1), rigidBody.velocity.y);

        //바닥에 닿았는지 계산

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
