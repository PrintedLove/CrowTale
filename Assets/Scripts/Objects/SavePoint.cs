using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

public class SavePoint : MonoBehaviour
{
    [SerializeField] private bool isTouch, isActObject;

    public GameObject[] objs;
    public Vector2 BoxPosition, BoxSize;

    Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
        isTouch = false;
        isActObject = false;
    }

    private void FixedUpdate()
    {
        if(!isActObject)
        {
            if (Physics2D.OverlapBox(BoxPosition, BoxSize, 0, LayerMask.GetMask("Player")))
            {
                isActObject = true;
                ActiveObject(true);

                if (!GameManager.Instance.preSavePoints.Contains(gameObject))
                    GameManager.Instance.preSavePoints.Add(gameObject);
            }
        }
    }

    public void ActiveObject(bool isActive)
    {
        if (objs.Length == 0) return;

        foreach(GameObject obj in objs)
        {
            if(obj != null)
                obj.SetActive(isActive);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        
        if (!isTouch && collision.gameObject.tag == "Player")
        {
            isTouch = true;
            animator.SetTrigger("isTouch");

            if(!isActObject)
            {
                isActObject = true;
                ActiveObject(true);
            }

            if (!GameManager.Instance.preSavePoints.Contains(gameObject))
                GameManager.Instance.preSavePoints.Add(gameObject);
            GameManager.Instance.DisablePreSavePointObject(gameObject);
            GameManager.Instance.respawnPosition = new Vector3(transform.position.x, transform.position.y + 2f, 0f);

            SoundManager.Instance.Play(SoundManager.AS.UI, SoundManager.UISound.savePoint);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(BoxPosition, BoxSize);
    }
}
