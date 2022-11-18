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
    [SerializeField] private GameObject dialogUI, messageIcon, brodcastScreen;

    protected Animator animator;
    protected SpriteRenderer spriteRenderer;

    private GameObject player;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        player = GameObject.FindWithTag("Player");
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
                action = Act.Talk;
            }
        }
        else if (action == Act.Talk)
        {
            //standing
            if (Input.GetKeyDown(KeyCode.Escape) || dialogUI.GetComponent<DialogUIController>().talkCounter == 5)
            {
                StartCoroutine(RunCloseBrodcast());
                action = Act.TalkEnd;
            }
        }
        //else if (action == Act.TalkEnd)
        //{

        //}

        IEnumerator RunCloseBrodcast()
        {
            float screen_size = 1f;

            while(screen_size > 0)
            {
                brodcastScreen.transform.localScale = new Vector3(1, screen_size, 1);
                screen_size -= 0.04f;

                yield return new WaitForSeconds(0.01f);
            }

            brodcastScreen.SetActive(false);

            spriteRenderer.flipX = player.transform.position.x < transform.position.x;
        }
    }
}
