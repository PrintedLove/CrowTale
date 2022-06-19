using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TilemapShadowCaster2DCustom : MonoBehaviour
{
    [SerializeField]
    protected CompositeCollider2D m_TilemapCollider;

    [SerializeField]
    protected bool m_SelfShadows = true;

    public void MakeShadow()
    {
        m_TilemapCollider = GetComponent<CompositeCollider2D>();
        ShadowCaster2DGenerator.GenerateTilemapShadowCasters(m_TilemapCollider, m_SelfShadows);
    }
}
