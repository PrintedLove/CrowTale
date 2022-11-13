using Com.LuisPedroFonseca.ProCamera2D;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using TMPro;
using UnityEditorInternal;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class SummonerBeat : MonoBehaviour
{
    public float speed, rotateSpeed;
    [SerializeField] private Sprite[] sprites;
    [SerializeField] private GameObject moveVFX, touchVFX, destroyVFX, healText;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private float angle;
    private short contactCount;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        angle = Random.Range(0, 360);
        contactCount = 0;
    }

    private void Start()
    {
        Vector3 vector = Quaternion.AngleAxis(Random.Range(0, 360), Vector3.forward) * Vector3.right;
        rb.AddRelativeForce(vector * speed);
    }

    private void Update()
    {
        angle += rotateSpeed * Time.deltaTime;
        if(angle >= 360)
            angle -= 360;

        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    private void OnEnable()
    {
        Vector3 vector = Quaternion.AngleAxis(Random.Range(0, 360), Vector3.forward) * Vector3.right;
        rb.AddRelativeForce(vector * speed);
    }

    private void OnDisable()
    {
        rb.velocity = Vector3.zero;
        angle = Random.Range(0, 360);
        contactCount = 0;
        spriteRenderer.sprite = sprites[contactCount];
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision != null)
        {
            if(collision.gameObject.tag == "Player")
            {
                int healAmount = 5 + 5 * contactCount;
                GameManager.Instance.health += healAmount;
                if(GameManager.Instance.health > 100) GameManager.Instance.health = 100;

                GameObject HT = Instantiate(healText,
                        GameObject.FindWithTag("Player").transform.position + new Vector3(Random.Range(-0.25f, 0.25f)
                        , Random.Range(-0.25f, 0.25f), 0f), new Quaternion(0, 0, 0, 0));
                HT.GetComponent<TextMeshPro>().text = healAmount.ToString();

                destroyVFX.transform.position = transform.position;
                destroyVFX.GetComponent<ParticleSystem>().Play();

                gameObject.SetActive(false);
                return;
            }
            else if (collision.gameObject.tag == "Struct")
            {
                contactCount++;

                if (contactCount > 4)
                {
                    destroyVFX.transform.position = transform.position;
                    destroyVFX.GetComponent<ParticleSystem>().Play();

                    gameObject.SetActive(false);
                    return;
                }

                spriteRenderer.sprite = sprites[contactCount];

                touchVFX.transform.position = collision.contacts[0].point;
                float angle = Mathf.Atan2(collision.contacts[0].point.y - transform.position.y,
                            collision.contacts[0].point.x - transform.position.x) * Mathf.Rad2Deg;
                touchVFX.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
                touchVFX.GetComponent<ParticleSystem>().Play();
            }
        }
    }
}
