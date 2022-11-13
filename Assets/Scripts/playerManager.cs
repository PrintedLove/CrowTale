using Com.LuisPedroFonseca.ProCamera2D;
using System.Collections;
using System.Collections.Generic;
using System.IO.Pipes;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

public class PlayerManager : MonoBehaviour
{
    public float maxSpeed = 10f;      //max speed
    public float jumpSpeed = 26f;     //jump acceleration
    public float horizontalFriction = 1.75f;    //horizontal air resistance
    public float runStopSpeed = 2f;          //horizontal movement stop speed
    [HideInInspector] public bool updateAttackCombo = false;
    [HideInInspector] public bool createAttack = false;
    [HideInInspector] public bool isRolling;
    public int[] consumeStamina = new int[] { 7, 5, 3, 15, 8 };
    // stamina consumption. [Attack 1, Attack 2, Attack 3, Roll, Fly]
    [SerializeField] GameObject fireDirection;   //attack direction
    [SerializeField] Transform firePoint;   //attack point
    [SerializeField] GameObject playerAttackObject;
    [SerializeField] ParticleSystem dustParticleSystem;

    private float horizontalInput;      //left-right movement input value
    private float verticalInput;        //up-down movement input value
    [SerializeField] private float groundDetectBox_y = 0.72f;
    [SerializeField] private Vector2 groundDetectBox = new Vector2(0.5f, 0.1f);
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
    private float attackAngle;
    private float showfireDir = 0f;
    private bool isRunShowAttackDirecion;
    private short preIndex_walk = 0;         //Save previous values ​​to avoid duplication of sounds
    private short preIndex_getDamage = 0;
    private bool isStop;
    [HideInInspector] public bool isDead;
    [HideInInspector] public Vector2 mousePosition;     //mouse position

    Rigidbody2D rigidBody;
    BoxCollider2D boxCollider2D;
    Animator animator;

