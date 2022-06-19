using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;

public class titleMenuController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] GameObject titleOverlaidUI;
    [SerializeField] GameObject ingameUI;
    [SerializeField] GameObject gameDescription;
    [SerializeField] GameObject BGMDescription;
    [SerializeField] GameObject blackFadeBox;
    [SerializeField] GameObject pressButtonText;
    [SerializeField] GameObject storySkipText;
    [SerializeField] GameObject languageChangeText;
    [SerializeField] GameObject storyUI;
    [SerializeField] Text[] storyTexts;
    [SerializeField] float[] pressTime;

    [Header("Others")]
    [SerializeField] AudioClip[] audioClip;
    [SerializeField] GameObject mainCamera;
    [SerializeField] GameObject player;

    private bool isPressAble = false;
    private bool storyAble = false;     
    private List<string> languageFileList = new List<string>();
    private int languageSelectedIndex = 0;
    private int maxLSIndex = -1;
    private float storyDownSpeed = 0f;
    private Font customFont;

    Animator animator;
    AudioSource audioSource;
    RectTransform storyUIRectTransform;

    void Start()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        storyUIRectTransform = storyUI.GetComponent<RectTransform>();

        GetLanguageFileList();
        GameManager.Instance.LoadLanguageData(languageFileList[languageSelectedIndex]);
        SetStoryText();
        StartCoroutine(RunCheckPressTime(pressTime[0]));
    }

    void Update()
    {
        if(isPressAble)
        {
            if (!storyAble)
            {
                if(Input.anyKeyDown)
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
                if(Input.GetKeyDown(KeyCode.L))
                {
                    languageSelectedIndex = languageSelectedIndex == maxLSIndex ? 0 : languageSelectedIndex + 1;
                    GameManager.Instance.LoadLanguageData(languageFileList[languageSelectedIndex]);
                    SetStoryText();
                    PlaySound("Click");

                    isPressAble = false;
                    StartCoroutine(RunCheckPressTime(1f));
                }
                else if (Input.GetKeyDown(KeyCode.Return))
                {
                    storyUI.GetComponent<Animator>().SetTrigger("End");
                    animator.SetTrigger("endStory");
                    StartCoroutine(RunCheckPressTime(5f));
                }
                else if(Input.anyKey)
                {
                    storyDownSpeed = 3.5f;
                }
                else
                {
                    if (storyDownSpeed == 3.5f) storyDownSpeed = 0f;
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

        if(languageFileList.Contains("kor"))
        {
            languageFileList.Remove("kor");
            languageFileList.Insert(0, "kor");
        }
    }

    private void SetStoryText()
    {
        for(int i = 0; i < 8; i++)
        {
            storyTexts[i].font = GameManager.Instance.customFont;
            storyTexts[i].fontSize = (int)GameManager.Instance.languageData[0]["Font_size"];
            string text = (string)GameManager.Instance.languageData[i]["Story"];
            storyTexts[i].text = text.Replace("$", "\n");;
        }
    }

    public void SetTitleEnd()
    {
        titleOverlaidUI.SetActive(false);
        ingameUI.SetActive(true);
        storyUI.SetActive(false);
        mainCamera.GetComponent<cameraManager>().enabled = true;
        player.GetComponent<playerManager>().isDead = false;
        gameObject.SetActive(false);
    }

    IEnumerator RunMoveStoryUI()
    {
        while(storyUIRectTransform.anchoredPosition.y < 5320f)
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
