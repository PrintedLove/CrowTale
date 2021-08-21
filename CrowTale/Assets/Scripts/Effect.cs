using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Effect : MonoBehaviour
{
    public bool isDestory;

    void Awake()
    {
        isDestory = false;
    }

    void Update()
    {
        if (isDestory)
        {
            Destroy(gameObject);
        }
    }
}
