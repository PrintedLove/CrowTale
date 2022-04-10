using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class playerAttack : MonoBehaviour
{
    public float speed = 20f;
    public int damage = 10;
    public new Rigidbody2D rigidbody;
    public GameObject destroyEffect;
    public Color fixedColor;
    public bool lighting;

    void Start()
    {
        rigidbody.velocity = transform.right * speed;
        Destroy(gameObject, 2.0f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        bool isDestroy;
        isDestroy = false;

        if (collision.gameObject.tag == "Struct")
            isDestroy = true;
        else if  (collision.gameObject.tag == "Enemy")
        {
            Enemy enemy = collision.GetComponent<Enemy>();
            
            if (enemy != null)
            {
                if (enemy.isHit)
                    enemy.TakeDamage(damage, Mathf.Sign(enemy.transform.position.x - transform.position.x));
                    
                isDestroy = true;
            }
        }

        if (isDestroy)
        {
            GameObject destroyEft = Instantiate(destroyEffect, transform.position, transform.rotation);
            SpriteRenderer destroyEftRenderer = destroyEft.GetComponent<SpriteRenderer>();
            Effect destroyEftScript = destroyEft.GetComponent<Effect>();

            destroyEftRenderer.material.SetColor("_Color", fixedColor);
            destroyEftRenderer.GetComponent<Light2D>().enabled = lighting;
            destroyEftScript.isLtFade = lighting;

            if (GameManager.Instance.angerCharged > 0)
                destroyEftScript.ptColor = Color.white;
            else
                destroyEftScript.ptColor = Color.black;

            Destroy(gameObject);
        }
    }
}
