using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class GameManager : MonoBehaviour
{

    private static GameManager _instance;
    private GameObject player;
    private SpriteRenderer playerRenderer;
    private Color playerMaterialColor;
    private float playerMaterialIntensity;
    private float _playTimeSec;      //플레이 타임
    private int _playTimeMin;

    public static GameManager Instance { get { return _instance; } }
    public bool isPause = false;
    public Text deatCounter;
    public Text playTime;
    public Slider healthBar;
    public Slider staminaBar;
    public Slider angerBar;
    public Text powerText;
    public int deathCount;
    public int maxHealth = 100;         //HP 최대값
    public int health = 100;            //HP 현재값
    public int maxStamina = 100;        //스테미나 최대값
    public int stamina = 100;           //스테미나 현재값
    public int angerLevel = 0;          //분노 에너지
    public int angerCharged = 0;        //분노 충전값
    public int power = 10;              //파워(데미지)
    public int fixedPower;              //수정된 파워

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
        playerMaterialIntensity = -10f;
        fixedPower = power;
        deathCount = 0;

        player = GameObject.FindWithTag("Player");
        playerRenderer = player.GetComponent<SpriteRenderer>();
        playerMaterialColor = new Color(80, 119, 191);
    }

    private void Update()
    {
        //플레이 타이머
        _playTimeSec += Time.deltaTime;
        if (_playTimeSec >= 60)
        {
            _playTimeSec = 0;
            _playTimeMin ++;
        }
        string timeStr;
        timeStr = _playTimeSec.ToString("00");
        timeStr = timeStr.Replace(".", " : ");
        playTime.text = "Play Time   " + _playTimeMin.ToString("00") + " : " + timeStr;

        //체력, 스태미나, 분노 게이지
        healthBar.value = (float)health / (float)maxHealth;
        staminaBar.value = (float)stamina / (float)maxStamina;
        if (angerCharged == 0)
            angerBar.value = (float)angerLevel / 100f;
        else
            angerBar.value = (float)angerCharged / 100f;
        

        //분노 게이지 감시
        if (angerLevel >= 100)
        {
            angerCharged = 100;
            angerLevel = 0;
            fixedPower = power * 2;
            powerText.text = fixedPower.ToString();
            StartCoroutine(RunAngerCountdown());
            StartCoroutine(RunIintensityUpdate(true));
        }

        //전체화면 토글
        if (Input.GetKeyDown(KeyCode.Escape))
            Screen.fullScreen = !Screen.fullScreen;
    }

    void UpdatePlayerMaterialColor()        // 플레이어 메테리얼 HDR Color 업데이트
    {
        float factor = 1f / playerMaterialIntensity;

        Color fixedColor = new Color(
            playerMaterialColor.r * factor, playerMaterialColor.g * factor, playerMaterialColor.b * factor);

        playerRenderer.material.SetColor("_Color", fixedColor);
    }

    void PlayerDie()
    {
        deathCount++;
        playTime.text = "Death   " + deathCount.ToString();
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
        StartCoroutine(RunIintensityUpdate(false));
    }

    IEnumerator RunIintensityUpdate(bool isUpDown)      // 플레이어 메테리얼 HDR Color 강도 조절 코루틴
    {
        if (isUpDown)
        {
            while (playerMaterialIntensity < 6.0f)
            {
                yield return new WaitForSeconds(0.01f);

                playerMaterialIntensity += 0.1f;
                UpdatePlayerMaterialColor();
            }
        } else
        {
            while (playerMaterialIntensity > (-10f))
            {
                yield return new WaitForSeconds(0.01f);

                playerMaterialIntensity -= 0.1f;
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
