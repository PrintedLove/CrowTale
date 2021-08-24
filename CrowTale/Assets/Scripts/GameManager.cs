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
    private float _playTimeSec;      //�÷��� Ÿ��
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
    public int maxHealth = 100;         //HP �ִ밪
    public int health = 100;            //HP ���簪
    public int maxStamina = 100;        //���׹̳� �ִ밪
    public int stamina = 100;           //���׹̳� ���簪
    public int angerLevel = 0;          //�г� ������
    public int angerCharged = 0;        //�г� ������
    public int power = 10;              //�Ŀ�(������)
    public int fixedPower;              //������ �Ŀ�

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
        //�÷��� Ÿ�̸�
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

        //ü��, ���¹̳�, �г� ������
        healthBar.value = (float)health / (float)maxHealth;
        staminaBar.value = (float)stamina / (float)maxStamina;
        if (angerCharged == 0)
            angerBar.value = (float)angerLevel / 100f;
        else
            angerBar.value = (float)angerCharged / 100f;
        

        //�г� ������ ����
        if (angerLevel >= 100)
        {
            angerCharged = 100;
            angerLevel = 0;
            fixedPower = power * 2;
            powerText.text = fixedPower.ToString();
            StartCoroutine(RunAngerCountdown());
            StartCoroutine(RunIintensityUpdate(true));
        }

        //��üȭ�� ���
        if (Input.GetKeyDown(KeyCode.Escape))
            Screen.fullScreen = !Screen.fullScreen;
    }

    void UpdatePlayerMaterialColor()        // �÷��̾� ���׸��� HDR Color ������Ʈ
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

    IEnumerator RunAngerCountdown()     //�г� ������ ��ٿ� �ڷ�ƾ
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

    IEnumerator RunIintensityUpdate(bool isUpDown)      // �÷��̾� ���׸��� HDR Color ���� ���� �ڷ�ƾ
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
