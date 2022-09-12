using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SavePoint : MonoBehaviour
{
    bool isTouch = false;
    Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        
        if (collision.gameObject.tag == "Player" && isTouch == false)
        {
            isTouch = true;
            animator.SetTrigger("isTouch");
            GameManager.Instance.respawnPosition = new Vector3(transform.position.x, transform.position.y + 2f, 0f);

            SoundManager.Instance.Play(SoundManager.AS.UI, SoundManager.UISound.savePoint);
        }
    }
}
