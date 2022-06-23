using System.Linq;
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
    [SerializeField] GameObject BGMDescription;
    [SerializeField] GameObject titleMenu;
    [SerializeField] GameObject storyUI;
    [SerializeField] GameObject blackFadeBox;

    [Header("Player")]
    public bool isPause = false;        //whether to pause
    public int maxHealth = 100;         //maximum HP
    public int health = 100;            //current HP
    public int maxStamina = 100;        //maximum stamina
    public int stamina = 100;           //current stamina
    public int angerLevel = 0;          //anger energy
    public int angerCharged = 0;        //anger charged
    public int power = 10;              //Power (Attack Damage)
    [HideInInspector] public int fixedPower;        //modified power
    public bool damageImmune;           //invincibility
    public bool isPlayerDie;            //Whether the player dies
    public Vector3 respawnPosition = new Vector3(0f, 0f, 0f);       //respawn coordinates


    [HideInInspector] public Color defaultColor
     = new Color(0.0001378677f, 0.0003025429f, 0.0007314645f);      // primary color
    [HideInInspector] public Color rageColor
     = new Color(1.210474f, 2.656317f, 6.422235f);      // color to change in anger

    private static GameManager _instance;
    private GameObject player;
    private SpriteRenderer playerRenderer;
    private float playerMaterialIntensity;
    private int deathCount;          //number of player deaths
    private float _playTimeSec;      //play time
    private int _playTimeMin;
    private float staminaRegenCool = 0;
    private float staminaRegenTimer = 0;
    private float warnningTimer;

    [Header("Others")]
    [SerializeField] bool fastStart;
    [SerializeField] Camera mainCamera;
    [SerializeField] Font munro;
    [SerializeField] Font neodgm;
    [HideInInspector] public List<Dictionary<string, object>> languageData;   //language data list
    [HideInInspector] public Font customFont;   //Font set in language data
    [HideInInspector] public bool isGameStart = false;   //Whether the game starts

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

        titleMenu.SetActive(true);
        storyUI.SetActive(true);
        blackFadeBox.SetActive(true);
    }

    private void Update()
    {
        if (fastStart)
        {
            titleMenu.GetComponent<titleMenuController>().SetTitleEnd();
            fastStart = false;
        }

        if (!isPlayerDie)
        {
            //watch for death
            if (health <= 0)
            {
                player.GetComponent<playerManager>().Die();
                PlayerDie();
            }

            //Stamina Regen Calculation
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

            //Health, Stamina, Anger Gauges
            healthBar.value = (float)health / (float)maxHealth;
            staminaBar.value = (float)stamina / (float)maxStamina;
            angerBar.value = angerCharged == 0 ? (float)angerLevel / 100f : (float)angerCharged / 100f;
        }

        //Toggle full screen
        if (Input.GetKeyDown(KeyCode.Escape)) Screen.fullScreen = !Screen.fullScreen;

        //warning message
        if (warnningTimer > 0)
            warnningTimer -= Time.deltaTime;
        else if (warnningTimer <= 0 && warnningTimer > -99f)
        {
            warnningText.text = "";
            warnningTimer = -100f;
        }

        if(isGameStart)
        {
            //play timer
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
    }

    // Player Damage
    public void GetDamage(int damage, float immuneTime) {
        if (!damageImmune) {
            health -= damage;
            damageImmune = true;
            StartCoroutine(RunDamageImmuneTime(immuneTime));
        }
    }

    // player Death
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

    //Player Respawn
    public void PlayerRespawn()
    {
        isPlayerDie = false;
        health = maxHealth;
        stamina = maxStamina;
    }

    //Show warning message
    public void ShowWarnning(string str, float time = 2f)
    {
        warnningTimer = time;
        warnningText.text = str;
    }

    //Icon description update
    public void UpdateIconDescription(string str)
    {
        iconDescription.text = str;
    }

    //Stamina consumption calculation
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

    //Increased anger gauge
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

    //Update Player Material HDR Color
    void UpdatePlayerMaterialColor()
    {
        Color fixedColor = new Color(
            rageColor.r * playerMaterialIntensity
            , rageColor.g * playerMaterialIntensity
            , rageColor.b * playerMaterialIntensity);

        playerRenderer.material.SetColor("_Color", fixedColor);
    }

    //Get language data
    public void LoadLanguageData(string fileName)
    {
        languageData = CSVReader.Read(Application.streamingAssetsPath + "/Languages/" + fileName + ".csv");

        if ((string)languageData[0]["Font"] == "neodgm")
            customFont = neodgm;
        else if ((string)languageData[0]["Font"] == "munro")
            customFont = munro;
        else
            customFont = Font.CreateDynamicFontFromOSFont((string)languageData[0]["Font"], (int)languageData[0]["Font_size"]);
    }

    //Change background music
    public void changeBGM(string description, AudioClip audioClip)
    {
        BGMDescription.SetActive(true);
        BGMDescription.GetComponent<Text>().text = "BGM - " + description;
        BGMDescription.GetComponent<Animator>().SetTrigger("show");

        StartCoroutine(RunBGMFade(audioClip));
    }

    //Invincible time coroutine
    IEnumerator RunDamageImmuneTime(float immuneTime)
    {
        yield return new WaitForSeconds(immuneTime);

        damageImmune = false;
    }

    //Anger Gauge Cooldown Coroutine
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

    //Player Material HDR Color Intensity Control Coroutine
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

    //BGM fade coroutine
    public IEnumerator RunBGMFade(AudioClip audioClip)
    {
        int channeling = 1;
        AudioSource adudioSource = mainCamera.GetComponent<AudioSource>();

        while (channeling < 750)
        {
            channeling += 1;

            if(channeling < 175)
                adudioSource.volume -= 0.01f;
            else if (channeling == 175)
            {
                adudioSource.clip = audioClip;
                adudioSource.Play();
            }
            else
                if (adudioSource.volume < 1) adudioSource.volume += 0.01f;

            yield return new WaitForSeconds(0.01f);
        }

        BGMDescription.GetComponent<Animator>().ResetTrigger("show");
        BGMDescription.SetActive(false);
    }
}

public class GameContorl
{
    public void IsPause()
    {
        GameManager.Instance.isPause = true;
    }
}