    [ContextMenu("Set Player Respawn Position")]
    public void SetPlayerRespawnPosition()
    {
        GameObject.FindWithTag("GameController").GetComponent<GameManager>().respawnPosition = transform.position;
        Camera.main.transform.position = new Vector3(transform.position.x, transform.position.y, -10);
    }

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
        isStop = true;
        isDead = false;
        isRunShowAttackDirecion = false;
    }

    private void Update()
    {
        //global use mouse position
        mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (!isStop && !isDead)
        {
            // Attack Direction
            if (showfireDir > 0)
            {
                attackAngle = Mathf.Atan2(mousePosition.y - fireDirection.transform.position.y
                    , mousePosition.x - fireDirection.transform.position.x) * Mathf.Rad2Deg;
                fireDirection.transform.rotation = Quaternion.AngleAxis(attackAngle, Vector3.forward);
            }

            // Attack
            if (Input.GetButtonDown("Fire1") && isGrounded && !isAttacking && !isRolling)
            {
                showfireDir = 5f;
                StartCoroutine(RunShowAttackDirecion());

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

                    if (!isRunShowAttackDirecion)
                        StartCoroutine(RunShowAttackDirecion());
                    showfireDir = 5f;

                    if (mousePosition.x < transform.position.x)
                        flip = true;
                    else
                        flip = false;

                    if (flip != flip_before)
                    {
                        transform.Rotate(0f, 180f, 0f);
                        flip_before = flip;
                    }

                    attackAngle = Mathf.Atan2(mousePosition.y - fireDirection.transform.position.y
                    , mousePosition.x - fireDirection.transform.position.x) * Mathf.Rad2Deg;
                    fireDirection.transform.rotation = Quaternion.AngleAxis(attackAngle, Vector3.forward);

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

                        if (isTouchPlatform) isTouchPlatform = false;
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

                        if (isTouchPlatform) isTouchPlatform = false;
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
        if (transform.position.y < -64 && !GameManager.Instance.isPlayerDie) GameManager.Instance.KillPlayer();
    }

    private void FixedUpdate()
    {
        if (!isStop && !isDead)
        {
            if (!isAttacking && !isRolling)
            {
                // move left-right
                horizontalInput = Input.GetAxisRaw("Horizontal");
                rigidBody.AddForce(Vector2.right * horizontalInput * 2, ForceMode2D.Impulse);

                // When in contact with a moving platform
                if (isTouchPlatform && horizontalInput == 0 && verticalInput == 0 && contactPlatform != null
                && CheckPlatform(contactPlatform.transform.position, contactPlatform.GetComponent<BoxCollider2D>().size))
                {
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

                    if (isTouchPlatform) isTouchPlatform = false;
                }
            }

            // Set max left-right speed
            if (rigidBody.velocity.x > maxSpeed)
                rigidBody.velocity = new Vector2(maxSpeed, rigidBody.velocity.y);
            else if (rigidBody.velocity.x < maxSpeed * (-1))
                rigidBody.velocity = new Vector2(maxSpeed * (-1), rigidBody.velocity.y);

            // cal if contact the floor
            if (Physics2D.OverlapBox(new Vector2(transform.position.x, transform.position.y - groundDetectBox_y)
                , groundDetectBox, 0, LayerMask.GetMask("Platform")))
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
    private void OnCollisionEnter2D(Collision2D collision)
    {
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
                    Camera.main.GetComponent<ProCamera2DShake>().Shake("PlayerHit");
                    SoundPlay_GetDamage();
                }
                // Saw Blade Trap
                else if (obj.objType == _ObjectType.CircleBlade)
                {
                    Knockback(0.75f, obj.transform.position, 0.3f);
                    GameManager.Instance.GetDamage(60, 1f);
                    Camera.main.GetComponent<ProCamera2DShake>().Shake("PlayerHit");
                    SoundPlay_GetDamage();
                }
                // Wooden Box, Moving Platform
                else if (obj.objType == _ObjectType.WoodenBox
                 || obj.objType == _ObjectType.MovingPlatform)
                {
                    if (collision.contacts[0].normal.y > 0.7f)
                        OnEnterPlatform(collision.gameObject);
                }
            }
            else if (collision.gameObject.tag == "Obstacle")
            {
                if (collision.gameObject.name == "String Attack")
                {
                    if (!GameManager.Instance.damageImmune)
                    {
                        Knockback(0.2f, collision.contacts[0].point, 0.1f);
                        GameManager.Instance.GetDamage(33, 1f);
                        Camera.main.GetComponent<ProCamera2DShake>().Shake("PlayerHit");
                        SoundPlay_GetDamage();
                    }
                }
            }
        }
    }

    // Collision Exit Check
    private void OnCollisionExit2D(Collision2D collision)
    {
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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision != null && !GameManager.Instance.isPlayerDie)
        {
            if (collision.gameObject.tag == "Obstacle")
            {
                if (!GameManager.Instance.damageImmune)
                {
                    if (collision.gameObject.name == "String Attack")
                    {
                        if (collision.transform.position.x > transform.position.x)
                            Knockback(0.75f, transform.position + Vector3.left * 0.1f, 0.3f);
                        else
                            Knockback(0.75f, transform.position + Vector3.right * 0.1f, 0.3f);

                        GameManager.Instance.GetDamage(12, 1f);
                        Camera.main.GetComponent<ProCamera2DShake>().Shake("PlayerHit");
                        SoundPlay_GetDamage();
                    }
                    else if (collision.gameObject.name == "Big String Attack")
                    {
                        if (collision.transform.position.x > transform.position.x)
                            Knockback(0.75f, transform.position + Vector3.left * 0.1f, 0.3f);
                        else
                            Knockback(0.75f, transform.position + Vector3.right * 0.1f, 0.3f);

                        GameManager.Instance.GetDamage(36, 1.5f);
                        Camera.main.GetComponent<ProCamera2DShake>().Shake("PlayerHit");
                        SoundPlay_GetDamage();
                    }
                }
            }
        }
    }

    // Player attack object creation
    private void PlayerAttack(int rotate = 0)
    {
        GameObject playerAtk = Instantiate(playerAttackObject, firePoint.position, firePoint.rotation);
        PlayerAttack playerAtkScript = playerAtk.GetComponent<PlayerAttack>();
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

    IEnumerator RunShowAttackDirecion()
    {
        isRunShowAttackDirecion = true;
        Color fixedColor = fireDirection.GetComponent<SpriteRenderer>().color;
        fireDirection.GetComponent<SpriteRenderer>().color = new Color(fixedColor.r, fixedColor.g, fixedColor.b, 1f);

        while (showfireDir > 0)
        {
            yield return new WaitForSeconds(0.01f);
            showfireDir -= 0.05f;

            if (showfireDir < 1)
                fireDirection.GetComponent<SpriteRenderer>().color
                    = new Color(fixedColor.r, fixedColor.g, fixedColor.b, showfireDir);
        }

        isRunShowAttackDirecion = false;
    }

    public void PlayerStop()
    {
        isStop = true;

        if (!isDead)
        {
            isAttack = false;
            isAttacking = false;
            isCreateAttack = false;
            isUpdateAttackCombo = true;
            attackCombo = 1;
            isRolling = false;
        }
    }

    public void PlayerPlay()
    {
        isStop = false;

        if (!isDead)
        {
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
    }

    // Dead
    public void Die(bool fastRespawn = false)
    {
        isDead = true;

        if (!fastRespawn)
        {
            animator.SetBool("isDead", true);
            animator.SetTrigger("isDie");
            Camera.main.GetComponent<ProCamera2DShake>().Shake("SmallExplosion");
        }

        isAttack = false;
        isAttacking = false;
        isCreateAttack = false;
        isUpdateAttackCombo = true;
        attackCombo = 1;
        isRolling = false;
        isTouchPlatform = false;
        contactPlatform = null;
        rigidBody.drag = 3;
        rigidBody.gravityScale = 2;
        verticalInputToggle = true;
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
        SoundManager.Instance.Play(SoundManager.AS.playerAction1, ACname);
    }

    public void SoundPlay_Action2(string ACname)
    {
        SoundManager.Instance.Play(SoundManager.AS.playerAction2, ACname);
    }

    public void SoundPlay_Walk()
    {
        SoundManager.Instance.Play(SoundManager.AS.playerAction1, (SoundManager.PlayerAction)RandomSoundIndex(preIndex_walk
            , (int)SoundManager.PlayerAction.walk1, 6));
    }

    public void SoundPlay_Walk2()
    {
        SoundManager.Instance.Play(SoundManager.AS.playerAction2, (SoundManager.PlayerAction)RandomSoundIndex(preIndex_walk
            , (int)SoundManager.PlayerAction.walk1, 6));
    }

    public void SoundPlay_GetDamage()
    {
        SoundManager.Instance.Play(SoundManager.AS.playerAction3, (SoundManager.PlayerAction)RandomSoundIndex(preIndex_getDamage
            , (int)SoundManager.PlayerAction.damage1, 3));
    }

    private int RandomSoundIndex(int pre, int startIndex, int maxIndex)
    {
        int index = Random.Range(0, maxIndex);

        if (index == pre) index += 1;
        if (index > maxIndex - 1) index = 0;

        pre = index;

        return startIndex + index;
    }

    // Collision with moving platform
    private void OnEnterPlatform(GameObject gameObject)
    {
        if (!isTouchPlatform && CheckPlatform(gameObject.transform.position, gameObject.GetComponent<BoxCollider2D>().size))
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
        if (transform.position.y - platformPos.y > (boxCollider2D.size.y + platformSize.y) / 2 - 0.1f
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

        while (timer <= duration)
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
    public void SetDamageImmuneMode(int val)
    {
        GameManager.Instance.damageImmune = val == 1;
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

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(new Vector2(transform.position.x, transform.position.y - groundDetectBox_y), groundDetectBox);
        Gizmos.DrawLine(transform.position, firePoint.position);
        Gizmos.DrawSphere(firePoint.position, 0.05f);
    }
}
