using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MessageIconController : MonoBehaviour
{
    public bool show = true;
    public float alpha, alpha_min = 0f, alpha_max = 1f, alpha_rise = 0.05f;
    private SpriteRenderer spriteRenderer;

    private bool fadeDone;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        alpha = spriteRenderer.color.a;
    }

    public void Fade(bool s)
    {
        show = s;
        fadeDone = true;
        StartCoroutine(RunFade());
    }


    private IEnumerator RunFade()  //true: fade in, false: fade out
    {
        while(fadeDone)
        {
            Color c = spriteRenderer.color;

            if (show)
            {
                if (alpha < alpha_max)
                {
                    alpha += alpha_rise;
                    spriteRenderer.color = new Color(c.r, c.g, c.b, alpha);
                }
                else
                    fadeDone = false;
            }
            else
            {
                if (alpha > alpha_min)
                {
                    alpha -= alpha_rise;
                    spriteRenderer.color = new Color(c.r, c.g, c.b, alpha);
                }
                else
                    fadeDone = false;
            }
                
            yield return new WaitForSeconds(0.02f);
        }
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && gameObject.activeSelf)
            Fade(true);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && gameObject.activeSelf)
            Fade(false);
    }
}
