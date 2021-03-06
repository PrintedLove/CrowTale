using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WoodenBox : _Object
{
    public Sprite[] sprite;

    protected override void Awake()
    {
        base.Awake();
        
        objType = _ObjectType.WoodenBox;
    }

    public override void CheckHP()
    {
        if (health <= maxHealth * 30 / 100) 
            spriteRenderer.sprite = sprite[1];
        else if (health <= maxHealth * 75 / 100)
            spriteRenderer.sprite = sprite[0];
    }
}
