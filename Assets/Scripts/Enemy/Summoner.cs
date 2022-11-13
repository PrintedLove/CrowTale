using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.UI;
using static Summoner;
using static UnityEngine.GraphicsBuffer;
using Random = UnityEngine.Random;

public class Summoner : _Object
{
    [SerializeField] private GameObject dialogUI;

    //boss act pattern list
    public enum Act
    {
        Sleep, Talk, Stand,
        Combat_start, Combat, Combat_Testing,
        CombatAfter
    }

    public enum MoveMode
    {
        right, left, center, playerTrace
    }

    [Space]
    [Header("- - - - - BOSS - - - - -")]
    public Act action;
    [SerializeField] private Vector3 startPosition;
    [SerializeField] private MoveMode moveMode;
    [SerializeField] private float moveSpeed;
    [SerializeField] private GameObject summonStoneAltar, messageIcon;
    [SerializeField] private Light2D lt;
    [SerializeField] private GameObject[] summonStones;
    [SerializeField] private GameObject[] strings;
    [SerializeField] private GameObject stringBig;
    [SerializeField] private GameObject[] beats;
    [SerializeField] private GameObject blockString;
    [SerializeField] private GameObject combatWall, hpBar;
    [SerializeField] private Vector2 moveBoxPosition, moveBoxSize;
    [SerializeField] private Vector2 stringBoxPosition, stringBoxSize;

    protected Animator animator;
    protected SpriteRenderer spriteRenderer;

