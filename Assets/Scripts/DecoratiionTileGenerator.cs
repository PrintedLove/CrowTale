using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

enum Tiletype:int
{
    Stone_1, Stone_2, Stone_3, Stone_4, Bush_1, Bush_2, Bush_3, Bush_4
    ,Tree_1, Tree_2, Tree_3, Tree_4, Tree_5, Tree_6
}

public class DecoratiionTileGenerator : MonoBehaviour
{
    struct DecoratiionTile
    {
        public Tiletype name;
        public Tile[] tileList;

        public DecoratiionTile(Tiletype name, Tile[] tileList)
        {
            this.name = name;
            this.tileList = tileList;
        }

        public DecoratiionTile(Tiletype name, Tile tileList)
        {
            this.name = name;
            this.tileList = new Tile[1];
            this.tileList[0] = tileList;
        }
    }

    public GameObject tilemap_Stones, tilemap_Bushs, tilemap_Trees;
    public Tile[] tiles;
    List<DecoratiionTile> Tileset = new List<DecoratiionTile>();

    private void Awake() {
        Tileset.Add(new DecoratiionTile(Tiletype.Stone_1, tiles[0]));
        Tileset.Add(new DecoratiionTile(Tiletype.Stone_2, tiles[1]));
        Tileset.Add(new DecoratiionTile(Tiletype.Stone_3, tiles[2]));
        Tileset.Add(new DecoratiionTile(Tiletype.Stone_4, tiles[3]));
        Tileset.Add(new DecoratiionTile(Tiletype.Bush_1, tiles[4]));
        Tileset.Add(new DecoratiionTile(Tiletype.Bush_2, tiles[5]));
        Tileset.Add(new DecoratiionTile(Tiletype.Bush_3, tiles[6]));
        Tileset.Add(new DecoratiionTile(Tiletype.Bush_4, tiles[7]));
        Tileset.Add(new DecoratiionTile(Tiletype.Tree_1
        , new Tile[8] {tiles[34], tiles[35], tiles[22], tiles[23], tiles[12], tiles[13], tiles[8], tiles[9]}));
        Tileset.Add(new DecoratiionTile(Tiletype.Tree_2
        , new Tile[8] {tiles[36], tiles[37], tiles[24], tiles[25], tiles[14], tiles[15], tiles[10], tiles[11]}));
        Tileset.Add(new DecoratiionTile(Tiletype.Tree_3
        , new Tile[6] {tiles[38], tiles[39], tiles[26], tiles[27], tiles[16], tiles[17]}));
        Tileset.Add(new DecoratiionTile(Tiletype.Tree_4
        , new Tile[6] {tiles[40], tiles[41], tiles[28], tiles[29], tiles[18], tiles[19]}));
        Tileset.Add(new DecoratiionTile(Tiletype.Tree_5
        , new Tile[6] {tiles[42], tiles[43], tiles[30], tiles[31], tiles[20], tiles[21]}));
        Tileset.Add(new DecoratiionTile(Tiletype.Tree_6
        , new Tile[4] {tiles[44], tiles[45], tiles[32], tiles[33]}));
    }

    private void LateUpdate() {
        GenerateTile();
        Destroy(this);
    }

    void GenerateTile()
    {
        GameObject[] objList = GameObject.FindGameObjectsWithTag("Decoration Tile Generator");

        for (int i = 0; i < objList.Length; i++)
        {
            Vector3 objPosRaw = objList[i].GetComponent<Transform>().position;
            Vector3Int objPos = new Vector3Int((int)objPosRaw.x - 1, (int)objPosRaw.y + 1, -1);

            if (calProbability(35))
            {
                 int tiletype;

                if(calProbability(20))
                {
                    tiletype = Random.Range((int)Tiletype.Tree_1, (int)Tiletype.Tree_6 + 1);

                    for (int n = 0; n < Tileset[tiletype].tileList.Length; n++)
                    {
                        
                        tilemap_Trees.GetComponent<Tilemap>().SetTile(new Vector3Int((int)objPos.x + (n % 2), objPos.y + n / 2, objPos.z), Tileset[tiletype].tileList[n]);
                    }
                }
                else 
                {
                    if(calProbability(40))
                    {
                        tiletype = Random.Range((int)Tiletype.Stone_1, (int)Tiletype.Stone_4 + 1);
                        tilemap_Stones.GetComponent<Tilemap>().SetTile(objPos, Tileset[tiletype].tileList[0]);
                    }
                    else
                    {
                        tiletype = Random.Range((int)Tiletype.Bush_1, (int)Tiletype.Bush_4 + 1);
                        tilemap_Bushs.GetComponent<Tilemap>().SetTile(objPos, Tileset[tiletype].tileList[0]);
                    }
                }
            }

            Destroy(objList[i].gameObject);
            objList[i] = null;
        }

        tilemap_Stones.GetComponent<TilemapCollider2D>().enabled = true;
        tilemap_Stones.GetComponent<TilemapShadowCaster2DCustom>().MakeShadow();
        tilemap_Bushs.GetComponent<TilemapCollider2D>().enabled = true;
        tilemap_Bushs.GetComponent<TilemapShadowCaster2DCustom>().MakeShadow();
    }

    bool calProbability(byte chance)
    {
        if (chance >= Random.Range(1, 101))
            return true;
        else
            return false;
    }
}
