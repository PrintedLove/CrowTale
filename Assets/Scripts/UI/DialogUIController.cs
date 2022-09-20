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

    private void Start()
    {
        animator_player = ilust_player.GetComponent<Animator>();
        animator_opponent = ilust_opponent.GetComponent<Animator>();
        //StartCoroutine(test());
        //StartDialog("Talk_summonerMeet");
    }

    private void OnEnable()
    {
        ingameUI.SetActive(false);
        settingMenu.SetActive(false);

        text_talk.text = "";
    }

    //IEnumerator test()
    //{
    //    while (true)
    //    {
    //        yield return new WaitForSeconds(2f);
    //        ChangeTalker();
    //    }
    //}

    public void StartDialog(string con)
    {
        conversation = con;
        talkCounter = 1;

        string[] nameStrings = GameManager.Instance.LoadTranslatedText(conversation, 0).Split('\n');
        nameTexts[0] = nameStrings[0];
        nameTexts[1] = nameStrings[1];

        UpdateDialogTexts();
    }

    private void UpdateDialogTexts()
    {
        string[] rawTexts = GameManager.Instance.LoadTranslatedText(conversation, talkCounter).Split('\n');
        ChangeTalker(rawTexts[0].Equals("p"));

        dialogTexts[0] = null;
        dialogTexts[1] = null;
        dialogTexts[2] = null;

        for (int i = 0; i < rawTexts.Length - 1 && i < 3; i++)
            dialogTexts[i] = rawTexts[i + 1];

        talkCounter++;
    }

    private void ChangeTalker(bool talker)  //true = player, false = opponent
    {
        isPlayerTalking = talker;
        animator_player.SetBool("talk", isPlayerTalking);
        animator_opponent.SetBool("talk", !isPlayerTalking);
        text_name.text = isPlayerTalking ? nameTexts[0] : nameTexts[1];
    }
}
