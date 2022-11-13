using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public enum itemType
{
    heal, power, energy, anger
}

public class Items : MonoBehaviour
{
    public itemType type;
    public float nSpeed = 10f, mSpeed = 5f;

    [SerializeField] private Sprite[] spr;
    [SerializeField] private GameObject destroyEffect, healText;

    private GameObject player;
    private Vector3 playerDir;
    private short isMove;

    private Animator animator;
    private Rigidbody2D rigidBody;


    void Awake()
    {
        isMove = 0;
        player = GameObject.FindWithTag("Player");

        animator = GetComponent<Animator>();
        rigidBody = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (isMove == 2)
        {
            playerDir = (player.transform.position - this.transform.position).normalized;
            rigidBody.AddForce(new Vector2(playerDir.x * mSpeed * Time.deltaTime, playerDir.y * mSpeed * Time.deltaTime));
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player" && !GameManager.Instance.isPlayerDie)
        {
            if(isMove == 0)
            {
                playerDir = (player.transform.position - this.transform.position).normalized;
                rigidBody.AddForce(new Vector2(-playerDir.x * nSpeed, -playerDir.y * nSpeed));

                SoundManager.Instance.Play(SoundManager.AS.playerInteract, SoundManager.UISound.itemGet1);
                isMove = 1;
                StartCoroutine(RunGetItem());
                Destroy(gameObject, 10f);
            }
            else if (isMove == 2)
            {
                if (type == itemType.heal)      // HP heal
                {
                    GameManager.Instance.health = 100;

                    GameObject HT = Instantiate(healText,
                        player.transform.position + new Vector3(Random.Range(-0.25f, 0.25f)
                        , Random.Range(-0.25f, 0.25f), 0f), new Quaternion(0, 0, 0, 0));
                    HT.GetComponent<TextMeshPro>().text = "100";
                }
                else if (type == itemType.power)    // increase Max Power
                {
                    GameManager.Instance.increasePower(2);
                }
                else if (type == itemType.energy)   // increase stamina
                {
                    GameManager.Instance.stamina = 100;
                }
                else                // increase anger
                {
                    GameManager.Instance.increaseAngerLevel(101);
                }

                SoundManager.Instance.Play(SoundManager.AS.playerInteract, SoundManager.UISound.itemGet2);
                rigidBody.velocity = new Vector2(0f, 0f);
                animator.SetTrigger("isDestroy");
                isMove = 3;
            }
        }
    }

    public void SetType(itemType it)
    {
        type = it;
        GetComponent<SpriteRenderer>().sprite = spr[(int)it];
    }

    public void DestroySelf()
    {
        Instantiate(destroyEffect, transform.position, transform.rotation);
        Destroy(gameObject);
    }

    IEnumerator RunGetItem()
    {
        yield return new WaitForSeconds(1f);

        rigidBody.velocity = new Vector2(0f, 0f);
        isMove = 2;
    }
}
