using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class playerManager : MonoBehaviour
{
    [SerializeField] float maxSpeed;      //max speed
    [SerializeField] float jumpSpeed;     //jump acceleration
    [SerializeField] float horizontalFriction;    //horizontal air resistance
    [SerializeField] float runStopSpeed;          //horizontal movement stop speed
    [HideInInspector] public bool updateAttackCombo = false;
    [HideInInspector] public bool createAttack = false;
    [HideInInspector] public bool isRolling;
    [SerializeField] int[] consumeStamina = new int[] { 8, 6, 4, 16, 6 };
    // stamina consumption. [Attack 1, Attack 2, Attack 3, Roll, Fly]
    [SerializeField] Transform firePoint;   //attack point
    [SerializeField] GameObject playerAttackObject;
    [SerializeField] ParticleSystem dustParticleSystem;

    private float horizontalInput;      //left-right movement input value
    private float verticalInput;        //up-down movement input value
    private bool isGrounded;    //whether the floor is in contact
    private bool isTouchPlatform = false;   //whether moving platform contact
    private GameObject contactPlatform;     //contact moving platform
    private Vector3 contactPlatformDistance;    //distance to moving platform in contact
    private bool flip;          //sprite inversion var
    private bool flip_before;
    private bool verticalInputToggle;   //jump<->fly toggle key
    private bool isAttack;              //whether to enter the attack key
    private bool isAttacking;           //whether the attack process is working
    private bool isCreateAttack;        //attack projectile spawn trigger
    private bool isUpdateAttackCombo = false;   //attack projectile spawn trigger
    private short attackCombo;          //attack combo
    private short preIndex_walk = 0;         //Save previous values ​​to avoid duplication of sounds
    [HideInInspector] public bool isDead;

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
        isDead = true;
    }

    private void Update()
    {
        if (!isDead)
        {
            // Attack
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
                    GameManager.Instance.ShowWarnning(1);
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

                    if (attackCombo != 3)                           // Combo 1, 2
                        PlayerAttack();
                    else
                    {
                        if (GameManager.Instance.angerCharged > 0)  // Combo 3
                            for (int i = 0; i < 5; i++)
                                PlayerAttack(i * 8 - 16);
                        else
                            for (int i = 0; i < 3; i++)
                                PlayerAttack(i * 10 - 10);
                    }
                }

                if (updateAttackCombo)              // At the end of each attack motion
                {
                    if (isAttack)   //Attack Motion Update
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

                                GameManager.Instance.ShowWarnning(1);
                            }
                        }
                    }
                    else            // Stop Attack
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
                // Roll
                if (Input.GetButtonDown("Fire2") && !isRolling && isGrounded)
                {
                    if (GameManager.Instance.consumeStamina(consumeStamina[3]))
                    {
                        isRolling = true;
                        animator.SetTrigger("isRoll");
                        animator.SetBool("isRolling", true);
                    }
                    else
                        GameManager.Instance.ShowWarnning(1);
                }

                if (Input.GetButtonUp("Fire2"))
                    animator.ResetTrigger("isRoll");

                if (!isRolling)
                {
                    verticalInput = Input.GetAxisRaw("Vertical");

                    // Jump and Fly
                    if (verticalInput == 1)
                    {
                        if (!verticalInputToggle)
                        {
                            if (isGrounded)                         // Jump
                            {
                                rigidBody.AddForce(Vector2.up * jumpSpeed, ForceMode2D.Impulse);
                                animator.SetBool("isJump", true);
                            }
                            else
                            {
                                if (!animator.GetBool("isJump"))    // Fly
                                {
                                    if (GameManager.Instance.consumeStamina(consumeStamina[4]))
                                    {
                                        rigidBody.AddForce(Vector2.up * jumpSpeed * 0.7f, ForceMode2D.Impulse);
                                        animator.SetBool("isFly", true);
                                        rigidBody.drag = 6.5f;
                                        rigidBody.gravityScale = 1f;
                                    }
                                    else
                                        GameManager.Instance.ShowWarnning(1);
                                }
                            }

                            verticalInputToggle = true;
                        }
                        else
                            animator.SetBool("isFly", false);

                        if(isTouchPlatform) isTouchPlatform = false;
                    }
                    else if (verticalInput == (-1))  // Downturn
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
                    else                             // release jump button
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

            // Friction when moving left-right
            if (Input.GetButtonUp("Horizontal"))
            {
                rigidBody.velocity = new Vector2(rigidBody.velocity.normalized.x * horizontalFriction, rigidBody.velocity.y);
            }

            // left-right movement animation
            animator.SetBool("isRun", Mathf.Abs(rigidBody.velocity.x) < runStopSpeed ? false : true);
        }

        // fall dead
        if (transform.position.y < -64) GameManager.Instance.health = 0;
    }

    private void FixedUpdate()
    {
        if (!isDead)
        {
            if (!isAttacking && !isRolling)
            {
                // move left-right
                horizontalInput = Input.GetAxisRaw("Horizontal");
                rigidBody.AddForce(Vector2.right * horizontalInput * 2, ForceMode2D.Impulse);

                // When in contact with a moving platform
                if (isTouchPlatform && horizontalInput == 0 && verticalInput == 0 && contactPlatform != null
                && CheckPlatform(contactPlatform.transform.position, contactPlatform.GetComponent<BoxCollider2D>().size)) {
                    transform.position = contactPlatform.transform.position - contactPlatformDistance;
                }

                // Sprite Orientation
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

            // Set max left-right speed
            if (rigidBody.velocity.x > maxSpeed)
                rigidBody.velocity = new Vector2(maxSpeed, rigidBody.velocity.y);
            else if (rigidBody.velocity.x < maxSpeed * (-1))
                rigidBody.velocity = new Vector2(maxSpeed * (-1), rigidBody.velocity.y);

            // cal if contact the floor
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

    // Collision Check
    private void OnCollisionEnter2D(Collision2D collision) {
        if (collision != null && !GameManager.Instance.isPlayerDie)
        {
            // Collision with an interactive object
            if (collision.gameObject.tag == "Enemy"
             || collision.gameObject.tag == "Interactive Object"
             || collision.gameObject.tag == "Damageable Object")
            {
                _Object obj = collision.gameObject.GetComponent<_Object>();

                // Thorn Trap
                if (obj.objType == _ObjectType.Spike)
                {
                    Knockback(0.5f, obj.transform.position, 0.25f);
                    GameManager.Instance.GetDamage(75, 1f);
                }
                // Saw Blade Trap
                else if (obj.objType == _ObjectType.CircleBlade)
                {
                    Knockback(0.75f, obj.transform.position, 0.3f);
                    GameManager.Instance.GetDamage(60, 1f);
                }
                // Wooden Box, Moving Platform
                else if (obj.objType == _ObjectType.WoodenBox
                 || obj.objType == _ObjectType.MovingPlatform)
                {
                    if(collision.contacts[0].normal.y > 0.7f)
                        OnEnterPlatform(collision.gameObject);
                }
            }
        }
    }

    // Collision Exit Check
    private void OnCollisionExit2D(Collision2D collision) {
        if (collision != null && !GameManager.Instance.isPlayerDie)
        {
            // Collision exit with an interactive object
            if (collision.gameObject.tag == "Enemy"
             || collision.gameObject.tag == "Interactive Object"
             || collision.gameObject.tag == "Damageable Object")
            {
                _Object obj = collision.gameObject.GetComponent<_Object>();

                // Wooden Box, Moving Platform
                if (obj.objType == _ObjectType.WoodenBox
                 || obj.objType == _ObjectType.MovingPlatform)
                {
                    OnExitPlatform();
                }
            }
        }
    }

    // Player attack object creation
    private void PlayerAttack(int rotate = 0)
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

    public void PlayerStop()
    {
        isDead = true;
        isAttack = false;
        isAttacking = false;
        isCreateAttack = false;
        isUpdateAttackCombo = true;
        attackCombo = 1;
        isRolling = false;
    }

    public void PlayerPlay()
    {
        isDead = false;
        isAttack = false;
        isAttacking = false;
        isCreateAttack = false;
        isUpdateAttackCombo = true;
        attackCombo = 1;
        isRolling = false;

        animator.SetBool("isDead", false);
        animator.ResetTrigger("isDie");
        animator.ResetTrigger("isRoll");
        animator.ResetTrigger("isAttack");
        animator.SetBool("isRolling", false);
        animator.SetBool("isAttacking", false);
        animator.SetInteger("AttackCombo", 1);
        animator.SetBool("isFly", false);
        animator.SetBool("isDown", false);
        animator.SetBool("isJump", false);
    }

   // Dead
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

    // Respawn
    public void Respawn()
    {
        isDead = false;
        isAttack = false;
        isAttacking = false;
        isCreateAttack = false;
        isUpdateAttackCombo = true;
        attackCombo = 1;
        isRolling = false;
        isTouchPlatform = false;
        contactPlatform = null;

        animator.SetBool("isDead", false);
        animator.ResetTrigger("isDie");
        animator.ResetTrigger("isRoll");
        animator.ResetTrigger("isAttack");
        animator.SetBool("isAttacking", false);
        animator.SetInteger("AttackCombo", 1);
        animator.SetBool("isRolling", false);
        animator.SetBool("isDown", false);
        animator.SetBool("isFly", false);
        animator.SetBool("isJump", false);

        MovePlayerPoint(GameManager.Instance.respawnPosition);
        GameManager.Instance.PlayerRespawn();
    }

    public void SoundPlay_Action(string ACname)
    {
        GameManager.Instance.SM.Play(soundManager.AS.playerAction, ACname);
    }

    public void SoundPlay_Action2(string ACname)
    {
        GameManager.Instance.SM.Play(soundManager.AS.playerAction2, ACname);
    }

    public void SoundPlay_Walk()
    {
        GameManager.Instance.SM.Play(1, (soundManager.PlayerAction)RandomSoundIndex(preIndex_walk, 6));
    }

    private int RandomSoundIndex(int pre, int maxIndex)
    {
        int index = Random.Range(0, maxIndex);

        if (index == pre) index += 1;
        if (index > maxIndex - 1) index = 0;

        pre = index;

        return index;
    }

    // Collision with moving platform
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

    // Collision Exit with moving platform
    private void OnExitPlatform()
    {
        isTouchPlatform ^= true;
    }

    // Check whether the mobile platform is functional or not
    private bool CheckPlatform(Vector3 platformPos, Vector2 platformSize)
    {
        if(transform.position.y - platformPos.y > (boxCollider2D.size.y + platformSize.y) / 2 - 0.1f
         && transform.position.x - platformPos.x >= (-platformSize.x / 2)
          && transform.position.x - platformPos.x <= platformSize.x / 2)
            return true;
        else
            return false;
    }

    // Player Simple Acceleration (Accelerate in the direction it were moving, left-right)
    public void MovePlayerToward(float speed)
    {
        rigidBody.AddForce((flip ? -1 : 1) * Vector2.right * speed, ForceMode2D.Impulse);
    }

    // Player Acceleration (Accelerate in a specific coordinate direction)
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

    // Player coordinate shift function
    public void MovePlayerPoint(Vector3 position)
    {
        transform.position = position;
    }

    // Player knockback
    public void Knockback(float power, Vector3 position, float duration)
    {
        rigidBody.velocity = new Vector2(0f, 0f);
        StartCoroutine(MovePlayerDirection(power, position, duration));
    }

    // Invulnerability toggle
    public void ToggleDamageImmuneMode()
    {
        GameManager.Instance.damageImmune ^= true;
    }

    // Dust generation
    void CreateDust()
    {
        dustParticleSystem.Play();
    }

    // Animator Parameter Toggle Function
    public void ToggleAnimatorParameter(string boolName)
    {
        animator.SetBool(boolName, !animator.GetBool(boolName));
    }
}
