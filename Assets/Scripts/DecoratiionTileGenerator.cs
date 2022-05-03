using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// 타일 그룹 종류
enum Tiletype:int
{
    Stone_1, Stone_2, Stone_3, Stone_4, Bush_1, Bush_2, Bush_3, Bush_4
    ,Tree_1, Tree_2, Tree_3, Tree_4, Tree_5, Tree_6
}

public class DecoratiionTileGenerator : MonoBehaviour
{
    //타일 그룹 종류에 따른 타일 리스트 저장용 구조체
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
        //타일 그룹 리스트에에 타일리스트 저장
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
        //다른 타일들의 위치정보를 받아오기 위해서 후순위 업데이트문에서 타일 생성
        GenerateTile();
        Destroy(this);
    }

    void GenerateTile()
    {
        //Ground Rule Tile(Assets - Others - Tilemaps)에 의해 상단이 빈공간인 플랫폼 타일위에 생성된 오브젝트 리스트를 받아옴.
        GameObject[] objList = GameObject.FindGameObjectsWithTag("Decoration Tile Generator");

        for (int i = 0; i < objList.Length; i++)
        {
            Vector3 objPosRaw = objList[i].GetComponent<Transform>().position;      //실제 오브젝트 위치
            Vector3Int objPos = new Vector3Int((int)objPosRaw.x, (int)objPosRaw.y, 0);      //float -> Int 처리한 오브젝트 위치
            Vector3Int setPos = new Vector3Int(objPos.x, objPos.y + 1, objPos.z);           //타일 생성 시작 위치

            if (calProbability(35))
            {
                 int tiletype;

                //하단에 플랫폼 타일이 위치하고, 생성할 위치에 플랫폼 타일이 없을 경우.
                if (tilemap_Ground.GetComponent<Tilemap>().GetTile(objPos) != null
                 && tilemap_Ground.GetComponent<Tilemap>().GetTile(new Vector3Int(objPos.x, objPos.y + 1, objPos.z)) == null) {
                    if(calProbability(19))      // 19%확률로 나무 생성
                    {
                        //생성 위치에 다른 데코레이션 타일이 없을 경우
                        if(tilemap_Ground.GetComponent<Tilemap>().GetTile(new Vector3Int(objPos.x + 1, objPos.y, objPos.z)) != null
                         && tilemap_Stones.GetComponent<Tilemap>().GetTile(new Vector3Int(objPos.x + 1, objPos.y + 1, objPos.z)) == null
                         && tilemap_Bushs.GetComponent<Tilemap>().GetTile(new Vector3Int(objPos.x + 1, objPos.y + 1, objPos.z)) == null)
                        {
                            tiletype = Random.Range((int)Tiletype.Tree_1, (int)Tiletype.Tree_6 + 1);
                            bool isGenOk = true;

                            //생성 위치에 같은 타일맵의 타일 그룹이 겹치지 않는지 검사
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
                        if(calProbability(38))  //나무가 아닐시 38% 확률로 암석 생성
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

            Destroy(objList[i].gameObject);     //위치 감지용 오브젝트 제거
            objList[i] = null;
        }

        tilemap_Stones.GetComponent<TilemapCollider2D>().enabled = true;
        tilemap_Stones.GetComponent<TilemapShadowCaster2DCustom>().MakeShadow();    //쉐도우캐스터 동적 생성
        tilemap_Bushs.GetComponent<TilemapCollider2D>().enabled = true;
        tilemap_Bushs.GetComponent<TilemapShadowCaster2DCustom>().MakeShadow();
    }

    bool calProbability(byte chance)    //퍼센트 확률 계산용 함수
    {
        if (chance >= Random.Range(1, 101))
            return true;
        else
            return false;
    }
}
