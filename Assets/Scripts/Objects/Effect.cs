using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class Effect : MonoBehaviour
{
    Light2D lt;
    private ParticleSystem ps;
    public bool isDestory;
    public bool isLtFade;
    public bool isParticalSpon;
    public Color ptColor;

    void Awake()
    {
        isDestory = false;
        lt = gameObject.GetComponent<Light2D>();
        ps = gameObject.GetComponent<ParticleSystem>();
    }

    private void Start() {
        if (isParticalSpon && ps != null)
        {
            var pt_color = ps.colorOverLifetime;
            pt_color.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys( new GradientColorKey[] { new GradientColorKey(ptColor, 0.0f), new GradientColorKey(ptColor, 1.0f) }
            , new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) } );
            pt_color.color = grad;
        }
    }

    void Update()
    {
        if(isParticalSpon) {
            isParticalSpon = false;
            CreatePaartical();
        }

        if (isDestory)
        {
            Destroy(gameObject);
        }

        if(isLtFade && lt.intensity > 0)
        {
            lt.intensity -= 0.1f;
        }
    }

    void CreatePaartical()       //  Create particle effect
    {
        ps.Play();
    }
}
