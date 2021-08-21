using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerAttack : MonoBehaviour
{
    public float speed = 20f;
    public int damage = 10;
    public new Rigidbody2D rigidbody;
    public GameObject destroyEffect;

    void Start()
    {
        rigidbody.velocity = transform.right * speed;
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

                enemy.TakeDamage(damage, Mathf.Sign(enemy.transform.position.x - transform.position.x));
                isDestroy = true;
            }
        }

        if (isDestroy)
        {
            Instantiate(destroyEffect, transform.position, transform.rotation);
            Destroy(gameObject);
        }
    }
}
