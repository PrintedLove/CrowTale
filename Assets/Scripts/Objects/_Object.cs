using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public enum _ObjectType
{
    Dummy, Snake, Spike, WoodenBox, MovingPlatform, CircleBlade, Summoner
}

public class _Object : MonoBehaviour
{
    public _ObjectType objType;         //object type
    public bool isHit = true;       //Whether the player is attacked

    public int maxHealth = 100;     //maximum HP
    public int health = 100;        //current HP

    public int angerAmount = 3;     //Anger Charges When Hit by Player Attacks

    public itemType dropItemType;
    public GameObject dropItem, deathEffect;      //object created upon destruction

    protected bool isDie = false;

    [ContextMenu("Reset Position")]
    public void ResetPosition()
    {
        float p_x = transform.position.x;
        float p_y = transform.position.y;
        float np_x = System.Math.Abs(p_x % 1);
        float np_y = System.Math.Abs(p_y % 1);

        if (np_x < 0.15f || np_x > 0.85f)
            p_x = (float)System.Math.Truncate(p_x);
        else
            p_x = (float)System.Math.Truncate(p_x) + Mathf.Sign(p_x) * 0.5f;

        if (np_y < 0.15f || np_y > 0.85f)
            p_y = (float)System.Math.Truncate(p_y);
        else
            p_y = (float)System.Math.Truncate(p_y) + Mathf.Sign(p_y) * 0.5f;

        transform.position = new Vector3(p_x, p_y, transform.position.z);
    }

    //damage handling
    public virtual void TakeDamage(int damage, float hitDir)
    {
        health -= damage;

        if(angerAmount > 0)
            GameManager.Instance.increaseAngerLevel(angerAmount);

        if (health <= 0 && !isDie)
        {
            Die();
        }

        CheckHP();
    }

    //Function called whenever HP changes
    public virtual void CheckHP()
    {

    }
    
    //Death
    protected virtual void Die()
    {
        isDie = true;

        if (dropItem != null)
        {
            GameObject DI = Instantiate(dropItem, transform.position, Quaternion.identity);
            DI.GetComponent<Items>().SetType(dropItemType);
            DI.GetComponent<Rigidbody2D>().AddForce(Vector2.up * 8f, ForceMode2D.Impulse);
        }

        Instantiate(deathEffect, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}