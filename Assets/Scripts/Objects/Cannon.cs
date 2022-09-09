using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cannon : MonoBehaviour
{
    public Transform firePoint;
    public GameObject shotEffectObject;
    public GameObject cannonBulletObject;
    public bool isAttack = true;
    public float attackCooltime = 2.5f;
    public float bulletSpeed = 7.5f;

    AudioSource AS;

    void Start()
    {
        AS = GetComponent<AudioSource>();
        StartCoroutine(RunCannonAttack());
    }

    IEnumerator RunCannonAttack()     //attack cycle
    {
        while (isAttack)
        {
            GameObject cannonBullet = Instantiate(cannonBulletObject, firePoint.position, firePoint.rotation);
            cannonBullet.GetComponent<Rigidbody2D>().velocity = -transform.right * bulletSpeed;

            Instantiate(shotEffectObject, firePoint.position, firePoint.rotation);
            AS.Play();

            yield return new WaitForSeconds(attackCooltime);
        }
    }
}
