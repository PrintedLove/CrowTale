using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SummonerString : MonoBehaviour
{
    public float  stringLength = 16, lifeTime = 3f, warnningBoxWidth = 1f;
    [SerializeField] private GameObject attack, warnningBox, stringBigAnimation;

    private SpriteRenderer attack_spr, warnningBox_spr;

    private void Awake()
    {
        attack_spr = attack.GetComponent<SpriteRenderer>();
        warnningBox_spr = warnningBox.GetComponent<SpriteRenderer>();

        //Grow();
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    public void Grow()
    {
        StartCoroutine(RunGrow());
    }

    public void Down()
    {
        StartCoroutine(RunDown());
    }

    IEnumerator RunGrow()
    {
        //show warnning box
        warnningBox.SetActive(true);
        warnningBox_spr.size = new Vector2(stringLength, 0f);

        float _length = warnningBoxWidth;
        while(_length > 0f)
        {
            yield return new WaitForSeconds(0.01f);
            _length -= 0.05f;
            warnningBox_spr.size = new Vector2(stringLength, _length);
        }
        warnningBox.SetActive(false);

        yield return new WaitForSeconds(0.25f);

        //grow string
        attack.SetActive(true);
        attack_spr.size = new Vector2(0, attack_spr.size.y);
        _length = 0;
        while (_length < stringLength)
        {
            yield return new WaitForSeconds(0.01f);
            attack_spr.size = new Vector2(_length, attack_spr.size.y);
            _length += stringLength / 20f;
        }
        attack_spr.size = new Vector2(stringLength, attack_spr.size.y);

        if(stringBigAnimation != null)
        {
            attack_spr.GetComponent<SpriteRenderer>().enabled = false;
            stringBigAnimation.SetActive(true);
            stringBigAnimation.GetComponent<Animator>().SetBool("isRun", true);
        }

        yield return new WaitForSeconds(lifeTime);

        StartCoroutine(RunDown());
    }

    IEnumerator RunDown()
    {
        if (stringBigAnimation != null)
        {
            stringBigAnimation.GetComponent<Animator>().SetBool("isRun", false);
            stringBigAnimation.SetActive(false);
            attack_spr.GetComponent<SpriteRenderer>().enabled = true;
        }

        float _length = stringLength;
        while (_length > 0)
        {
            yield return new WaitForSeconds(0.01f);
            attack_spr.size = new Vector2(_length, attack_spr.size.y);
            _length -= stringLength / 20f;
        }
        attack_spr.size = new Vector2(0, attack_spr.size.y);
        attack.SetActive(false);
        gameObject.SetActive(false);
    }
}
