using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class Cannon : MonoBehaviour
{
    public Transform firePoint;
    public GameObject shotEffectObject;
    public GameObject cannonBulletObject;
    public bool isAttack = true;
    public float attackCooltime = 2.5f;
    public float bulletSpeed = 7.5f;

    private bool isCoroutineRun;

    AudioSource AS;

    private void Awake()
    {
        AS = GetComponent<AudioSource>();
        isCoroutineRun = false;
    }

    private void Start()
    {
        if (!isCoroutineRun)
            StartCoroutine(RunCannonAttack());
    }

    private void OnEnable()
    {
        if (!isCoroutineRun)
            StartCoroutine(RunCannonAttack());
    }

    IEnumerator RunCannonAttack()     //attack cycle
    {
        isCoroutineRun = true;

        while (isAttack)
        {
            GameObject cannonBullet = Instantiate(cannonBulletObject, firePoint.position, firePoint.rotation);
            cannonBullet.GetComponent<Rigidbody2D>().velocity = -transform.right * bulletSpeed;

            Instantiate(shotEffectObject, firePoint.position, firePoint.rotation);

            if(AS.enabled)
                AS.Play();

            yield return new WaitForSeconds(attackCooltime);
        }

        isCoroutineRun = false;
    }
}
