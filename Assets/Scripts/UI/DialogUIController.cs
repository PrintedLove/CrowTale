using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogUIController : MonoBehaviour
{
    public bool isPlayerTalking = true;    //true = player, false = opponent
    [SerializeField] private Animator ilust_player, ilust_opponent;

    [SerializeField] private GameObject ingameUI;

    private void Start()
    {
        //ingameUI.SetActive(false);
        //StartCoroutine(test());
    }

    //IEnumerator test()
    //{
    //    while (true)
    //    {
    //        yield return new WaitForSeconds(2f);
    //        ChangeTalker();
    //    }
    //}

    private void ChangeTalker()
    {
        isPlayerTalking = !isPlayerTalking;
        ilust_player.SetBool("talk", isPlayerTalking);
        ilust_opponent.SetBool("talk", !isPlayerTalking);
    }
}
