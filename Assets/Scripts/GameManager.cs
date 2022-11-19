using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using Com.LuisPedroFonseca.ProCamera2D;

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
    public Camera mainCamera;
    [HideInInspector] public bool isUpdateState;
    private float playerMaterialIntensity;
    private int deathCount;          //number of player deaths
    private float _playTimeSec;      //play time
    private int _playTimeMin;
    private float staminaRegenCool = 0;
    private float staminaRegenTimer = 0;

    [Space]
    [Header("- - - - - Others - - - - -")]
    [SerializeField] private bool fastStart;
    [SerializeField] private Font munro;
    [SerializeField] private Font neodgm;

    [HideInInspector] public List<Dictionary<string, object>> languageData;   //language data list
    [HideInInspector] public Font customFont;   //Font set in language data
    [HideInInspector] public bool isGameStart = false;   //Whether the game starts

    [HideInInspector] public bool showFPS;
    private float _deltaT;
    private GUIStyle gUIStyle;
    private Rect rect_FPS;
    private List<string> languageFileList = new List<string>();     // List of fetchable language data names
    private int languageSelectedIndex = 0;  // Current language data index
    private int maxLSIndex = -1;    // Max Language Data Index
    private float warnningTimer;
    private string[] UITexts = new string[4];
    //0: warnning die, 1: warnning stamina, 2: death count, 3: play time

    public GameObject currentSavepoint = null;
    public List<GameObject>  preSavePoints = new List<GameObject>();

    public UnityEvent m_PlayerDieEvent;

    [Space]
    [Header("- - - - - UI - - - - -")]
    [SerializeField] private UnityEngine.UI.Text deatCounter;
    [SerializeField] private UnityEngine.UI.Text playTime;
    [SerializeField] private UnityEngine.UI.Slider healthBar;
    [SerializeField] private UnityEngine.UI.Slider staminaBar;
    [SerializeField] private UnityEngine.UI.Slider angerBar;
    [SerializeField] private UnityEngine.UI.Text iconDescription;
    [SerializeField] private UnityEngine.UI.Text powerText;
    [SerializeField] private UnityEngine.UI.Image angerEffect;
    [SerializeField] private UnityEngine.UI.Text warnningText;
    [SerializeField] private GameObject titleMenu;
    [SerializeField] private GameObject storyUI;
    [SerializeField] private GameObject blackFadeBox;
    [SerializeField] private GameObject[] manualTexts;
    [SerializeField] private GameObject[] statBarIcons;
    [SerializeField] private UnityEngine.UI.Text[] settingTexts;
    [SerializeField] private GameObject ingameUI;
    [SerializeField] private GameObject settingMenu;
    [SerializeField] private GameObject dialogUI;
    public string dialogConversation = "";

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
        showFPS = false;
        playerMaterialIntensity = 0;
        fixedPower = power;
        isUpdateState = true;
        deathCount = 0;
        damageImmune = false;
        isPlayerDie = false;
        Application.targetFrameRate = 60;

        player = GameObject.FindWithTag("Player");
        PM = player.GetComponent<PlayerManager>();
        playerRenderer = player.GetComponent<SpriteRenderer>();
        playerAttackDirectionRenderer = player.transform.Find("Attack Direction").GetComponent<SpriteRenderer>();
        mainCamera = Camera.main;
        mainCamera.transform.Find("Background Sky").gameObject.SetActive(true);
        mainCamera.GetComponent<ProCamera2D>().FollowHorizontal = false;
        mainCamera.GetComponent<ProCamera2D>().FollowVertical = false;

        titleMenu.SetActive(true);
        storyUI.SetActive(true);
        blackFadeBox.SetActive(true);

        rect_FPS = new Rect(0, 0, Screen.width, Screen.height * 0.02f);
        gUIStyle = new GUIStyle
        {
            font = munro,
            alignment = TextAnchor.UpperCenter,
            fontSize = Screen.height / 40
        };
        gUIStyle.normal.textColor = new Color(0.9647059f, 0.8392158f, 0.7411765f, 1.0f);

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
            mainCamera.transform.position = new Vector3(-110f, 24f, -10f);
            player.transform.position = new Vector3(-110f, 4f, -0f);
            respawnPosition = new Vector3(-110f, 4f, -0f);
            ingameUI.SetActive(false);
        }

        Resources.UnloadUnusedAssets();
    }

    private void Update()
    {
        if (!isPlayerDie)
        {
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
                        isUpdateState = true;

                        if (stamina > maxStamina) stamina = maxStamina;
                    }
                }
            }
            else
                staminaRegenCool -= Time.deltaTime;
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

            //cal fps
            if(showFPS)
            {
                _deltaT += (Time.unscaledDeltaTime - _deltaT) * 0.1f;
            }

            //warning message
            if (warnningTimer > 0)
                warnningTimer -= Time.deltaTime;
            else if (warnningTimer <= 0 && warnningTimer > -99f)
            {
                warnningText.text = "";
                warnningTimer = -100f;
            }

            if (!dialogUI.activeSelf)
            {
                //Show setting menu
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    settingMenu.SetActive(!settingMenu.activeSelf);
                    SoundManager.Instance.Play(SoundManager.AS.UI, SoundManager.UISound.click1);
                    showFPS = settingMenu.activeSelf;
                }
                //reset stage
                else if (Input.GetKeyDown(KeyCode.N))
                {
                    if(!settingMenu.activeSelf) KillPlayer(true);
                }
            }
        }
    }

    private void LateUpdate()
    {
        if(isUpdateState)
        {
            healthBar.value = (float)health / (float)maxHealth;
            staminaBar.value = (float)stamina / (float)maxStamina;
            angerBar.value = angerCharged == 0 ? (float)angerLevel / 100f : (float)angerCharged / 100f;

            isUpdateState = false;
        }
    }

    private void OnGUI()
    {
        if(showFPS)
        {
            GUI.Label(rect_FPS, string.Format("{0:0.0} ms ({1:0.} fps)", _deltaT * 1000.0f, 1.0f / _deltaT), gUIStyle);
        }
    }

    //Player Damage
    public void GetDamage(int damage, float immuneTime) {
        if (!damageImmune) {
            health -= damage;

            //watch for death
            if (health <= 0)
            {
                KillPlayer();
                return;
            }

            damageImmune = true;
            isUpdateState = true;
            StartCoroutine(RunDamageImmuneTime(immuneTime));
        }
    }

    public void KillPlayer(bool fastRespawn = false)
    {
        healthBar.value = 0f;
        PM.Die(fastRespawn);
        PlayerDie(fastRespawn);

        if (m_PlayerDieEvent != null)
            m_PlayerDieEvent.Invoke();
    }

    //Player Death
    void PlayerDie(bool fastRespawn)
    {
        isPlayerDie = true;
        deathCount += 1;
        deatCounter.text = UITexts[2] + "   " + deathCount.ToString();
        if(!fastRespawn)
            ShowWarnning(0, 3f);

        health = 0;     
        stamina = 0;           
        angerLevel = 0;          
        angerCharged = 0;        
        fixedPower = power;
        isUpdateState = true;
        playerMaterialIntensity = 0f;
        damageImmune = false;
        UpdatePlayerMaterialColor();

        if(currentSavepoint != null)
            currentSavepoint.GetComponent<SavePoint>().ResetObject();

        SoundManager.Instance.Play(SoundManager.AS.UI, SoundManager.UISound.die);

        if(fastRespawn)
            PM.Respawn();

        GC.Collect();
        Resources.UnloadUnusedAssets();
    }

    //Player Respawn
    public void PlayerRespawn()
    {
        isPlayerDie = false;
        health = maxHealth;
        stamina = maxStamina;
        isUpdateState = true;
    }

    //Show Dialog UI
    public void ShowDialogUI(string con, int opponentSpriteIndex)
    {
        if (!dialogUI.activeSelf)
        {
            dialogConversation = con;
            dialogUI.SetActive(true);
            dialogUI.GetComponent<DialogUIController>().SetOpponentSprite(opponentSpriteIndex);
            SoundManager.Instance.Play(SoundManager.AS.UI, SoundManager.UISound.click2);
        }
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
                isUpdateState = true;
            }
            
            return true;
        } else
            return false;
    }

    //Increased anger gauge
    public void increaseAngerLevel(int val)
    {
        if (angerCharged <= 0)
        {
            angerLevel += val;
            isUpdateState = true;

            if (angerLevel >= 100)
            {
                SoundManager.Instance.Play(SoundManager.AS.UI, SoundManager.UISound.anger1);
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

    public void increasePower(int val)
    {
        power += val;
        isUpdateState = true;

        if (angerCharged > 0)
            fixedPower = power * 2;
        else
            fixedPower = power;

        powerText.text = fixedPower.ToString();
    }

    public void DisablePreSavePointObject(GameObject obj)
    {
        int i = 0;

        while (i < preSavePoints.Count)
        {
            if(preSavePoints[i] != obj)
            {
                if (preSavePoints[i] != null)
                    preSavePoints[i].GetComponent<SavePoint>().ActiveObject(false);

                preSavePoints.RemoveAt(i);
            }
            else
                i++;
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
            statBarIcons[i].GetComponent<StatIconController>().descripton = LoadTranslatedText("Others", 2 + i);

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
        playerRenderer.color = new Color(1f, 1f, 1f, 0.35f);
        yield return new WaitForSeconds(immuneTime);

        playerRenderer.color = new Color(1f, 1f, 1f, 1f);
        damageImmune = false;
    }

    //Anger Gauge Cooldown Coroutine
    IEnumerator RunAngerCountdown()
    {
        while(angerCharged > 0)
        {
            yield return new WaitForSeconds(0.12f);
            angerCharged --;
            isUpdateState = true;
        }

        fixedPower = power;
        isUpdateState = true;
        powerText.text = fixedPower.ToString();
        angerEffect.gameObject.SetActive(false);
        StartCoroutine(RunIintensityUpdate(false));
        SoundManager.Instance.Play(SoundManager.AS.UI, SoundManager.UISound.anger2);
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
