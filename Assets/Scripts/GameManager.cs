using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get { return _instance; } }
    private static GameManager _instance;

    [Space]
    [Header("- - - - - Player - - - - -")]
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
    public bool isPlayerDie;            //Whether the player dies.
    public Vector3 respawnPosition = new Vector3(0f, 0f, 0f);       //respawn coordinates

    [HideInInspector]
    public Color defaultColor
     = new Color(0.0001378677f, 0.0003025429f, 0.0007314645f);      // primary color
    [HideInInspector]
    public Color rageColor
     = new Color(1.210474f, 2.656317f, 6.422235f);      // color to change in anger

    private GameObject player;
    private PlayerManager PM;
    private SpriteRenderer playerRenderer, playerAttackDirectionRenderer;
    private Camera mainCamera;
    private float playerMaterialIntensity;
    private int deathCount;          //number of player deaths
    private float _playTimeSec;      //play time
    private int _playTimeMin;
    private float staminaRegenCool = 0;
    private float staminaRegenTimer = 0;

    [Space]
    [Header("- - - - - Others - - - - -")]
    [SerializeField] bool fastStart;
    [SerializeField] Font munro;
    [SerializeField] Font neodgm;

    [HideInInspector] public List<Dictionary<string, object>> languageData;   //language data list
    [HideInInspector] public Font customFont;   //Font set in language data
    [HideInInspector] public bool isGameStart = false;   //Whether the game starts

    private List<string> languageFileList = new List<string>();     // List of fetchable language data names
    private int languageSelectedIndex = 0;  // Current language data index
    private int maxLSIndex = -1;    // Max Language Data Index
    private float warnningTimer;
    private string[] UITexts = new string[4];
    //0: warnning die, 1: warnning stamina, 2: death count, 3: play time

    [Space]
    [Header("- - - - - UI - - - - -")]
    [SerializeField] Text deatCounter;
    [SerializeField] Text playTime;
    [SerializeField] Slider healthBar;
    [SerializeField] Slider staminaBar;
    [SerializeField] Slider angerBar;
    [SerializeField] Text iconDescription;
    [SerializeField] Text powerText;
    [SerializeField] Image angerEffect;
    [SerializeField] Text warnningText;
    [SerializeField] GameObject titleMenu;
    [SerializeField] GameObject storyUI;
    [SerializeField] GameObject blackFadeBox;
    [SerializeField] GameObject[] manualTexts;
    [SerializeField] GameObject[] statBarIcons;
    [SerializeField] Text[] settingTexts;
    [SerializeField] GameObject settingMenu;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);

        _playTimeSec = 0;
        _playTimeMin = 0;
        playerMaterialIntensity = 0;
        fixedPower = power;
        deathCount = 0;
        damageImmune = false;
        isPlayerDie = false;

        player = GameObject.FindWithTag("Player");
        PM = player.GetComponent<PlayerManager>();
        playerRenderer = player.GetComponent<SpriteRenderer>();
        playerAttackDirectionRenderer = player.transform.Find("Attack Direction").GetComponent<SpriteRenderer>();
        mainCamera = Camera.main;

        titleMenu.SetActive(true);
        storyUI.SetActive(true);
        blackFadeBox.SetActive(true);

        Screen.fullScreen = true;
        Application.targetFrameRate = 60;

        GetLanguageFileList();
        LoadLanguageData(false);
        TranslateManualText();
        TranslateUI();

        //Quick start mode for testing
        if (fastStart)
        {
            titleMenu.GetComponent<TitleMenuController>().SetTitleEnd();
        } else
        {
            mainCamera.transform.position = new Vector3(-70f, 24f, -10f);
            player.transform.position = new Vector3(-70f, 4f, -0f);
        }
    }

    private void Update()
    {
        if (!isPlayerDie)
        {
            //watch for death
            if (health <= 0)
            {
                PM.Die();
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

        //Show setting menu
        if (Input.GetKeyDown(KeyCode.Escape) && isGameStart)
        {
            settingMenu.SetActive(!settingMenu.activeSelf);
            SoundManager.Instance.Play(SoundManager.AS.UI, SoundManager.UISound.click1);
        }
        
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
            playTime.text = UITexts[3] + "   " + _playTimeMin.ToString("00") + " : " + timeStr;
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
        deatCounter.text = UITexts[2] + "   " + deathCount.ToString();
        ShowWarnning(0, 3f);

        health = 0;     
        stamina = 0;           
        angerLevel = 0;          
        angerCharged = 0;        
        fixedPower = power;
        playerMaterialIntensity = 0f;
        damageImmune = false;
        UpdatePlayerMaterialColor();

        SoundManager.Instance.Play(SoundManager.AS.UI, SoundManager.UISound.die);
    }

    //Player Respawn
    public void PlayerRespawn()
    {
        isPlayerDie = false;
        health = maxHealth;
        stamina = maxStamina;
    }

    //Show warning message
    public void ShowWarnning(int index, float time = 2f)
    {
        warnningTimer = time;
        warnningText.text = UITexts[index];
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
        playerAttackDirectionRenderer.material.SetColor("_Color", fixedColor);
    }

    //Get the names of the language data list in the streaming folder.
    private void GetLanguageFileList()
    {
        string filePath = Application.streamingAssetsPath + "/Languages";

        System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(filePath);

        foreach (System.IO.FileInfo File in di.GetFiles())
        {
            if (File.Extension.ToLower().CompareTo(".csv") == 0)
            {
                string FileNameOnly = File.Name.Substring(0, File.Name.Length - 4);
                languageFileList.Add(FileNameOnly);
                maxLSIndex += 1;
            }
        }

        if (languageFileList.Contains("kor"))
        {
            languageFileList.Remove("kor");
            languageFileList.Insert(0, "kor");
        }
    }

    //Get language data(true: increase index, false: keep)
    public void LoadLanguageData(bool isIncrease)
    {
        if(isIncrease)
            languageSelectedIndex = languageSelectedIndex == maxLSIndex ? 0 : languageSelectedIndex + 1;

        languageData = CSVReader.Read(Application.streamingAssetsPath
            + "/Languages/" + languageFileList[languageSelectedIndex] + ".csv");

        if ((string)languageData[0]["Font"] == "neodgm")
            customFont = neodgm;
        else if ((string)languageData[0]["Font"] == "munro")
            customFont = munro;
        else
            customFont = Font.CreateDynamicFontFromOSFont((string)languageData[0]["Font"], (int)languageData[0]["Meta"]);
    }

    public string LoadTranslatedText(string key, int index)
    {
        string text = (string)languageData[index][key];
        text = text.Replace('$', '\n');
        text = text.Replace('#', '"');

        return text;
    }

    public void TranslateUI()
    {
        iconDescription.font = customFont;
        warnningText.font = customFont;
        deatCounter.font = customFont;
        playTime.font = customFont;

        for (int i = 0; i < 4; i++)
            statBarIcons[i].GetComponent<IconController>().descripton = LoadTranslatedText("Others", 2 + i);

        UITexts[0] = LoadTranslatedText("Others", 6);
        UITexts[1] = LoadTranslatedText("Others", 7);
        UITexts[2] = LoadTranslatedText("Others", 8);
        UITexts[3] = LoadTranslatedText("Others", 9);

        for (int i = 0; i < 5; i++)
        {
            settingTexts[i].font = customFont;
            settingTexts[i].text = LoadTranslatedText("Others", 10 + i);
        }

        deatCounter.text = UITexts[2] + "   " + deathCount.ToString();
    }

    public void TranslateManualText()
    {
        for(int i = 0; i < manualTexts.Length; i++)
        {
            Text mt = manualTexts[i].GetComponent<Text>();

            mt.font = customFont;
            mt.fontSize = (int)languageData[1]["Meta"];
            mt.text = LoadTranslatedText("Manual", i);
        }
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
}

public class GameContorl
{
    public void IsPause()
    {
        GameManager.Instance.isPause = true;
    }
}
