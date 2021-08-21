using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerManager : MonoBehaviour
{
    public float maxSpeed;      //최대 속도
    public float jumpSpeed;     //점프 가속도
    public float horizontalFriction;    //가로축 공기저항
    public float runStopSpeed;          //가로 이동 정지 속도
    public bool isAttack;
    public Transform firePoint;
    public GameObject playerAttackObject;

    private bool isGrounded;    //바닥 접촉 여부
    private bool flip;          //스프라이트 좌우 반전
    private bool flip_before;
    private bool verticalInputToggle;   //점프<->비행 토글

    Rigidbody2D rigidBody;
    Animator animator;

    void Awake()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        flip = false;
        flip_before = false;
        verticalInputToggle = false;
        isAttack = false;
    }

    void Update()
    {
        // 점프 and 비행
        if (Input.GetAxisRaw("Vertical") == 1)
        {
            if (!verticalInputToggle)
            {
                if (isGrounded)     // 점프
                {
                    rigidBody.AddForce(Vector2.up * jumpSpeed, ForceMode2D.Impulse);
                    animator.SetBool("isJump", true);
                }
                else
                {
                    if (!animator.GetBool("isJump"))    // 비행
                    {
                        rigidBody.AddForce(Vector2.up * jumpSpeed * 0.8f, ForceMode2D.Impulse);
                        animator.SetBool("isFly", true);
                        rigidBody.drag = 8;
                        rigidBody.gravityScale = 1;
                    }
                }

                verticalInputToggle = true;
            } else
            {
                animator.SetBool("isFly", false);
            }
        }
        // 하강
        else if (Input.GetAxisRaw("Vertical") == (-1))
        {
            if (!verticalInputToggle)
            {
                animator.SetBool("isDown", true);
                rigidBody.drag = 3;
                rigidBody.gravityScale = 2;
                verticalInputToggle = true;
            }
        }
        // 점프 버튼 해제
        else
        {
            if (verticalInputToggle)
            {
                animator.SetBool("isJump", false);
                animator.SetBool("isFly", false);
                animator.SetBool("isDown", false);
                verticalInputToggle = false;
            }
        }

        // 공격
        if (Input.GetButtonDown("Fire1"))
        {
            Instantiate(playerAttackObject, firePoint.position, firePoint.rotation);
        }

        // 좌우 이동 정지시 마찰
        if (Input.GetButtonUp("Horizontal"))       
        {
            rigidBody.velocity = new Vector2(rigidBody.velocity.normalized.x * horizontalFriction
                , rigidBody.velocity.y);
        }

        // 좌우 이동 애니메이션
        if (Mathf.Abs(rigidBody.velocity.x) < runStopSpeed)    
            animator.SetBool("isRun", false);
        else
            animator.SetBool("isRun", true);
    }

    void FixedUpdate()
    {
        // 좌우 이동
        float horizontalInput = Input.GetAxisRaw("Horizontal");

        rigidBody.AddForce(Vector2.right * horizontalInput * 2, ForceMode2D.Impulse);

        // 스프라이트 방향 조정
        if (horizontalInput != 0)
        {
            if (flip == false && horizontalInput < 0)
                flip = true;
            else if (flip == true && horizontalInput > 0)
                flip = false;

            if (flip != flip_before) {
                transform.Rotate(0f, 180f, 0f);
                flip_before = flip;
            }     
        }

        // 좌우 최대 속도
        if (rigidBody.velocity.x > maxSpeed)
            rigidBody.velocity = new Vector2(maxSpeed, rigidBody.velocity.y);
        else if (rigidBody.velocity.x < maxSpeed * (-1))
            rigidBody.velocity = new Vector2(maxSpeed * (-1), rigidBody.velocity.y);

        // 바닥에 닿았는지 계산
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
    //    Gizmos.DrawWireCube(new Vector2(transform.position.x, transform.position.y - 0.72f)
    //    , new Vector2(0.71f, 0.1f));
    //}
}
