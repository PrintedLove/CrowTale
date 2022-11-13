using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckDecorationTilemapGenerator : MonoBehaviour
{
    string[] layers = { "Default", "Object", "NonTouchObject", "Platform" };
    private void Update()
    {
        if(Physics2D.OverlapBox(new Vector2(transform.position.x, transform.position.y + 1f)
            , new Vector2(0.5f, 0.5f), 0, LayerMask.GetMask(layers)))
            Destroy(gameObject);
    }
}
