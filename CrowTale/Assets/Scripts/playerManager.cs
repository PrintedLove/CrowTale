using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class playerManager : MonoBehaviour
{
    public float maxSpeed;      //�ִ� �ӵ�
    public float jumpSpeed;     //���� ���ӵ�
    public float horizontalFriction;    //������ ��������
    public float runStopSpeed;          //���� �̵� ���� �ӵ�
    public bool updateAttackCombo = false;
    public bool createAttack = false;
    public Transform firePoint;
    public GameObject playerAttackObject;

    private bool isGrounded;    //�ٴ� ���� ����
    private bool flip;          //��������Ʈ �¿� ����
    private bool flip_before;
    private bool verticalInputToggle;   //����<->���� ���
    private bool isAttack;              //���� Ű �Է� ����
    private bool isAttacking;           //���� ���μ��� ���� ����
    private bool isCreateAttack;        //���� ����ü ���� Ʈ����
    private bool isUpdateAttackCombo = false;   //���� �޺� ������Ʈ Ʈ����
    private short attackCombo;          //���� �޺�

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
        isAttacking = false;
        isCreateAttack = false;
        isUpdateAttackCombo = true;
        attackCombo = 1;
    }

    void Update()
    {
        // ����
        if (Input.GetButtonDown("Fire1") && !animator.GetBool("isAttack") && isGrounded && isAttacking == false)
        {
            isAttack = true;
            isAttacking = true;
            isCreateAttack = true;
            animator.SetTrigger("isAttack");
            animator.SetBool("isAttacking", true);
        }

        if (Input.GetButtonUp("Fire1"))
        {
            isAttack = false;
            attackCombo = 1;
            animator.SetInteger("AttackCombo", attackCombo);
            animator.SetBool("isAttack", false);
        }

        if (isAttacking)
        {
            if (createAttack && isCreateAttack)
            {
                isCreateAttack = false;
                isUpdateAttackCombo = true;

                if (attackCombo != 3)           // �޺� 1, 2
                    playerAttack();
                else
                {
                    if (GameManager.Instance.angerCharged > 0)           // �޺� 3
                        for (int i = 0; i < 5; i++)
                            playerAttack(i * 8 - 16);
                    else
                        for (int i = 0; i < 3; i++)
                            playerAttack(i * 10 - 10);
                }
            }

            if (updateAttackCombo)
            {
                if (isAttack)
                {
                    if (isUpdateAttackCombo)
                    {
                        if (attackCombo < 3)
                            attackCombo += 1;
                        else
                            attackCombo = 1;

                        animator.SetInteger("AttackCombo", attackCombo);
                        isCreateAttack = true;
                        isUpdateAttackCombo = false;
                    }
                } else
                {
                    isAttacking = false;
                    isCreateAttack = true;
                    isUpdateAttackCombo = true;
                    attackCombo = 1;
                    animator.SetInteger("AttackCombo", attackCombo);
                    animator.SetBool("isAttacking", false);
                }
            }
        } else
        {
            // ���� and ����
            if (Input.GetAxisRaw("Vertical") == 1)
            {
                if (!verticalInputToggle)
                {
                    if (isGrounded)     // ����
                    {
                        rigidBody.AddForce(Vector2.up * jumpSpeed, ForceMode2D.Impulse);
                        animator.SetBool("isJump", true);
                    }
                    else
                    {
                        if (!animator.GetBool("isJump"))    // ����
                        {
                            rigidBody.AddForce(Vector2.up * jumpSpeed * 0.8f, ForceMode2D.Impulse);
                            animator.SetBool("isFly", true);
                            rigidBody.drag = 8;
                            rigidBody.gravityScale = 1;
                        }
                    }

                    verticalInputToggle = true;
                }
                else
                {
                    animator.SetBool("isFly", false);
                }
            }
            // �ϰ�
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
            // ���� ��ư ����
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
        }

        // �¿� �̵� ������ ����
        if (Input.GetButtonUp("Horizontal"))       
        {
            rigidBody.velocity = new Vector2(rigidBody.velocity.normalized.x * horizontalFriction
                , rigidBody.velocity.y);
        }

        // �¿� �̵� �ִϸ��̼�
        if (Mathf.Abs(rigidBody.velocity.x) < runStopSpeed)    
            animator.SetBool("isRun", false);
        else
            animator.SetBool("isRun", true);
    }

    void FixedUpdate()
    {
        if (!isAttacking)
        {
            // �¿� �̵�
            float horizontalInput = Input.GetAxisRaw("Horizontal");

            rigidBody.AddForce(Vector2.right * horizontalInput * 2, ForceMode2D.Impulse);


            // ��������Ʈ ���� ����
            if (horizontalInput != 0)
            {
                if (flip == false && horizontalInput < 0)
                    flip = true;
                else if (flip == true && horizontalInput > 0)
                    flip = false;

                if (flip != flip_before)
                {
                    transform.Rotate(0f, 180f, 0f);
                    flip_before = flip;
                }
            }
        }

        // �¿� �ִ� �ӵ�
        if (rigidBody.velocity.x > maxSpeed)
            rigidBody.velocity = new Vector2(maxSpeed, rigidBody.velocity.y);
        else if (rigidBody.velocity.x < maxSpeed * (-1))
            rigidBody.velocity = new Vector2(maxSpeed * (-1), rigidBody.velocity.y);

        // �ٴڿ� ��Ҵ��� ���
        if (Physics2D.OverlapBox(new Vector2(transform.position.x, transform.position.y - 0.72f)
            , new Vector2(0.71f, 0.1f), 0, LayerMask.GetMask("Platform")))
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

    private void playerAttack(int rotate = 0)
    {
        GameObject playerAtk = Instantiate(playerAttackObject, firePoint.position, firePoint.rotation);
        playerAttack playerAtkScript = playerAtk.GetComponent<playerAttack>();
        SpriteRenderer playerAtkRenderer = playerAtk.GetComponent<SpriteRenderer>();
        Color fixedColor;

        if (GameManager.Instance.angerCharged > 0)
        {
            fixedColor = GameManager.Instance.rageColor;
            playerAtk.GetComponent<Light2D>().enabled = true;
            playerAtkScript.lighting = true;
        }
        else
        {
            fixedColor = GameManager.Instance.defaultColor;
            playerAtkScript.lighting = false;
        }

        playerAtkRenderer.material.SetColor("_Color", fixedColor);

        playerAtk.transform.Rotate(new Vector3(0, 0, rotate));

        playerAtkScript.damage = GameManager.Instance.fixedPower;
        playerAtkScript.fixedColor = fixedColor;
    }
    //void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.red;
    //    Gizmos.DrawWireCube(new Vector2(transform.position.x, transform.position.y - 0.72f)
    //    , new Vector2(0.71f, 0.1f));
    //}
}
