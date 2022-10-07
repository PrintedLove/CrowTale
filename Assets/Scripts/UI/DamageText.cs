using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DamageText : MonoBehaviour
{
    public float speed_move, speed_alpha;
    TextMeshPro text;
    Color c;

    void Start()
    {
        text = GetComponent<TextMeshPro>();
        c = text.color;
        Destroy(gameObject, 2f);
    }

    void Update()
    {
        transform.Translate(new Vector3(0, speed_move * Time.deltaTime, 0));
        c.a = Mathf.Lerp(c.a, 0, Time.deltaTime * speed_alpha);
        text.color = c;
    }

}
