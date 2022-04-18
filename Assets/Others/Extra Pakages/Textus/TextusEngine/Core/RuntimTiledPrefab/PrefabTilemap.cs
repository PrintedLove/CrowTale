// Creator: TextusGames

using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Tilemap))]
public class PrefabTilemap : MonoBehaviour
{
    void Start()
    {
        var map = GetComponent<Tilemap>();

        foreach (var pos in map.cellBounds.allPositionsWithin)
        {   
            Vector3Int localPlace = new Vector3Int(pos.x, pos.y, pos.z);
            //Vector3 place = map.CellToWorld(localPlace);
            if (map.HasTile(localPlace))
            {   
                var tilePrefab = map.GetTile<PrefabTile>(localPlace);
                //print(tilePrefab);
                if (!tilePrefab) continue;
                tilePrefab.InstantiatePrefab(map,localPlace);
            }
        }

    }
   
}
