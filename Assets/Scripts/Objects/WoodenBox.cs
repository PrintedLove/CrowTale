using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WoodenBox : _Object
{
    public Sprite[] sprite;
    protected SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        objType = _ObjectType.WoodenBox;
    }

    private void FixedUpdate()
    {
        if (transform.position.y < -64)
            Destroy(gameObject);
    }

    public override void CheckHP()
    {
        if (health <= maxHealth * 30 / 100)
            spriteRenderer.sprite = sprite[1];
        else if (health <= maxHealth * 75 / 100)
            spriteRenderer.sprite = sprite[0];
    }
}
