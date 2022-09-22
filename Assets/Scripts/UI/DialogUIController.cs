using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogUIController : MonoBehaviour
{
    public bool isPlayerTalking = true;    //true = player, false = opponent

    [SerializeField] private GameObject ilust_player, ilust_opponent;
    [SerializeField] private Text text_name, text_talk;
    [SerializeField] private GameObject clickIcon;
    [SerializeField] private GameObject ingameUI;
    [SerializeField] private GameObject settingMenu;

    private Animator animator_player, animator_opponent;
    private int talkCounter = 1;    //A number indicating the current conversation
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
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ilust_player.GetComponent<Animator>().enabled = false;
            ilust_opponent.GetComponent<Animator>().enabled = false;
            animator.SetBool("exit", true);
        }
        else if (Input.anyKeyDown)
        {
            if(!isTyping)
                UpdateDialogTexts();
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

        UpdateDialogTexts();
    }

    private void UpdateDialogTexts()
    {
        string[] rawTexts = GameManager.Instance.LoadTranslatedText(conversation, talkCounter).Split('\n');

        if(rawTexts[0].Equals("e"))
        {
            ilust_player.GetComponent<Animator>().enabled = false;
            ilust_opponent.GetComponent<Animator>().enabled = false;
            animator.SetBool("exit", true);
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
        isPlayerTalking = talker;
        animator_player.SetBool("talk", isPlayerTalking);
        animator_opponent.SetBool("talk", !isPlayerTalking);
        text_name.text = isPlayerTalking ? nameTexts[0] : nameTexts[1];
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
                wattingTime = 0.75f;
                rowfinish = false;
            }
            else
                wattingTime = 0.04f;
        }

        isTyping = false;
        clickIcon.SetActive(!isTyping);
    }

    public void Disable()
    {
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        ingameUI.SetActive(false);
        GameObject.FindWithTag("Player").GetComponent<PlayerManager>().PlayerStop();
        Resetting();
        ShowDialog();
    }

    private void OnDisable()
    {
        ingameUI.SetActive(true);
        GameObject.FindWithTag("Player").GetComponent<PlayerManager>().PlayerPlay();
    }

    public void Resetting()
    {
        talkCounter = 1;
        text_talk.text = "";
        isTyping = false;
        animator.SetBool("exit", false);
    }
}
