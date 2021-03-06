using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cannonBullet : MonoBehaviour
{
    public int damage = 10;
    public GameObject destroyEffect;
    public GameObject particalSystem;

    Rigidbody2D rigidBody;

    private bool isCreateEffect = false;

    void Start()
    {
        rigidBody = GetComponent<Rigidbody2D>();

        Destroy(gameObject, 5.0f);
    }

    private void Update()
    {
        transform.Rotate(new Vector3(0, 0, transform.rotation.z + 2f));
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        bool isDestroy;
        isDestroy = false;

        if (collision.gameObject.tag == "Struct")
        {
            Instantiate(particalSystem, transform.position, transform.rotation);
            isDestroy = true;
        }
        else if (collision.gameObject.tag == "Player")
        {
            if (!GameManager.Instance.isPlayerDie)
            {
                if (!GameManager.Instance.damageImmune)
                {
                    collision.GetComponent<playerManager>().Knockback(1.25f, transform.position, 0.75f);
                    GameManager.Instance.GetDamage(damage, 0.1f);
                    isDestroy = true;
                    isCreateEffect = true;
                }
                else
                    GameManager.Instance.increaseAngerLevel(20);
            }
        }

        if (isDestroy)
        {
            if (isCreateEffect)
            {
                GameObject destroyEft = Instantiate(destroyEffect, transform.position, transform.rotation);
                destroyEft.transform.Rotate(new Vector3(0, 0, Random.Range(0, 360f)));
            }

            Destroy(gameObject);
        }
    }
}
