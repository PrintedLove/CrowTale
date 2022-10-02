using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Summoner : _Object
{
    [SerializeField] private GameObject dialogUI;
    public enum act
    {
        Sleeping, talking, Standing
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
        action = act.Sleeping;
    }

    protected override void Update()
    {
        if (action == act.Sleeping)
        {
            //talk button
            if (Input.GetKeyDown(KeyCode.T) && messageIcon.GetComponent<MessageIconController>().show)
            {
                GameManager.Instance.ShowDialogUI("Talk_summonerMeet");
                messageIcon.SetActive(false);
                action = act.talking;
            }
        }
        else if (action == act.talking)
        {
            //standing
            if (Input.GetKeyDown(KeyCode.Escape) || dialogUI.GetComponent<DialogUIController>().talkCounter == 7)
            {
                animator.SetTrigger("wakeUp");
                action = act.Standing;
                SoundManager.Instance.ChangeBGM(SoundManager.BGM.TheWitch, 0.8f);
            }
        }
        else if (action == act.Standing)
        {

        }
    }
}
