using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// tile group type
enum Tiletype :int
{
    Stone_1, Stone_2, Stone_3, Stone_4, Bush_1, Bush_2, Bush_3, Bush_4
    ,Tree_1, Tree_2, Tree_3, Tree_4, Tree_5, Tree_6
}

public class DecoratiionTileGenerator : MonoBehaviour
{
    //Structure for storing tile list according to tile group type
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

    public GameObject tilemap_Ground, tilemap_Stones, tilemap_Bushs, tilemap_Trees, tilemap_Objects;
    public Tile[] tiles;
    List<DecoratiionTile> Tileset = new List<DecoratiionTile>();

    private void Awake() {
        //Save tile list to tile group list
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
        //Create a tile in a subordinate update statement to receive the location information of other tiles
        GenerateTile();
        Destroy(this);
    }

    void GenerateTile()
    {
        /*Gets a list of objects created on the platform tile with an empty space at the top by
        Ground Rule Tile(Assets - Others - Tilemaps).*/
        GameObject[] objList = GameObject.FindGameObjectsWithTag("Decoration Tile Generator");

        for (int i = 0; i < objList.Length; i++)
        {
            Vector3 objPosRaw = objList[i].GetComponent<Transform>().position;      //actual object position
            Vector3Int objPos = new Vector3Int((int)objPosRaw.x, (int)objPosRaw.y, 0);      //float -> Int object position
            Vector3Int setPos = new Vector3Int(objPos.x, objPos.y + 1, objPos.z);           //start creating tiles position

            if (calProbability(32))
            {
                 int tiletype;

                //When a platform tile is located at the bottom and there is no platform tile in the place to be created.
                if (tilemap_Ground.GetComponent<Tilemap>().GetTile(objPos) != null
                 && tilemap_Ground.GetComponent<Tilemap>().GetTile(new Vector3Int(objPos.x, objPos.y + 1, objPos.z)) == null) {
                    if(calProbability(19))      // 19% chance to create a tree
                    {
                        //If there are no other decoration tiles in the spawn location
                        if (tilemap_Ground.GetComponent<Tilemap>().GetTile(new Vector3Int(objPos.x + 1, objPos.y, objPos.z)) != null
                         && tilemap_Stones.GetComponent<Tilemap>().GetTile(new Vector3Int(objPos.x + 1, objPos.y + 1, objPos.z)) == null
                         && tilemap_Bushs.GetComponent<Tilemap>().GetTile(new Vector3Int(objPos.x + 1, objPos.y + 1, objPos.z)) == null)
                        {
                            tiletype = Random.Range((int)Tiletype.Tree_1, (int)Tiletype.Tree_6 + 1);
                            bool isGenOk = true;

                            //Checks that tile groups of the same tilemap do not overlap at the creation location
                            for (int n = 0; n < Tileset[tiletype].tileList.Length; n++)
                            {
                                if(tilemap_Trees.GetComponent<Tilemap>().GetTile(new Vector3Int(setPos.x + (n % 2), setPos.y + n / 2, objPos.z)) != null)
                                    isGenOk = false;
                            }

                            if(isGenOk)
                            {
                                for (int n = 0; n < Tileset[tiletype].tileList.Length; n++)
                                {
                                    tilemap_Trees.GetComponent<Tilemap>().SetTile(
                                        new Vector3Int(setPos.x + (n % 2), setPos.y + n / 2, objPos.z), Tileset[tiletype].tileList[n]);
                                }
                            }
                        }
                    }
                    else 
                    {
                        if(calProbability(30))  //30% chance to generate a rock if it is not a tree
                        {
                            tiletype = Random.Range((int)Tiletype.Stone_1, (int)Tiletype.Stone_4 + 1);
                            tilemap_Stones.GetComponent<Tilemap>().SetTile(setPos, Tileset[tiletype].tileList[0]);
                        }
                        else
                        {
                            tiletype = Random.Range((int)Tiletype.Bush_1, (int)Tiletype.Bush_4 + 1);
                            tilemap_Bushs.GetComponent<Tilemap>().SetTile(setPos, Tileset[tiletype].tileList[0]);
                        }
                    }
                }
            }

            Destroy(objList[i].gameObject);     //Remove object for position detection
            objList[i] = null;
        }

        tilemap_Stones.GetComponent<TilemapCollider2D>().enabled = true;
        tilemap_Stones.GetComponent<TilemapShadowCaster2DCustom>().MakeShadow();    //Shadowcaster Dynamic Creation
        tilemap_Bushs.GetComponent<TilemapCollider2D>().enabled = true;
        tilemap_Bushs.GetComponent<TilemapShadowCaster2DCustom>().MakeShadow();
    }

    bool calProbability(byte chance)    //Functions for calculating percent probability
    {
        if (chance >= Random.Range(1, 101))
            return true;
        else
            return false;
    }
}
