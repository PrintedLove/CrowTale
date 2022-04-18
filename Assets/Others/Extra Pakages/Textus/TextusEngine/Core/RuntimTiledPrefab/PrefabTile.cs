 // Creator: TextusGames

 using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "TiledPrefab" ,fileName = "TiledPrefab", order = 360)]
public class PrefabTile : TileBase
{
    public bool Unparent;
    public Sprite PreviewSprite;
    public GameObject Prefab;
    public Vector3 PrefabOffset = new Vector3(0.5f,0.5f,0);
    
    public override bool StartUp(Vector3Int position, ITilemap tilemap, GameObject go)
    {
        return false;
    }
  
    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
    {
        tileData.sprite = Application.isPlaying ? null : PreviewSprite;
    }

    public  void InstantiatePrefab(Tilemap map, Vector3Int position)
    {
        if (map.gameObject.layer == 31) return ;

        if (!Application.isPlaying) return;

        //Debug.Log(Prefab);

        if (Prefab)
        {
            var instance = Instantiate(Prefab);
            instance.transform.position = position + PrefabOffset;
            instance.transform.SetParent(map.transform);
        }
        map.SetTile(position,null);
    }
}

