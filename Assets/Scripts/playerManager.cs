using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class playerManager : MonoBehaviour
{
    [SerializeField] float maxSpeed;      //최대 속도
    [SerializeField] float jumpSpeed;     //점프 가속도
    [SerializeField] float horizontalFriction;    //가로축 공기저항
    [SerializeField] float runStopSpeed;          //가로 이동 정지 속도
    [SerializeField] bool updateAttackCombo = false;
    [SerializeField] bool createAttack = false;
    [SerializeField] bool isRolling;
    [SerializeField] int[] consumeStamina = new int[] { 8, 6, 4, 16, 6 };     // 스태미나 소비값. 공격1, 공격2, 공격3, 구르기, 비행
    [SerializeField] Transform firePoint;
    [SerializeField] GameObject playerAttackObject;
    [SerializeField] ParticleSystem dustParticleSystem;
    
    private float horizontalInput;      //좌우 이동 입력값
    private float verticalInput;        //상하 이동 입력값
    private bool isGrounded;    //바닥 접촉 여부
    private bool isTouchPlatform = false;   //이동 플랫폼 접촉 여부
    private GameObject contactPlatform;     //접촉한 이동 플랫폼
    private Vector3 contactPlatformDistance;    //접촉한 이동 플랫폼과의 거리
    private bool flip;          //스프라이트 좌우 반전
    private bool flip_before;
    private bool verticalInputToggle;   //점프<->비행 토글
    private bool isAttack;              //공격 키 입력 여부
    private bool isAttacking;           //공격 프로세스 동작 여부
    private bool isCreateAttack;        //공격 투사체 생성 트리거
    private bool isUpdateAttackCombo = false;   //공격 콤보 업데이트 트리거
    private short attackCombo;          //공격 콤보
    private bool isDead;

    Rigidbody2D rigidBody;
    BoxCollider2D boxCollider2D;
    Animator animator;

    private void Awake()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        boxCollider2D = GetComponent<BoxCollider2D>();
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
            // 공격
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

                    if (attackCombo != 3)                           // 콤보 1, 2
                        playerAttack();
                    else
                    {
                        if (GameManager.Instance.angerCharged > 0)  // 콤보 3
                            for (int i = 0; i < 5; i++)
                                playerAttack(i * 8 - 16);
                        else
                            for (int i = 0; i < 3; i++)
                                playerAttack(i * 10 - 10);
                    }
                }

                if (updateAttackCombo)              // 각 공격 모션 종료시
                {
                    if (isAttack)   //공격 모션 업데이트
                    {
                        if (isUpdateAttackCombo)
                        {
                            attackCombo = (attackCombo < 3) ? (short)(attackCombo + 1) : (short)(1);

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
                    else            // 공격 중지
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
                // 구르기
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
                    animator.ResetTrigger("isRoll");

                if (!isRolling)
                {
                    verticalInput = Input.GetAxisRaw("Vertical");

                    // 점프 and 비행
                    if (verticalInput == 1)
                    {
                        if (!verticalInputToggle)
                        {
                            if (isGrounded)                         // 점프
                            {
                                rigidBody.AddForce(Vector2.up * jumpSpeed, ForceMode2D.Impulse);
                                animator.SetBool("isJump", true);
                            }
                            else
                            {
                                if (!animator.GetBool("isJump"))    // 비행
                                {
                                    if (GameManager.Instance.consumeStamina(consumeStamina[4]))
                                    {
                                        rigidBody.AddForce(Vector2.up * jumpSpeed * 0.7f, ForceMode2D.Impulse);
                                        animator.SetBool("isFly", true);
                                        rigidBody.drag = 6.5f;
                                        rigidBody.gravityScale = 1f;
                                    }
                                    else
                                        GameManager.Instance.ShowWarnning("Not enough stamina");
                                }
                            }

                            verticalInputToggle = true;
                        }
                        else
                            animator.SetBool("isFly", false);

                        if(isTouchPlatform) isTouchPlatform = false;
                    }
                    else if (verticalInput == (-1))  // 하강
                    {
                        if (!verticalInputToggle)
                        {
                            animator.SetBool("isDown", true);
                            rigidBody.drag = 3;
                            rigidBody.gravityScale = 2;
                            verticalInputToggle = true;
                        }

                        if(isTouchPlatform) isTouchPlatform = false;
                    }
                    else                             // 점프 버튼 해제
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

            // 좌우 이동 정지시 마찰
            if (Input.GetButtonUp("Horizontal"))
            {
                rigidBody.velocity = new Vector2(rigidBody.velocity.normalized.x * horizontalFriction, rigidBody.velocity.y);
            }

            // 좌우 이동 애니메이션
            animator.SetBool("isRun", Mathf.Abs(rigidBody.velocity.x) < runStopSpeed ? false : true);
        }

        // 낙사
        if (transform.position.y < -64) GameManager.Instance.health = 0;
    }

    private void FixedUpdate()
    {
        if (!isDead)
        {
            if (!isAttacking && !isRolling)
            {
                // 좌우 이동
                horizontalInput = Input.GetAxisRaw("Horizontal");
                rigidBody.AddForce(Vector2.right * horizontalInput * 2, ForceMode2D.Impulse);

                //이동 플랫폼과 접촉중일 경우
                if(isTouchPlatform && horizontalInput == 0 && verticalInput == 0 && contactPlatform != null
                && CheckPlatform(contactPlatform.transform.position, contactPlatform.GetComponent<BoxCollider2D>().size)) {
                    transform.position = contactPlatform.transform.position - contactPlatformDistance;
                }
                
                // 스프라이트 방향 조정
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

                        if (isGrounded)
                            CreateDust();
                    }

                    if(isTouchPlatform) isTouchPlatform = false;
                }
            }

            // 좌우 최대 속도 지정
            if (rigidBody.velocity.x > maxSpeed)
                rigidBody.velocity = new Vector2(maxSpeed, rigidBody.velocity.y);
            else if (rigidBody.velocity.x < maxSpeed * (-1))
                rigidBody.velocity = new Vector2(maxSpeed * (-1), rigidBody.velocity.y);

            // 바닥에 닿았는지 계산
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

    //충돌 검사
    private void OnCollisionEnter2D(Collision2D collision) {
        if (collision != null && !GameManager.Instance.isPlayerDie)
        {
            //상호작용 가능한 오브젝트와 충돌시
            if (collision.gameObject.tag == "Enemy"
             || collision.gameObject.tag == "Interactive Object"
             || collision.gameObject.tag == "Damageable Object")
            {
                _Object obj = collision.gameObject.GetComponent<_Object>();

                //가시 함정
                if (obj.objType == _ObjectType.Spike)
                {
                    Knockback(0.5f, obj.transform.position, 0.25f);
                    GameManager.Instance.GetDamage(75, 1f);
                }
                // 톱날 함정
                else if (obj.objType == _ObjectType.CircleBlade)
                {
                    Knockback(0.75f, obj.transform.position, 0.3f);
                    GameManager.Instance.GetDamage(60, 1f);
                }
                //나무 상자, 이동 플랫폼
                else if (obj.objType == _ObjectType.WoodenBox
                 || obj.objType == _ObjectType.MovingPlatform)
                {
                    if(collision.contacts[0].normal.y > 0.7f)
                        OnEnterPlatform(collision.gameObject);
                }
            }
        }
    }

    //충돌 해제 검사
    private void OnCollisionExit2D(Collision2D collision) {
        if (collision != null && !GameManager.Instance.isPlayerDie)
        {
            //상호작용 가능한 오브젝트와 충돌 해제시
            if (collision.gameObject.tag == "Enemy"
             || collision.gameObject.tag == "Interactive Object"
             || collision.gameObject.tag == "Damageable Object")
            {
                _Object obj = collision.gameObject.GetComponent<_Object>();

                //나무 상자, 이동 플랫폼
                if (obj.objType == _ObjectType.WoodenBox
                 || obj.objType == _ObjectType.MovingPlatform)
                {
                    OnExitPlatform();
                }
            }
        }
    }

    //플레이어 공격 오브젝트 생성 함수
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

   //죽음
    public void Die()
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
        isTouchPlatform = false;
        contactPlatform = null;
    }

    //리스폰
    public void Respawn()
    {
        isDead = false;

        animator.SetBool("isDead", false);
        animator.ResetTrigger("isDie");
        animator.ResetTrigger("isRoll");
        animator.ResetTrigger("isAttack");
        animator.SetBool("isRolling", false);
        animator.SetBool("isFly", false);
        animator.SetBool("isJump", false);

        MovePlayerPoint(GameManager.Instance.respawnPosition);
        GameManager.Instance.PlayerRespawn();
    }

    //이동 플랫폼과 충돌
    private void OnEnterPlatform(GameObject gameObject)
    {
        if(!isTouchPlatform && CheckPlatform(gameObject.transform.position, gameObject.GetComponent<BoxCollider2D>().size))
        {
            rigidBody.velocity = gameObject.GetComponent<Rigidbody2D>().velocity;
            contactPlatform = gameObject;
            contactPlatformDistance = gameObject.transform.position - transform.position;
            isTouchPlatform = true;
        }
    }

    //이동 플랫폼과 충돌 해제
    private void OnExitPlatform()
    {
        isTouchPlatform = isTouchPlatform ? false : true;
    }

    //이동 플랫폼의 기능 여부 체크 함수
    private bool CheckPlatform(Vector3 platformPos, Vector2 platformSize)
    {
        if(transform.position.y - platformPos.y > (boxCollider2D.size.y + platformSize.y) / 2 - 0.1f
         && transform.position.x - platformPos.x >= (-platformSize.x / 2)
          && transform.position.x - platformPos.x <= platformSize.x / 2)
            return true;
        else
            return false;
    }

    //플레이어 단순가속 함수(원래 이동하던 방향으로 가속, 오른쪽 or 왼쪽)
    public void MovePlayerToward(float speed)
    {
        rigidBody.AddForce((flip ? -1 : 1) * Vector2.right * speed, ForceMode2D.Impulse);
    }

    //플레이어 가속 함수(특정 좌표 방향으로 가속)
    public IEnumerator MovePlayerDirection(float power, Vector3 position, float duration)
    {
        float timer = 0f;
        Vector3 dir = new Vector3(transform.position.x - position.x, transform.position.y - position.y, transform.position.z - position.z);

        while(timer <= duration)
        {
            timer += Time.deltaTime;
            rigidBody.AddForce(dir * power, ForceMode2D.Impulse);
        }
        
        yield return 0;
    }

    //플레이어 좌표 이동 함수
    public void MovePlayerPoint(Vector3 position)
    {
        transform.position = position;
    }

    //플레이어 넉백
    public void Knockback(float power, Vector3 position, float duration)
    {
        rigidBody.velocity = new Vector2(0f, 0f);
        StartCoroutine(MovePlayerDirection(power, position, duration));
    }

    //무적 토글
    public void ToggleDamageImmuneMode()
    {
        GameManager.Instance.damageImmune = !GameManager.Instance.damageImmune;
    }

    //먼지 생성
    void CreateDust()
    {
        dustParticleSystem.Play();
    }

    //애니메이터 파라미터 토글 함수
    public void ToggleAnimatorParameter(string boolName)
    {
        animator.SetBool(boolName, !animator.GetBool(boolName));
    }
}
