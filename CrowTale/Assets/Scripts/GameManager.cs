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
    public int maxHealth = 100;         //HP �ִ밪
    public int health = 100;            //HP ���簪
    public int maxStamina = 100;        //���׹̳� �ִ밪
    public int stamina = 100;           //���׹̳� ���簪
    public int angerLevel = 0;          //�г� ������
    public int angerCharged = 0;        //�г� ������
    public int power = 10;              //�Ŀ�(������)
    public int fixedPower;              //������ �Ŀ�
    public bool isGodMode;              //���� ���
    public bool isPlayerDie;
    public Vector3 respawnPosition = new Vector3(0f, 0f);
    public Color defaultColor = new Color(0.0001378677f, 0.0003025429f, 0.0007314645f);
    public Color rageColor = new Color(1.210474f, 2.656317f, 6.422235f);

    private static GameManager _instance;
    private GameObject player;
    private SpriteRenderer playerRenderer;
    private float playerMaterialIntensity;
    private float _playTimeSec;      //�÷��� Ÿ��
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
            //���� ����
            if (health <= 0 || player.GetComponent<Transform>().position.y < -64)
            {
                player.GetComponent<playerManager>().Die();
                PlayerDie();
            }

            //���¹̳� ���� ���
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

            //ü��, ���¹̳�, �г� ������
            healthBar.value = (float)health / (float)maxHealth;

            staminaBar.value = (float)stamina / (float)maxStamina;

            if (angerCharged == 0)
                angerBar.value = (float)angerLevel / 100f;
            else
                angerBar.value = (float)angerCharged / 100f;
        }

        //��üȭ�� ���
        if (Input.GetKeyDown(KeyCode.Escape))
            Screen.fullScreen = !Screen.fullScreen;

        //��� �޼���
        if(warnningTimer > 0)
        {
            warnningTimer -= Time.deltaTime;
        } else if (warnningTimer <= 0 && warnningTimer > -99f)
        {
            warnningText.text = "";
            warnningTimer = -100f;
        }

        //�÷��� Ÿ�̸�
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

    void PlayerDie()        // �÷��̾� ����
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

    public void PlayerRespawn()     //�÷��̾� ������
    {
        isPlayerDie = false;
        health = maxHealth;
        stamina = maxStamina;
    }

    public void ShowWarnning(string str, float time = 2f)       //��� �޼��� ���̱�
    {
        warnningTimer = time;
        warnningText.text = str;
    }

    public void UpdateIconDescription(string str)       //������ ����� ������Ʈ
    {
        iconDescription.text = str;
    }

    public bool consumeStamina(int val, bool isReduce = true)       //���¹̳� �Ҹ� ���
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

    public void increaseAngerLevel(int val)      // �г� ������ ����
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

    void UpdatePlayerMaterialColor()        // �÷��̾� ���׸��� HDR Color ������Ʈ
    {
        Color fixedColor = new Color(
            rageColor.r * playerMaterialIntensity
            , rageColor.g * playerMaterialIntensity
            , rageColor.b * playerMaterialIntensity);

        playerRenderer.material.SetColor("_Color", fixedColor);
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
        angerEffect.gameObject.SetActive(false);
        StartCoroutine(RunIintensityUpdate(false));
    }

    IEnumerator RunIintensityUpdate(bool isUpDown)      // �÷��̾� ���׸��� HDR Color ���� ���� �ڷ�ƾ
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
