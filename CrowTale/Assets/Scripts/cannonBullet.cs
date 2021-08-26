using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cannonBullet : MonoBehaviour
{
    public float speed = 20f;
    public int damage = 10;
    public new Rigidbody2D rigidbody;
    public GameObject destroyEffect;

    private bool isCreateEffect = false;

    void Start()
    {
        Destroy(gameObject, 5.0f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        bool isDestroy;
        isDestroy = false;

        if (collision.gameObject.tag == "Struct")
            isDestroy = true;
        else if (collision.gameObject.tag == "Player")
        {
            GameManager.Instance.health -= 25;
            isDestroy = true;
            isCreateEffect = true;
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
