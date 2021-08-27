using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get { return _instance; } }
    public bool isPause = false;
    public Text deatCounter;
    public Text playTime;
    public Slider healthBar;
    public Slider staminaBar;
    public Slider angerBar;
    public Text iconDescription;
    public Text powerText;
    public Image angerEffect;
    public Text warnningText;
    public int deathCount;
    public int maxHealth = 100;         //HP 최대값
    public int health = 100;            //HP 현재값
    public int maxStamina = 100;        //스테미나 최대값
    public int stamina = 100;           //스테미나 현재값
    public int angerLevel = 0;          //분노 에너지
    public int angerCharged = 0;        //분노 충전값
    public int power = 10;              //파워(데미지)
    public int fixedPower;              //수정된 파워
    public bool isGodMode;              //무적 모드
    public bool isPlayerDie;
    public Vector3 respawnPosition = new Vector3(0f, 0f);
    public Color defaultColor = new Color(0.0001378677f, 0.0003025429f, 0.0007314645f);
    public Color rageColor = new Color(1.210474f, 2.656317f, 6.422235f);

    private static GameManager _instance;
    private GameObject player;
    private SpriteRenderer playerRenderer;
    private float playerMaterialIntensity;
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
        isGodMode = false;
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
            if (health <= 0 || player.GetComponent<Transform>().position.y < -64)
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

                    if (staminaRegenTimer > 0.2f)
                    {
                        staminaRegenTimer = 0;
                        stamina += 3;

                        if (stamina > maxStamina)
                            stamina = maxStamina;
                    }
                }
            }
            else
                staminaRegenCool -= Time.deltaTime;

            //체력, 스태미나, 분노 게이지
            healthBar.value = (float)health / (float)maxHealth;

            staminaBar.value = (float)stamina / (float)maxStamina;

            if (angerCharged == 0)
                angerBar.value = (float)angerLevel / 100f;
            else
                angerBar.value = (float)angerCharged / 100f;
        }

        //전체화면 토글
        if (Input.GetKeyDown(KeyCode.Escape))
            Screen.fullScreen = !Screen.fullScreen;

        //경고 메세지
        if(warnningTimer > 0)
        {
            warnningTimer -= Time.deltaTime;
        } else if (warnningTimer <= 0 && warnningTimer > -99f)
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

    void PlayerDie()        // 플레이어 죽음
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
        isGodMode = false;
        UpdatePlayerMaterialColor();
    }

    public void PlayerRespawn()     //플레이어 리스폰
    {
        isPlayerDie = false;
        health = maxHealth;
        stamina = maxStamina;
    }

    public void ShowWarnning(string str, float time = 2f)       //경고 메세지 보이기
    {
        warnningTimer = time;
        warnningText.text = str;
    }

    public void UpdateIconDescription(string str)       //아이콘 설명글 업데이트
    {
        iconDescription.text = str;
    }

    public bool consumeStamina(int val, bool isReduce = true)       //스태미나 소모 계산
    {
        if (stamina >= val)
        {
            if (isReduce)
            {
                stamina -= val;
                staminaRegenCool = 2f;
                staminaRegenTimer = 0;
            }
            
            return true;
        } else
            return false;
    }

    public void increaseAngerLevel(int val)      // 분노 게이지 증가
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

    void UpdatePlayerMaterialColor()        // 플레이어 메테리얼 HDR Color 업데이트
    {
        Color fixedColor = new Color(
            rageColor.r * playerMaterialIntensity
            , rageColor.g * playerMaterialIntensity
            , rageColor.b * playerMaterialIntensity);

        playerRenderer.material.SetColor("_Color", fixedColor);
    }


    IEnumerator RunAngerCountdown()     //분노 게이지 쿨다운 코루틴
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

    IEnumerator RunIintensityUpdate(bool isUpDown)      // 플레이어 메테리얼 HDR Color 강도 조절 코루틴
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
