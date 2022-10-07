using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class PlayerAttack : MonoBehaviour
{
    public float speed = 20f;
    public int damage = 10;
    public GameObject damageText, destroyEffect;
    public Color fixedColor;
    public bool lighting;

    void Start()
    {
        GetComponent<Rigidbody2D>().velocity = transform.right * speed;
        Destroy(gameObject, 1.0f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        bool isDestroy;
        isDestroy = false;

        if (collision.gameObject.tag == "Struct")
            isDestroy = true;
        else if  (collision.gameObject.tag == "Enemy" || collision.gameObject.tag == "Damageable Object")
        {
            _Object obj = collision.GetComponent<_Object>();
            
            if (obj != null)
            {
                if (obj.isHit)
                {
                    int round = Mathf.FloorToInt(damage / 5f);
                    int fixedDamage = Random.Range(damage - round, damage + round + 1);
                    BoxCollider2D objBox = obj.GetComponent<BoxCollider2D>();

                    obj.TakeDamage(fixedDamage, Mathf.Sign(obj.transform.position.x - transform.position.x));

                    GameObject DT = Instantiate(damageText,
                        objBox.bounds.center + new Vector3(Random.Range(-0.25f, 0.25f),
                        objBox.bounds.extents.y + Random.Range(0.4f, 0.6f), 0f), obj.transform.rotation);
                    DT.GetComponent<TextMeshPro>().text = fixedDamage.ToString();
                }

                isDestroy = true;
            }
        }

        if (isDestroy)
        {
            GameObject DE = Instantiate(destroyEffect, transform.position, transform.rotation);
            SpriteRenderer DERenderer = DE.GetComponent<SpriteRenderer>();
            Effect DEScript = DE.GetComponent<Effect>();

            DERenderer.material.SetColor("_Color", fixedColor);
            DERenderer.GetComponent<Light2D>().enabled = lighting;
            DEScript.isLtFade = lighting;

            if (GameManager.Instance.angerCharged > 0)
                DEScript.ptColor = Color.white;
            else
                DEScript.ptColor = Color.black;

            Destroy(gameObject);
        }
    }
}
