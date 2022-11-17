using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class DialogUIController : MonoBehaviour
{
    [SerializeField] private GameObject ilust_player, ilust_opponent;
    [SerializeField] private Text text_name, text_talk;
    [SerializeField] private GameObject clickIcon;
    [SerializeField] private GameObject ingameUI;
    [SerializeField] private GameObject settingMenu;
    public int talkCounter = 1;    //A number indicating the current conversation

    private Animator animator_player, animator_opponent;
    private GameObject player;
    private string conversation;
    private string[] nameTexts = new string[] { null, null };
    private string[] dialogTexts = new string[] { null, null, null };
    private bool isTyping = true;

    Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        animator_player = ilust_player.GetComponent<Animator>();
        animator_opponent = ilust_opponent.GetComponent<Animator>();
        player = GameObject.FindWithTag("Player");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) ExitDialog();
        else if (Input.anyKeyDown)
        {
            if(!isTyping)
            {
                UpdateDialogTexts();
                SoundManager.Instance.Play(SoundManager.AS.UI, SoundManager.UISound.click1);
            }
        }
    }

    //resetting dialog UI
    public void ShowDialog()
    {
        conversation = GameManager.Instance.dialogConversation;

        string[] nameStrings = GameManager.Instance.LoadTranslatedText(conversation, 0).Split('\n');
        nameTexts[0] = nameStrings[0];
        nameTexts[1] = nameStrings[1];

        text_name.font = GameManager.Instance.customFont;
        text_talk.font = GameManager.Instance.customFont;

        ilust_player.GetComponent<Animator>().enabled = true;
        ilust_opponent.GetComponent<Animator>().enabled = true;

        UpdateDialogTexts();
    }

    private void UpdateDialogTexts()
    {
        string[] rawTexts = GameManager.Instance.LoadTranslatedText(conversation, talkCounter).Split('\n');

        if(rawTexts[0].Equals("e"))
        {
            ExitDialog();
            return;
        }

        ChangeTalker(rawTexts[0].Equals("p"));

        dialogTexts[0] = null;
        dialogTexts[1] = null;
        dialogTexts[2] = null;

        for (int i = 0; i < rawTexts.Length - 1 && i < 3; i++)
            dialogTexts[i] = rawTexts[i + 1];

        talkCounter++;
        text_talk.text = "";
        StartCoroutine(RunTyping());
    }

    private void ChangeTalker(bool talker)  //true = player, false = opponent
    {
        animator_player.SetBool("talk", talker);
        animator_opponent.SetBool("talk", !talker);
        text_name.text = talker ? nameTexts[0] : nameTexts[1];
    }

    IEnumerator RunTyping()
    {
        isTyping = true;
        clickIcon.SetActive(!isTyping);
        
        int maxDialogTextIndex = 0, dialogTextIndex = 0,
            inputTextIndex = 0;
        float wattingTime = 0;
        bool rowfinish = false;

        foreach (string dialogText in dialogTexts)
            if (dialogText != null)
                maxDialogTextIndex++;

        while (dialogTextIndex < maxDialogTextIndex)
        {
            yield return new WaitForSeconds(wattingTime);

            if (inputTextIndex < dialogTexts[dialogTextIndex].Length)
            {
                if (dialogTextIndex != 0 && inputTextIndex == 0)
                    text_talk.text += '\n';

                text_talk.text += dialogTexts[dialogTextIndex][inputTextIndex];
                inputTextIndex++;
            }
            else
            {
                dialogTextIndex++;
                inputTextIndex = 0;
                rowfinish = true;
            }

            if (rowfinish)
            {
                wattingTime = 0.5f;
                rowfinish = false;
            }
            else
                wattingTime = 0.03f;
        }

        isTyping = false;
        clickIcon.SetActive(!isTyping);
    }

    public void ResetDialog()
    {
        talkCounter = 1;
        text_talk.text = "";
        isTyping = false;
        animator.SetBool("exit", false);
    }

    private void ExitDialog()
    {
        ilust_player.GetComponent<Animator>().enabled = false;
        ilust_opponent.GetComponent<Animator>().enabled = false;
        animator.SetBool("exit", true);
    }

    private void OnEnable()
    {
        ingameUI.SetActive(false);
        player.GetComponent<PlayerManager>().PlayerStop();
        ResetDialog();
        ShowDialog();
    }

    private void OnDisable()
    {
        ingameUI.SetActive(true);
        player.GetComponent<PlayerManager>().PlayerPlay();
    }

    public void Disable()
    {
        gameObject.SetActive(false);
    }
}
