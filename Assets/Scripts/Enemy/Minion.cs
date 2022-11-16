using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Summoner;

public class Minion : _Object
{
    public enum Act
    {
        Watching, Talk, TalkEnd
    }

    public Act action;
    [SerializeField] private GameObject dialogUI, messageIcon;

    protected Animator animator;
    protected SpriteRenderer spriteRenderer;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (action == Act.Watching)
        {
            //talk button
            if (Input.GetKeyDown(KeyCode.T) && messageIcon.GetComponent<MessageIconController>().show)
            {
                GameManager.Instance.ShowDialogUI("Talk_viperFirstMeet");
                messageIcon.SetActive(false);
                //animator.ResetTrigger("CombatReset");
                action = Act.Talk;
            }
        }
        else if (action == Act.Talk)
        {
            //standing
            if (Input.GetKeyDown(KeyCode.Escape) || dialogUI.GetComponent<DialogUIController>().talkCounter == 6)
            {
                //animator.SetTrigger("wakeUp");
                action = Act.TalkEnd;
            }
        }
        else if (action == Act.TalkEnd)
        {

        }
    }
}