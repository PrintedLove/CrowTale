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
    public bool isRolling;
    public Transform firePoint;
    public GameObject playerAttackObject;
    public int[] consumeStamina = new int[] { 8, 6, 4, 16, 6 };     // ���¹̳� �Һ�. ����1, ����2, ����3, ������, ����

    private bool isGrounded;    //�ٴ� ���� ����
    private bool flip;          //��������Ʈ �¿� ����
    private bool flip_before;
    private bool verticalInputToggle;   //����<->���� ���
    private bool isAttack;              //���� Ű �Է� ����
    private bool isAttacking;           //���� ���μ��� ���� ����
    private bool isCreateAttack;        //���� ����ü ���� Ʈ����
    private bool isUpdateAttackCombo = false;   //���� �޺� ������Ʈ Ʈ����
    private short attackCombo;          //���� �޺�
    private bool isDead;

    Rigidbody2D rigidBody;
    Animator animator;

    private void Awake()
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
        isRolling = false;
        isDead = false;
    }

    private void Update()
    {
        if (!isDead)
        {
            // ����
            if (Input.GetButtonDown("Fire1") && isGrounded && !isAttacking && !isRolling)
            {
                if (GameManager.Instance.angerCharged > 0 
                    || GameManager.Instance.consumeStamina(consumeStamina[attackCombo - 1], false))
                {
                    isAttack = true;
                    isAttacking = true;
                    isCreateAttack = true;
                    animator.SetTrigger("isAttack");
                    animator.SetBool("isAttacking", true);
                }
                else
                    GameManager.Instance.ShowWarnning("Not enough stamina");
            }

            if (Input.GetButtonUp("Fire1"))
            {
                isAttack = false;
                attackCombo = 1;
                animator.SetInteger("AttackCombo", attackCombo);
                animator.ResetTrigger("isAttack");
            }

            if (isAttacking)
            {
                if (createAttack && isCreateAttack)
                {
                    isCreateAttack = false;
                    isUpdateAttackCombo = true;

                    if (GameManager.Instance.angerCharged == 0)
                        GameManager.Instance.consumeStamina(consumeStamina[attackCombo - 1]);

                    if (attackCombo != 3)                           // �޺� 1, 2
                        playerAttack();
                    else
                    {
                        if (GameManager.Instance.angerCharged > 0)  // �޺� 3
                            for (int i = 0; i < 5; i++)
                                playerAttack(i * 8 - 16);
                        else
                            for (int i = 0; i < 3; i++)
                                playerAttack(i * 10 - 10);
                    }
                }

                if (updateAttackCombo)              // �� ���� ��� �����
                {
                    if (isAttack)   //���� ��� ������Ʈ
                    {
                        if (isUpdateAttackCombo)
                        {
                            if (attackCombo < 3)
                                attackCombo += 1;
                            else
                                attackCombo = 1;

                            if (GameManager.Instance.angerCharged > 0 
                                || GameManager.Instance.consumeStamina(consumeStamina[attackCombo - 1], false))
                            {
                                animator.SetInteger("AttackCombo", attackCombo);
                                isCreateAttack = true;
                                isUpdateAttackCombo = false;
                            }
                            else
                            {
                                isAttacking = false;
                                isCreateAttack = true;
                                isUpdateAttackCombo = true;
                                attackCombo = 1;
                                animator.SetInteger("AttackCombo", attackCombo);
                                animator.SetBool("isAttacking", false);

                                GameManager.Instance.ShowWarnning("Not enough stamina");
                            }
                        }
                    }
                    else            // ���� ����
                    {
                        isAttacking = false;
                        isCreateAttack = true;
                        isUpdateAttackCombo = true;
                        attackCombo = 1;
                        animator.SetInteger("AttackCombo", attackCombo);
                        animator.SetBool("isAttacking", false);
                    }
                }
            }
            else
            {
                // ������
                if (Input.GetButtonDown("Fire2") && !isRolling && isGrounded)
                {
                    if (GameManager.Instance.consumeStamina(consumeStamina[3]))
                    {
                        isRolling = true;
                        animator.SetTrigger("isRoll");
                        animator.SetBool("isRolling", true);
                    }
                    else
                        GameManager.Instance.ShowWarnning("Not enough stamina");
                }

                if (Input.GetButtonUp("Fire2"))
                {
                    animator.ResetTrigger("isRoll");
                }

                if (!isRolling)
                {
                    // ���� and ����
                    if (Input.GetAxisRaw("Vertical") == 1)
                    {
                        if (!verticalInputToggle)
                        {
                            if (isGrounded)                         // ����
                            {
                                rigidBody.AddForce(Vector2.up * jumpSpeed, ForceMode2D.Impulse);
                                animator.SetBool("isJump", true);
                            }
                            else
                            {
                                if (!animator.GetBool("isJump"))    // ����
                                {
                                    if (GameManager.Instance.consumeStamina(consumeStamina[4]))
                                    {
                                        rigidBody.AddForce(Vector2.up * jumpSpeed * 0.7f, ForceMode2D.Impulse);
                                        animator.SetBool("isFly", true);
                                        rigidBody.drag = 6.5f;
                                        rigidBody.gravityScale = 1.2f;
                                    }
                                    else
                                        GameManager.Instance.ShowWarnning("Not enough stamina");
                                }
                            }

                            verticalInputToggle = true;
                        }
                        else
                        {
                            animator.SetBool("isFly", false);
                        }
                    }
                    else if (Input.GetAxisRaw("Vertical") == (-1))  // �ϰ�
                    {
                        if (!verticalInputToggle)
                        {
                            animator.SetBool("isDown", true);
                            rigidBody.drag = 3;
                            rigidBody.gravityScale = 2;
                            verticalInputToggle = true;
                        }
                    }
                    else                                            // ���� ��ư ����
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
    }

    private void FixedUpdate()
    {
        if (!isDead)
        {
            if (!isAttacking && !isRolling)
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
    }

    private void playerAttack(int rotate = 0)       //�÷��̾� ���� ������Ʈ ���� �Լ�
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

    public void MovePlayerToward(float speed)       //�÷��̾� ���� �Լ�
    {
        if(flip)
            rigidBody.AddForce(-Vector2.right * speed, ForceMode2D.Impulse);
        else
            rigidBody.AddForce(Vector2.right * speed, ForceMode2D.Impulse);
    }

    public void MovePlayerPoint(Vector3 position)       //�÷��̾� ��ǥ �̵� �Լ�
    {
        transform.position = position;
    }

    public void Respawn()       //������
    {
        isDead = false;

        animator.SetBool("isDead", false);
        animator.ResetTrigger("isDie");

        MovePlayerPoint(GameManager.Instance.respawnPosition);
        GameManager.Instance.PlayerRespawn();
    }

    public void ToggleAnimatorParameter(string boolName)       //�ִϸ����� �Ķ���� ��� �Լ�
    {
        animator.SetBool(boolName, !animator.GetBool(boolName));
    }

    public void ToggleGodMode()       //���� ���
    {
        GameManager.Instance.isGodMode = !GameManager.Instance.isGodMode;
    }

    public void Die()       //����
    {
        isDead = true;
        animator.SetBool("isDead", true);
        animator.SetTrigger("isDie");

        isAttack = false;
        isAttacking = false;
        isCreateAttack = false;
        isUpdateAttackCombo = true;
        attackCombo = 1;
        isRolling = false;
    }
}
