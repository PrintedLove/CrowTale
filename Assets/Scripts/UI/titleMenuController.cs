using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;

public class TitleMenuController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject titleOverlaidUI;
    [SerializeField] private GameObject ingameUI;
    [SerializeField] private GameObject gameDescription;
    [SerializeField] private GameObject BGMDescription;
    [SerializeField] private GameObject blackFadeBox;
    [SerializeField] private GameObject pressButtonText;
    [SerializeField] private GameObject storySkipText;
    [SerializeField] private GameObject languageChangeText;
    [SerializeField] private GameObject storyUI;
    [SerializeField] private Text[] storyTexts;
    [SerializeField] private float[] pressTime;     // input latency

    [Header("Others")]
    [SerializeField] private AudioClip[] audioClip;
    [SerializeField] private GameObject mainCamera;
    [SerializeField] private GameObject player;

    private bool isPressAble = false;   // Interact availability
    private bool storyAble = false;     // story UI availability
    private float storyDownSpeed = 0f;  // story scrolls down speed.

    Animator animator;
    AudioSource audioSource;
    RectTransform storyUIRectTransform;

    void Start()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        storyUIRectTransform = storyUI.GetComponent<RectTransform>();

        string description
            = "App Ver - " + Application.version
            + "$$printed.tistory.com$github.com/PrintedLove$$¨Ï 2022. Printed Love All rights reserved.";

        description = description.Replace("$", "\n");

        titleOverlaidUI.SetActive(true);
        titleOverlaidUI.transform.Find("Game Description").GetComponent<Text>().text = description;

        SetTitleText();
        StartCoroutine(RunCheckPressTime(pressTime[0]));
    }

    void Update()
    {
        if(isPressAble)
        {
            if (!storyAble)
            {
                // When any key is pressed on the title screen
                if (Input.anyKeyDown)
                {
                    gameDescription.SetActive(false);
                    BGMDescription.SetActive(false);
                    blackFadeBox.SetActive(false);
                    pressButtonText.SetActive(false);
                    animator.SetTrigger("isPressed");
                    PlaySound("Click");

                    isPressAble = false;
                    storyAble = true;
                    StartCoroutine(RunMoveStoryUI());
                    StartCoroutine(RunCheckPressTime(pressTime[1]));
                }
            } else
            {
                // Change language key
                if (Input.GetKeyDown(KeyCode.L))
                {
                    GameManager.Instance.LoadLanguageData(true);
                    SetTitleText();
                    GameManager.Instance.TranslateManualText();
                    GameManager.Instance.TranslateUI();

                    PlaySound("Click");

                    isPressAble = false;
                    StartCoroutine(RunCheckPressTime(1f));
                }
                // Enter key
                else if (Input.GetKeyDown(KeyCode.Return))
                {
                    storyUI.GetComponent<Animator>().SetTrigger("End");
                    animator.SetTrigger("endStory");
                    StartCoroutine(RunCheckPressTime(5f));
                }
                // Any key
                else if(Input.anyKey)
                {
                    storyDownSpeed = 4f;
                }
                else
                {
                    if (storyDownSpeed == 4f) storyDownSpeed = 0f;
                }
            }
        }
    }

    //Sound
    private void PlaySound(string act)
    {
        switch(act)
        {
            case "Click":
                audioSource.clip = audioClip[0];
                break;

            default:
                break;
        }

        audioSource.Play();
    }

    private void SetTitleText()
    {
        for(int i = 0; i < storyTexts.Length; i++)
        {
            storyTexts[i].font = GameManager.Instance.customFont;
            storyTexts[i].fontSize = (int)GameManager.Instance.languageData[0]["Meta"];
            storyTexts[i].text = GameManager.Instance.LoadTranslatedText("Story", i);
        }

        languageChangeText.GetComponent<Text>().text = GameManager.Instance.LoadTranslatedText("Others", 0);
        storySkipText.GetComponent<Text>().text = GameManager.Instance.LoadTranslatedText("Others", 1);
    }

    public void SetTitleEnd()
    {
        titleOverlaidUI.SetActive(false);
        ingameUI.SetActive(true);
        storyUI.SetActive(false);
        mainCamera.GetComponent<cameraManager>().enabled = true;
        player.GetComponent<PlayerManager>().PlayerPlay();
        GameManager.Instance.isGameStart = true;
        SoundManager.Instance.ChangeBGM(SoundManager.BGM.Bittersweet, 0.6f);

        gameObject.SetActive(false);
    }

    IEnumerator RunMoveStoryUI()
    {
        while(storyUIRectTransform.anchoredPosition.y < 5110f)
        {
            yield return new WaitForSeconds(0.02f);
            storyUIRectTransform.anchoredPosition
                = new Vector2(storyUIRectTransform.anchoredPosition.x
                , storyUIRectTransform.anchoredPosition.y + 1f + storyDownSpeed);
        }

        animator.SetTrigger("endStory");
        storyUI.SetActive(false);
    }

    IEnumerator RunCheckPressTime(float pressTime)
    {
        yield return new WaitForSeconds(pressTime);
        
        isPressAble = true;

        if(storyAble)
        {
            storySkipText.SetActive(true);
            languageChangeText.SetActive(true);
        }
    }
}
