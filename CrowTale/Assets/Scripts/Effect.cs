using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class Effect : MonoBehaviour
{
    Light2D lt;

    public bool isDestory;
    public bool isLtFade;

    void Awake()
    {
        isDestory = false;
        lt = gameObject.GetComponent<Light2D>();
    }

    void Update()
    {
        if (isDestory)
        {
            Destroy(gameObject);
        }

        if(isLtFade && lt.intensity > 0)
        {
            lt.intensity -= 0.1f;
        }
    }
}