    private GameObject player;
    private float StoneTriangle_Len = 5f;     //summon stone triangle one side lengh
    private Vector3[] StoneTriangle_startPosition
        = new Vector3[3] {new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 0, 0) };    //summon stone triangle start position
    private bool 
        isCombat,
        isTalking,
        isPatternOperate_String,
        isPatternOperate_BigString,
        isPatternOperate_Beat,
        isPatternOperate_Stone,
        lazerPatternCycle;
    private float 
        stoneRotateSpeed, 
        playerTowardSpeed,
        PatternSpeed;
    private short 
        phase, 
        lazerShootMode,
        stringTraceNum,
        stringRandomNum;
    private Vector3 pos_right, pos_left, pos_center;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        player = GameObject.FindWithTag("Player");
        objType = _ObjectType.Summoner;
        isHit = false;
        isCombat = false;
        isTalking = false;
        stoneRotateSpeed = 0.5f; 
        playerTowardSpeed = 0.5f;
        PatternSpeed = 1f;
        phase = 1;
        isPatternOperate_String = false;
        isPatternOperate_BigString = false;
        isPatternOperate_Beat = false;
        isPatternOperate_Stone = false;
        stringTraceNum = 3;
        stringRandomNum = 4;
        lazerPatternCycle = false;
        lazerShootMode = 0;
        pos_right = new Vector3(moveBoxPosition.x + moveBoxSize.x / 2f, moveBoxPosition.y, 0);
        pos_left = new Vector3(moveBoxPosition.x - moveBoxSize.x / 2f, moveBoxPosition.y, 0);
        pos_center = new Vector3(moveBoxPosition.x, moveBoxPosition.y, 0);
    }

    private void Reset()
    {
        StopAllCoroutines();

        isHit = false;
        isCombat = false;
        stoneRotateSpeed = 0.5f;
        playerTowardSpeed = 0.5f;
        PatternSpeed = 1f;
        phase = 1;
        isPatternOperate_String = false;
        isPatternOperate_BigString = false;
        isPatternOperate_Beat = false;
        isPatternOperate_Stone = false;
        stringTraceNum = 3;
        stringRandomNum = 4;
        lazerPatternCycle = false;
        lazerShootMode = 0;
        
        health = maxHealth; CheckHP();
        lt.intensity = 0f;

        hpBar.SetActive(false);
        messageIcon.SetActive(true);
        messageIcon.GetComponent<MessageIconController>().ResetMessageIcon();

        for (int i = 0; i < summonStones.Length; i++)
        {
            summonStones[i].transform.rotation = Quaternion.identity;
            summonStones[i].GetComponent<SummonStone>().ResetSummonStone();
            summonStones[i].SetActive(false);
        }

        for (int i = 0; i < strings.Length; i++)
            strings[i].SetActive(false);

        stringBig.SetActive(false);

        for (int i = 0; i < beats.Length; i++)
            beats[i].SetActive(false);
    }

    private void Update()
    {
        if (action == Act.Sleep)
        {
            //talk button
            if (Input.GetKeyDown(KeyCode.T) && messageIcon.GetComponent<MessageIconController>().show)
            {
                GameManager.Instance.ShowDialogUI("Talk_summonerMeet");
                messageIcon.SetActive(false);
                animator.ResetTrigger("CombatReset");
                action = Act.Talk;
            }
        }
        else if (action == Act.Talk)
        {
            //standing
            if (Input.GetKeyDown(KeyCode.Escape) || dialogUI.GetComponent<DialogUIController>().talkCounter == 6)
            {
                animator.SetTrigger("wakeUp");
                action = Act.Stand;
            }
        }
        else if (action == Act.Stand)
        {
            if(!dialogUI.activeSelf)
            {
                action = Act.Combat_start;
                StartCoroutine(RunStartCombat());
                combatWall.SetActive(true);
                SoundManager.Instance.ChangeBGM(SoundManager.BGM.TheWitch, 0.8f);
            }
        }
        else if (action == Act.Combat)
        {
            if (!isPatternOperate_String)
            {
                if (Random.value > 0.5)
                    StartCoroutine(RunPattern_StringTrace(stringTraceNum));
                else
                    StartCoroutine(RunPattern_StringRandom(stringRandomNum));
            }

            if (!isPatternOperate_Beat) StartCoroutine(RunPattern_BeatsSpon());

            if (phase > 1)    // boss phase 2 (HP <= 60%)
            {
                if (!isPatternOperate_BigString) StartCoroutine(RunPattern_BigString());
            }

            if (phase > 2)    // boss phase 3 (HP <= 20%)
            {
                if (!isPatternOperate_Stone)
                {
                    if (lazerPatternCycle)
                        StartCoroutine(RunPattern_StoneLazerCicle());
                    else
                        StartCoroutine(RunPattern_StoneLazerGround());
                    lazerPatternCycle = !lazerPatternCycle;
                }
            }
        }
        else if (action == Act.Combat_Testing)
        {
            animator.SetTrigger("TestMode");
            combatWall.SetActive(true);
            SoundManager.Instance.ChangeBGM(SoundManager.BGM.TheWitch, 0.8f);

            StartCombat();
        }
        else if (action == Act.CombatAfter)
        {
            //talk button
            if (!isTalking && Input.GetKeyDown(KeyCode.T) && messageIcon.GetComponent<MessageIconController>().show)
            {
                GameManager.Instance.ShowDialogUI("Talk_viperFirstMeet");
                messageIcon.SetActive(false);
                isTalking = true;
            }

            //talk end or press Esc
            if (isTalking && dialogUI.gameObject.activeSelf)
            {
                isTalking = false;
                messageIcon.SetActive(true);
                combatWall.SetActive(false);
                if (blockString.activeSelf) blockString.GetComponent<SummonerString>().Down();      //stage blocker get down
            }
        }
    }

    private void FixedUpdate()
    {
        if (isCombat)
        {
            //rotate summon stones
            foreach (GameObject summonStone in summonStones)
            {
                summonStone.transform.RotateAround(transform.position, Vector3.back, stoneRotateSpeed);

                if (lazerShootMode == 1)
                {
                    summonStone.transform.Find("Shooter").transform.rotation
                        = Quaternion.AngleAxis(Vector3.Angle(transform.position, summonStone.transform.position) - 90, Vector3.forward);
                }
                else if (lazerShootMode == 2)
                {
                    float angle = Mathf.Atan2(summonStone.transform.position.y - transform.position.y,
                        summonStone.transform.position.x - transform.position.x) * Mathf.Rad2Deg;
                    summonStone.transform.Find("Shooter").transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
                }

                summonStone.transform.Rotate(-Vector3.back * stoneRotateSpeed);
            }

            // move mode
            if (moveMode == MoveMode.playerTrace)
            {
                moveSpeed = playerTowardSpeed;

                Vector3 fixedPlayerPos = player.transform.position + new Vector3(0, 5f, 0);
                if (fixedPlayerPos.x > moveBoxPosition.x + moveBoxSize.x / 2f)
                    fixedPlayerPos.x = moveBoxPosition.x + moveBoxSize.x / 2f;
                else if (fixedPlayerPos.x < moveBoxPosition.x - moveBoxSize.x / 2f)
                    fixedPlayerPos.x = moveBoxPosition.x - moveBoxSize.x / 2f;
                if (fixedPlayerPos.y > moveBoxPosition.y + moveBoxSize.y / 2f)
                    fixedPlayerPos.y = moveBoxPosition.y + moveBoxSize.y / 2f;
                else if (fixedPlayerPos.y < moveBoxPosition.y - moveBoxSize.y / 2f)
                    fixedPlayerPos.y = moveBoxPosition.y - moveBoxSize.y / 2f;

                transform.position = Vector3.Lerp(transform.position, fixedPlayerPos, moveSpeed * Time.deltaTime);
            }
            else if (moveMode == MoveMode.right)
            {
                transform.position = Vector3.Lerp(transform.position, pos_right, moveSpeed * Time.deltaTime);
                moveSpeed = 2.5f;
            }
            else if (moveMode == MoveMode.left)
            {
                transform.position = Vector3.Lerp(transform.position, pos_left, moveSpeed * Time.deltaTime);
                moveSpeed = 2.5f;
            }
            else if (moveMode == MoveMode.center)
            {
                transform.position = Vector3.Lerp(transform.position, pos_center, moveSpeed * Time.deltaTime);
                moveSpeed = 2.5f;
            }
        }
    }

    public override void TakeDamage(int damage, float hitDir)
    {
        health -= damage;

        GameManager.Instance.increaseAngerLevel(angerAmount);

        if (health <= 0 && !isDie)
        {
            Die();
        }

        CheckHP();
    }

    public override void CheckHP()
     {
        hpBar.GetComponent<Slider>().value = (float)health / (float)maxHealth;

        if ((float)health / maxHealth <= 0.6f && phase == 1)
        {
            phase = 2;
            stoneRotateSpeed = 0.85f;
            playerTowardSpeed = 0.8f;
            PatternSpeed = 0.9f;
        }
        else if ((float)health / maxHealth <= 0.2f && phase == 2)
        {
            phase = 3;
            stoneRotateSpeed = 1.5f;
            playerTowardSpeed = 1.25f;
            PatternSpeed = 0.75f;
            stringTraceNum = 4;
            stringRandomNum = 6;

            for (int i = 0; i < summonStones.Length; i++)
                summonStones[i].GetComponent<Animator>().SetBool("transform", true);
        }
    }

    protected override void Die()
    {
        isDie = true;
        hpBar.SetActive(false);
        GetComponent<BoxCollider2D>().enabled = false;
        action = Act.CombatAfter;
        animator.SetTrigger("CombatAfter");

        SoundManager.Instance.ChangeBGM(SoundManager.BGM.Bittersweet, 0.5f);

        Reset();
        StartCoroutine(RunDie());
    }

    IEnumerator RunPattern_StringRandom(int stringNum)
    {
        isPatternOperate_String = true;

        if (phase < 3)
        {
            moveMode = MoveMode.center;
        }
        yield return new WaitForSeconds(1f);

        for (int i = 0; i < stringNum; i++)
        {
            Vector2 coord1, coord2;
            coord1 = new Vector2(stringBoxPosition.x - stringBoxSize.x / 2f + stringBoxSize.x * Random.value
                , stringBoxPosition.y - stringBoxSize.y / 2);
            coord2 = new Vector2(stringBoxPosition.x - stringBoxSize.x / 2f + stringBoxSize.x * Random.value
                , stringBoxPosition.y + stringBoxSize.y / 2);
            float angle = Mathf.Atan2(coord2.y - coord1.y, coord2.x - coord1.x) * Mathf.Rad2Deg;

            strings[i].SetActive(true);
            strings[i].transform.position = coord1;
            strings[i].transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

            SummonerString ss = strings[i].GetComponent<SummonerString>();
            ss.stringLength = Vector2.Distance(coord1, coord2);
            ss.lifeTime = 2.5f * PatternSpeed;
            ss.Grow();
        }

        yield return new WaitForSeconds(10f * PatternSpeed);
        isPatternOperate_String = false;
    }

    IEnumerator RunPattern_StringTrace(int stringNum)
    {
        isPatternOperate_String = true;

        if (phase < 3)
            moveMode = MoveMode.playerTrace;
        yield return new WaitForSeconds(1f);

        for (int i = 0; i < stringNum; i++)
        {
            Vector2 coord1, coord2;
            coord1 = new Vector2(player.transform.position.x, stringBoxPosition.y - stringBoxSize.y / 2);
            coord2 = new Vector2(stringBoxPosition.x - stringBoxSize.x / 2f + stringBoxSize.x * Random.value
                , stringBoxPosition.y + stringBoxSize.y / 2);
            float angle = Mathf.Atan2(coord2.y - coord1.y, coord2.x - coord1.x) * Mathf.Rad2Deg;

            strings[i].SetActive(true);
            strings[i].transform.position = coord1;
            strings[i].transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

            SummonerString ss = strings[i].GetComponent<SummonerString>();
            ss.stringLength = Vector2.Distance(coord1, coord2);
            ss.lifeTime = 0.85f * PatternSpeed;
            ss.Grow();

            yield return new WaitForSeconds(1f * PatternSpeed);
        }

        yield return new WaitForSeconds(10f * PatternSpeed);
        isPatternOperate_String = false;
    }

    IEnumerator RunPattern_BigString()
    {
        isPatternOperate_BigString = true;
        stringBig.SetActive(true);
        stringBig.transform.position = new Vector2(player.transform.position.x, stringBoxPosition.y - 30f);

        SummonerString ss = stringBig.GetComponent<SummonerString>();
        ss.stringLength = 60f;
        ss.lifeTime = 10f * PatternSpeed;
        ss.Grow();

        yield return new WaitForSeconds(17.5f * PatternSpeed);
        isPatternOperate_BigString = false;
    }

    IEnumerator RunPattern_StoneLazerGround()
    {
        isPatternOperate_Stone = true;
        moveMode = MoveMode.playerTrace;
        yield return new WaitForSeconds(1f);

        lazerShootMode = 1;
        foreach (GameObject summonStone in summonStones)
            summonStone.transform.Find("Shooter").transform.rotation
                        = Quaternion.AngleAxis(Vector3.Angle(transform.position, summonStone.transform.position) - 90, Vector3.forward);

        yield return new WaitForSeconds(0.01f);

        foreach (GameObject summonStone in summonStones)
            summonStone.GetComponent<SummonStone>().StartLazer();

        yield return new WaitForSeconds(3f);

        lazerShootMode = 0;
        foreach (GameObject summonStone in summonStones)
            summonStone.GetComponent<SummonStone>().EndLazer();

        yield return new WaitForSeconds(3f);
        isPatternOperate_Stone = false;
    }

    IEnumerator RunPattern_StoneLazerCicle()
    {
        isPatternOperate_Stone = true;
        moveMode = MoveMode.center;
        yield return new WaitForSeconds(1f);

        lazerShootMode = 2;
        foreach (GameObject summonStone in summonStones)
        {
            float angle = Mathf.Atan2(summonStone.transform.position.y - transform.position.y,
                        summonStone.transform.position.x - transform.position.x) * Mathf.Rad2Deg;
            summonStone.transform.Find("Shooter").transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }

        yield return new WaitForSeconds(0.01f);

        foreach (GameObject summonStone in summonStones)
            summonStone.GetComponent<SummonStone>().StartLazer();

        yield return new WaitForSeconds(2.5f);

        lazerShootMode = 0;
        foreach (GameObject summonStone in summonStones)
            summonStone.GetComponent<SummonStone>().EndLazer();

        yield return new WaitForSeconds(3f);
        isPatternOperate_Stone = false;
    }  

    IEnumerator RunPattern_BeatsSpon()
    {
        isPatternOperate_Beat = true;

        for(int i = 0; i < beats.Length; i++)
        {
            if (!beats[i].activeSelf)
            {
                beats[i].SetActive(true);
                beats[i].transform.position = summonStones[i].transform.position;
            }
        }

        yield return new WaitForSeconds(10f * PatternSpeed);
        isPatternOperate_Beat = false;
    }

    IEnumerator RunStartCombat()
    {
        //transform summon stone altar
        yield return new WaitForSeconds(2.25f);
        animator.SetTrigger("transformAltar");

        //summoner fly start
        yield return new WaitForSeconds(1.75f);
        animator.SetTrigger("fly start");

        int flyTimer = 200;
        float flyDis = 0;
        while (flyTimer > 0)
        {
            yield return new WaitForSeconds(0.01f);

            SetSSTStartPosition();
            for (int i = 0; i < summonStones.Length; i++)
            {
                summonStones[i].transform.position
                    = Vector3.Lerp(summonStones[i].transform.position, StoneTriangle_startPosition[i], 0.03f);
            }
            if (flyDis < 5f) transform.Translate(Vector3.up * 0.025f); flyDis += 0.025f;
            if (lt.intensity < 0.3f) lt.intensity += 0.01f;

            flyTimer--;
        }
        
        yield return new WaitForSeconds(1f);
        StartCombat();
    }

    IEnumerator RunDie()
    {
        Vector3 aimPos = new Vector3(transform.position.x, startPosition.y, transform.position.z);
        while (transform.position != aimPos)
        {
            transform.position = Vector3.MoveTowards(transform.position, aimPos, 0.2f);
            yield return new WaitForSeconds(0.01f);
        }
        transform.position = new Vector3(transform.position.x, startPosition.y, transform.position.z);
    }

    private void StartCombat()  //summonre combat start
    {
        SetSSTStartPosition();
        for (int i = 0; i < summonStones.Length; i++)
            summonStones[i].transform.position = StoneTriangle_startPosition[i];

        action = Act.Combat;
        moveMode = MoveMode.playerTrace;
        isHit = true;
        isCombat = true;
        hpBar.SetActive(true);
        Text hpbarText = hpBar.transform.Find("Text").GetComponent<Text>();
        hpbarText.font = GameManager.Instance.customFont;
        hpbarText.text = GameManager.Instance.LoadTranslatedText("Meta", 4);
    }

    public void TransformAltar()
    {
        summonStoneAltar.SetActive(false);
        summonStones[0].SetActive(true);
        summonStones[1].SetActive(true);
        summonStones[2].SetActive(true);
    }

    private void SetSSTStartPosition()  //set summon stone triangle start position
    {
        float sqrt3 = Mathf.Sqrt(3);
        StoneTriangle_startPosition[0] = transform.position + new Vector3(StoneTriangle_Len / 2f, -sqrt3 / 6f * StoneTriangle_Len, 0);
        StoneTriangle_startPosition[1] = transform.position + new Vector3(0, sqrt3 / 3f * StoneTriangle_Len, 0);
        StoneTriangle_startPosition[2] = transform.position + new Vector3(-StoneTriangle_Len / 2f, -sqrt3 / 6f * StoneTriangle_Len, 0);
    }

    public void ResetSummoner()
    {
        if (action != Act.Sleep && !isDie)
        {
            if (action == Act.Combat || action == Act.Combat_start)
                SoundManager.Instance.ChangeBGM(SoundManager.BGM.Bittersweet, 0.5f);

            action = Act.Sleep;
            transform.position = startPosition;
            animator.SetTrigger("CombatReset");
            animator.ResetTrigger("wakeUp");
            animator.ResetTrigger("transformAltar");
            animator.ResetTrigger("fly start");
            summonStoneAltar.SetActive(true);
            combatWall.SetActive(false);

            Reset();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(moveBoxPosition, moveBoxSize);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(stringBoxPosition, stringBoxSize);
    }
}
