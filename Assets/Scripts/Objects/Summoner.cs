using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Summoner : _Object
{
    [SerializeField] private GameObject dialogUI;
    public enum act
    {
        Sleep, Talk, Stand,
        battle_start,
        battle_phase1_, battle_phase2, battle_phase3
    }
    private GameObject player, summonStone, messageIcon;

    protected Animator animator;

    public act action;

    protected override void Awake()
    {
        if (GetComponent<Animator>() != null)
            animator = GetComponent<Animator>();

        spriteRenderer = GetComponent<SpriteRenderer>();

        summonStone = transform.Find("Summon Stone").gameObject;
        messageIcon = transform.Find("Message Icon").gameObject;
        player = GameObject.FindWithTag("Player").gameObject;

        objType = _ObjectType.Summoner;
        isHit = false;
        action = act.Sleep;
    }

    protected override void Update()
    {
        if (action == act.Sleep)
        {
            //talk button
            if (Input.GetKeyDown(KeyCode.T) && messageIcon.GetComponent<MessageIconController>().show)
            {
                GameManager.Instance.ShowDialogUI("Talk_summonerMeet");
                messageIcon.SetActive(false);
                action = act.Talk;
            }
        }
        else if (action == act.Talk)
        {
            //standing
            if (Input.GetKeyDown(KeyCode.Escape) || dialogUI.GetComponent<DialogUIController>().talkCounter == 7)
            {
                animator.SetTrigger("wakeUp");
                action = act.Stand;
            }
        }
        else if (action == act.Stand)
        {
            if(!dialogUI.activeSelf)
            {
                action = act.battle_start;
                SoundManager.Instance.ChangeBGM(SoundManager.BGM.TheWitch, 0.8f);
            }
        }
    }
}
