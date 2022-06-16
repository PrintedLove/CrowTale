using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get { return _instance; } }
    
    [Header("UI")]
    [SerializeField] Text deatCounter;
    [SerializeField] Text playTime;
    [SerializeField] Slider healthBar;
    [SerializeField] Slider staminaBar;
    [SerializeField] Slider angerBar;
    [SerializeField] Text iconDescription;
    [SerializeField] Text powerText;
    [SerializeField] Image angerEffect;
    [SerializeField] Text warnningText;

    [Header("플레이어 변수")]
    public bool isPause = false;        //일시 정지 여부
    public int maxHealth = 100;         //HP 최대값
    public int health = 100;            //HP 현재값
    public int maxStamina = 100;        //스테미나 최대값
    public int stamina = 100;           //스테미나 현재값
    public int angerLevel = 0;          //분노 에너지
    public int angerCharged = 0;        //분노 충전값
    public int power = 10;              //파워(데미지)
    [HideInInspector] public int fixedPower;        //수정된 파워
    public bool damageImmune;           //무적
    public bool isPlayerDie;            //플레이어 죽음 여부
    public Vector3 respawnPosition = new Vector3(0f, 0f, 0f);       //리스폰 좌표


    [HideInInspector] public Color defaultColor
     = new Color(0.0001378677f, 0.0003025429f, 0.0007314645f);      // 기본색
    [HideInInspector] public Color rageColor
     = new Color(1.210474f, 2.656317f, 6.422235f);      // 분노시 변할 색

    private static GameManager _instance;
    private GameObject player;
    private SpriteRenderer playerRenderer;
    private float playerMaterialIntensity;
    private int deathCount;          //플레이어 죽음 횟수
    private float _playTimeSec;      //플레이 타임
    private int _playTimeMin;
    private float staminaRegenCool = 0;
    private float staminaRegenTimer = 0;
    private float warnningTimer;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        _instance = this;

        DontDestroyOnLoad(this.gameObject);

        _playTimeSec = 0;
        _playTimeMin = 0;
        playerMaterialIntensity = 0;
        fixedPower = power;
        deathCount = 0;
        damageImmune = false;
        isPlayerDie = false;

        player = GameObject.FindWithTag("Player");
        playerRenderer = player.GetComponent<SpriteRenderer>();

        Screen.fullScreen = true;
    }

    private void Update()
    {
        if (!isPlayerDie)
        {
            //죽음 감시
            if (health <= 0)
            {
                player.GetComponent<playerManager>().Die();
                PlayerDie();
            }

            //스태미나 리젠 계산
            if (staminaRegenCool <= 0)
            {
                if (stamina < maxStamina)
                {
                    staminaRegenTimer += Time.deltaTime;

                    if (staminaRegenTimer > 0.12f)
                    {
                        staminaRegenTimer = 0;
                        stamina += 4;

                        if (stamina > maxStamina) stamina = maxStamina;
                    }
                }
            }
            else
                staminaRegenCool -= Time.deltaTime;

            //체력, 스태미나, 분노 게이지
            healthBar.value = (float)health / (float)maxHealth;
            staminaBar.value = (float)stamina / (float)maxStamina;
            angerBar.value = angerCharged == 0 ? (float)angerLevel / 100f : (float)angerCharged / 100f;
        }

        //전체화면 토글
        if (Input.GetKeyDown(KeyCode.Escape)) Screen.fullScreen = !Screen.fullScreen;

        //경고 메세지
        if(warnningTimer > 0)
            warnningTimer -= Time.deltaTime;
        else if (warnningTimer <= 0 && warnningTimer > -99f)
        {
            warnningText.text = "";
            warnningTimer = -100f;
        }

        //플레이 타이머
        _playTimeSec += Time.deltaTime;
        if (_playTimeSec >= 60)
        {
            _playTimeSec = 0;
            _playTimeMin++;
        }
        string timeStr;
        timeStr = _playTimeSec.ToString("00");
        timeStr = timeStr.Replace(".", " : ");
        playTime.text = "Play Time   " + _playTimeMin.ToString("00") + " : " + timeStr;
    }

    // 플레이어 데미지
    public void GetDamage(int damage, float immuneTime) {
        if (!damageImmune) {
            health -= damage;
            damageImmune = true;
            StartCoroutine(RunDamageImmuneTime(immuneTime));
        }
    }

    // 플레이어 죽음
    void PlayerDie()
    {
        isPlayerDie = true;
        deathCount += 1;
        deatCounter.text = "Death   " + deathCount.ToString();
        ShowWarnning("You Die", 3f);

        health = 0;     
        stamina = 0;           
        angerLevel = 0;          
        angerCharged = 0;        
        fixedPower = power;
        playerMaterialIntensity = 0f;
        damageImmune = false;
        UpdatePlayerMaterialColor();
    }

    //플레이어 리스폰
    public void PlayerRespawn()
    {
        isPlayerDie = false;
        health = maxHealth;
        stamina = maxStamina;
    }

    //경고 메세지 보이기
    public void ShowWarnning(string str, float time = 2f)
    {
        warnningTimer = time;
        warnningText.text = str;
    }

    //아이콘 설명글 업데이트
    public void UpdateIconDescription(string str)
    {
        iconDescription.text = str;
    }

    //스태미나 소모 계산
    public bool consumeStamina(int val, bool isReduce = true)
    {
        if (stamina >= val)
        {
            if (isReduce)
            {
                stamina -= val;
                staminaRegenCool = 1.6f;
                staminaRegenTimer = 0;
            }
            
            return true;
        } else
            return false;
    }

    // 분노 게이지 증가
    public void increaseAngerLevel(int val)
    {
        if (angerCharged == 0)
        {
            angerLevel += val;

            if (angerLevel >= 100)
            {
                angerCharged = 100;
                angerLevel = 0;
                fixedPower = power * 2;
                powerText.text = fixedPower.ToString();
                angerEffect.gameObject.SetActive(true);
                StartCoroutine(RunAngerCountdown());
                StartCoroutine(RunIintensityUpdate(true));
            }
        }
    }

    // 플레이어 메테리얼 HDR Color 업데이트
    void UpdatePlayerMaterialColor()
    {
        Color fixedColor = new Color(
            rageColor.r * playerMaterialIntensity
            , rageColor.g * playerMaterialIntensity
            , rageColor.b * playerMaterialIntensity);

        playerRenderer.material.SetColor("_Color", fixedColor);
    }

    //무적 시간 코루틴
    IEnumerator RunDamageImmuneTime(float immuneTime)
    {
        yield return new WaitForSeconds(immuneTime);

        damageImmune = false;
    }

    //분노 게이지 쿨다운 코루틴
    IEnumerator RunAngerCountdown()
    {
        while(angerCharged > 0)
        {
            yield return new WaitForSeconds(0.12f);
            angerCharged --;
        }

        fixedPower = power;
        powerText.text = fixedPower.ToString();
        angerEffect.gameObject.SetActive(false);
        StartCoroutine(RunIintensityUpdate(false));
    }

    // 플레이어 메테리얼 HDR Color 강도 조절 코루틴
    IEnumerator RunIintensityUpdate(bool isUpDown)
    {
        if (isUpDown)
        {
            while (playerMaterialIntensity < 1f)
            {
                yield return new WaitForSeconds(0.01f);

                playerMaterialIntensity += 0.05f;
                UpdatePlayerMaterialColor();
            }
        } else
        {
            while (playerMaterialIntensity > 0)
            {
                yield return new WaitForSeconds(0.01f);

                playerMaterialIntensity -= 0.05f;
                UpdatePlayerMaterialColor();
            }
        }
    }
}

public class GameContorl
{
    public void IsPause()
    {
        GameManager.Instance.isPause = true;
    }
}
