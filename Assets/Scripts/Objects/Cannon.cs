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

    void Start()
    {
        StartCoroutine(RunCannonAttack());
    }

    IEnumerator RunCannonAttack()     //공격 주기
    {
        while (isAttack)
        {
            GameObject cannonBullet = Instantiate(cannonBulletObject, firePoint.position, firePoint.rotation);
            cannonBullet.GetComponent<Rigidbody2D>().velocity = -transform.right * bulletSpeed;

            Instantiate(shotEffectObject, firePoint.position, firePoint.rotation);

            yield return new WaitForSeconds(attackCooltime);
        }
    }
}
