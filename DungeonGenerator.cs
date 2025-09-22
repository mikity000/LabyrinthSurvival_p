using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

/// <summary>
/// GameManagerオブジェクトにアタッチ
/// ダンジョンのランダム生成、Player・Enemyをランダムで配置するクラス
/// </summary>
public class DungeonGenerator : MonoBehaviour, ISavable {
    private enum Tile
    {
        wall,
        aisle,
        room = 11
    }

    [SerializeField] private GameObject floor;
    [SerializeField] private GameObject aisle;
    [SerializeField] private GameObject wall;
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject goal;
    [SerializeField] private GameObject treasure;
    [SerializeField] private Table<GameObject>[] enemyArray;
    [SerializeField] private int enemyCount;
    [SerializeField] private GameObject waypoint;
    [SerializeField] private List<GameObject> allPrefab;
    public static bool isFinish = false;

    [Header("マップ全体の大きさ")]
    public Vector2 MapSize;

    [Header("壁の高さ")]
    [SerializeField]
    int WallHeght;

    [HideInInspector]
    private Tile[,] Map; //マップ情報

    [Header("部屋の数 最小,最大\n※絶対ではなく、最小値を下回る可能性があります")]
    [SerializeField]
    [Range(1, 10)]
    int MinRooms;
    [SerializeField]
    [Range(1, 20)]
    int MaxRooms;
    int roomNum;//MinRoomsからMaxRoomsまでのランダムな値を代入

    [Header("部屋の一辺の最小サイズ")]
    [SerializeField]
    [Range(4, 16)]
    int roomMinSize;

    List<RoomInfo> roomSplit = new List<RoomInfo>();

    //ここから追加
    private ActorMovement playerMovement;
    [Header("オブジェクトの親")]
    [SerializeField] private Transform floorParent;
    [SerializeField] private Transform aisleParent;
    [SerializeField] private Transform wallParent;
    [SerializeField] private Transform playerParent;
    [SerializeField] private Transform goalParent;
    [SerializeField] private Transform treasureParent;
    [SerializeField] private Transform enemyParent;
    [SerializeField] private Transform itemParent;
    [SerializeField] private Transform waypointParent;

    private void Awake()
    {
        //セーブデータに既にダンジョンが保存されていればreturn
        if (SaveLoadSystem.Instance.HasKey(name))
            return;

        Create();
    }
    public void Create()
    {
        MapResetData();
        MapSplit();
        CreateRoom();
        CreateRoad();
        CreateDungeon();
    }

    //ダンジョンを壁だけにする
    private void MapResetData()
    {
        Map = new Tile[(int)MapSize.x, (int)MapSize.y]; //Mapデータ[横,縦]

        // 壁しかないMapデータを作る
        for (int i = 0; i < MapSize.x; i++)
        {
            for (int j = 0; j < MapSize.y; j++)
            {
                Map[i, j] = Tile.wall;
            }
        }
    }
    private void MapSplit()
    {
        roomSplit.Add(new RoomInfo());
        roomNum = Random.Range(MinRooms, MaxRooms); //部屋数を決める
                                                    // 大部屋作成
        roomSplit[0].Top = 0;
        roomSplit[0].Left = 0;
        roomSplit[0].Bottom = (int)MapSize.y - 1;
        roomSplit[0].Right = (int)MapSize.x - 1;
        roomSplit[0].area = roomSplit[0].Bottom * roomSplit[0].Right; //部屋の大きさ

        for (int i = 1; i < roomNum; i++)
        {
            roomSplit.Add(new RoomInfo());
            int target = 0; //分割する部屋
            int areaMax = 0; //最大面積だった部屋の面積

            // 最大面積を持つ部屋を求める
            for (int j = 0; j < i; j++)
            {
                if (roomSplit[j].area >= areaMax)
                {
                    areaMax = roomSplit[j].area;
                    target = j;
                }
            }
            // 分割点を求める
            if (roomSplit[target].Right - roomSplit[target].Left >= roomSplit[target].Bottom - roomSplit[target].Top)
            {
                //横分割
                // 縦横の大きさが最小値*2より大きい場合は実行
                if (roomSplit[target].Right - roomSplit[target].Left >= roomMinSize * 2 && roomSplit[target].Bottom - roomSplit[target].Top >= roomMinSize * 2)
                {

                    // 分割点を求めて左座標へ入力する
                    roomSplit[i].Left = roomSplit[target].Left + Random.Range(roomMinSize, roomSplit[target].Right - roomSplit[target].Left - roomMinSize);
                    roomSplit[i].Right = roomSplit[target].Right;
                    roomSplit[target].Right = roomSplit[i].Left - 1;
                    roomSplit[i].Top = roomSplit[target].Top;
                    roomSplit[i].Bottom = roomSplit[target].Bottom;

                    for (int id = roomSplit[target].childRoom.Count - 1; id >= 0; id--)
                    {
                        if (target == roomSplit[roomSplit[target].childRoom[id]].parentRoom)
                        {
                            if (roomSplit[target].Right < roomSplit[roomSplit[target].childRoom[id]].Left)
                            {
                                roomSplit[roomSplit[target].childRoom[id]].parentRoom = i;
                                roomSplit[i].childRoom.Add(roomSplit[target].childRoom[id]);
                                roomSplit[i].isSplitX.Add(true);
                                roomSplit[i].childSplitPos.Add(roomSplit[roomSplit[target].childRoom[id]].Left);

                                roomSplit[target].childRoom.RemoveAt(id);
                                roomSplit[target].isSplitX.RemoveAt(id);
                                roomSplit[target].childSplitPos.RemoveAt(id);
                                break;

                            }
                        }
                    }
                    roomSplit[i].parentRoom = target;
                    roomSplit[target].childRoom.Add(i);
                    roomSplit[target].isSplitX.Add(true);
                    roomSplit[target].childSplitPos.Add(roomSplit[i].Left);
                }
                else
                {
                    roomNum = i;
                    break;
                }
                roomSplit[i].area = (roomSplit[i].Right - roomSplit[i].Left) * (roomSplit[i].Bottom - roomSplit[i].Top);
                roomSplit[target].area = (roomSplit[target].Right - roomSplit[target].Left) * (roomSplit[target].Bottom - roomSplit[target].Top);

            }
            else
            {
                //縦分割
                // 縦横の大きさが最小値*2より大きい場合は実行
                if (roomSplit[target].Right - roomSplit[target].Left >= roomMinSize * 2 && roomSplit[target].Bottom - roomSplit[target].Top >= roomMinSize * 2)
                {

                    // 分割点を求めて左座標へ入力する
                    roomSplit[i].Top = roomSplit[target].Top + Random.Range(roomMinSize, roomSplit[target].Bottom - roomSplit[target].Top - roomMinSize);
                    roomSplit[i].Bottom = roomSplit[target].Bottom;
                    roomSplit[target].Bottom = roomSplit[i].Top - 1;
                    roomSplit[i].Left = roomSplit[target].Left;
                    roomSplit[i].Right = roomSplit[target].Right;

                    for (int id = roomSplit[target].childRoom.Count - 1; id >= 0; id--)
                    {
                        if (target == roomSplit[roomSplit[target].childRoom[id]].parentRoom)
                        {
                            if (roomSplit[target].Bottom < roomSplit[roomSplit[target].childRoom[id]].Top)
                            {
                                roomSplit[roomSplit[target].childRoom[id]].parentRoom = i;
                                roomSplit[i].childRoom.Add(roomSplit[target].childRoom[id]);
                                roomSplit[i].isSplitX.Add(false);
                                roomSplit[i].childSplitPos.Add(roomSplit[roomSplit[target].childRoom[id]].Top);

                                roomSplit[target].childRoom.RemoveAt(id);
                                roomSplit[target].isSplitX.RemoveAt(id);
                                roomSplit[target].childSplitPos.RemoveAt(id);
                                break;

                            }
                        }
                    }
                    roomSplit[i].parentRoom = target;
                    roomSplit[target].childRoom.Add(i);
                    roomSplit[target].isSplitX.Add(false);
                    roomSplit[target].childSplitPos.Add(roomSplit[i].Top);
                }
                else
                {
                    roomNum = i;
                    break;
                }
                roomSplit[i].area = (roomSplit[i].Right - roomSplit[i].Left) * (roomSplit[i].Bottom - roomSplit[i].Top);
                roomSplit[target].area = (roomSplit[target].Right - roomSplit[target].Left) * (roomSplit[target].Bottom - roomSplit[target].Top);

            }
        }
    }
    private void CreateRoom()
    {
        int ratioX; //範囲を狭めた部屋の幅
        int ratioY; //範囲を狭めた部屋の高さ
        int moveX;  //範囲を狭めた時、範囲を動かす幅
        int moveY;  //範囲を狭めた時、範囲を動かす高さ
        for (int i = 0; i < roomNum; i++)   //作成した区画（部屋）数まで実行する
        {
            if (roomSplit[i] != null)
            {
                ratioY = roomSplit[i].Bottom - roomSplit[i].Top;    //部屋の高さを代入
                ratioY = Mathf.FloorToInt(Random.Range(0.60f, 0.80f) * ratioY);   //部屋の高さを乱数で調整
                ratioX = roomSplit[i].Right - roomSplit[i].Left;    //部屋の幅を代入
                ratioX = Mathf.FloorToInt(Random.Range(0.60f, 0.80f) * ratioX);   //部屋の幅を乱数で調整
                if (ratioY * 2 < ratioX)// 部屋が横長だった場合横の大きさを半分にする
                {
                    ratioX /= 2;
                }
                else if (ratioX * 2 < ratioY)// 部屋が縦長だった場合縦の大きさを半分にする
                {
                    ratioY /= 2;
                }


                moveY = (roomSplit[i].Bottom - roomSplit[i].Top - ratioY) / 2;  //上から下に動かす座標なので狭めた高さの半分を代入
                moveX = (roomSplit[i].Right - roomSplit[i].Left - ratioX) / 2;  //左から右に動かす座標なので狭めた幅の半分を代入
                roomSplit[i].Top += moveY;    // 区画の上座標に動かす高さの座標を足す
                roomSplit[i].Bottom = roomSplit[i].Top + ratioY;    // 区画の下座標に区画の上座標と部屋の高さを足す
                roomSplit[i].Left += moveX;   // 区画の左座標に動かす幅の座標を足す
                roomSplit[i].Right = roomSplit[i].Left + ratioX;    // 区画の右座標に区画の左座標と部屋の幅を足す

                // 部屋の範囲までMap配列に書き込む
                for (int j = 0; j < ratioY; j++)    //部屋の高さの範囲までループ
                {
                    for (int k = 0; k < ratioX; k++)    //部屋の幅の範囲までループ
                    {
                        Map[roomSplit[i].Left + k + 1, roomSplit[i].Top + j + 1] = Tile.room + i;  // 部屋の左側座標と増分をMapデータのXに、部屋の上側座標と増分をMapデータのYに設定する
                    }
                }
            }
            else
                break;
        }
    }

    private void CreateRoad()
    {
        int NowPos;
        int NowDis;
        int NextPos;
        int NextDis;
        int nowLines;
        int nextLines;

        for (int roomID = 0; roomID < roomNum; roomID++)
        {
            for (int childID = roomSplit[roomID].childRoom.Count - 1; childID >= 0; childID--)
            {
                if (roomSplit[roomID].isSplitX[childID])
                {
                    // X分割された
                    NowPos = roomSplit[roomID].Bottom - roomSplit[roomID].Top;
                    NowPos = Random.Range(1, NowPos) + roomSplit[roomID].Top;
                    NextPos = roomSplit[roomSplit[roomID].childRoom[childID]].Bottom - roomSplit[roomSplit[roomID].childRoom[childID]].Top;
                    NextPos = Random.Range(1, NextPos) + roomSplit[roomSplit[roomID].childRoom[childID]].Top;

                    NowDis = roomSplit[roomID].childSplitPos[childID] - roomSplit[roomID].Right + 1;
                    NextDis = roomSplit[roomSplit[roomID].childRoom[childID]].Left - roomSplit[roomID].childSplitPos[childID] + 1;

                    // ライン補正
                    if (roomSplit[roomID].Right + 1 < MapSize.x)
                    {
                        if (NowPos + 1 < MapSize.y)
                            if (Map[roomSplit[roomID].Right + 1, NowPos + 1] == Tile.aisle)
                                NowPos++;
                        if (NowPos - 1 > 0)
                            if (Map[roomSplit[roomID].Right + 1, NowPos - 1] == Tile.aisle)
                                NowPos--;
                    }

                    if (roomSplit[roomSplit[roomID].childRoom[childID]].Left - 1 > 0)
                    {
                        if (NextPos + 1 < MapSize.y)
                        {
                            if (Map[roomSplit[roomSplit[roomID].childRoom[childID]].Left - 1, NextPos + 1] == Tile.aisle)
                                NextPos++;
                        }
                        if (NextPos - 1 > 0)
                        {
                            if (Map[roomSplit[roomSplit[roomID].childRoom[childID]].Left - 1, NextPos - 1] == Tile.aisle)
                                NextPos--;
                        }
                    }




                    // 横ライン作成
                    // →ライン作成
                    for (nowLines = 0; nowLines < NowDis; nowLines++)
                    {
                        if (nowLines + roomSplit[roomID].Right < MapSize.x)
                        {
                            if (Map[nowLines + roomSplit[roomID].Right, NowPos] == Tile.wall)
                            {
                                Map[nowLines + roomSplit[roomID].Right, NowPos] = Tile.aisle;
                            }
                        }
                        else
                            break;

                    }

                    // ←ライン作成
                    for (nextLines = 0; nextLines < NextDis; nextLines++)
                    {
                        if (-nextLines + roomSplit[roomSplit[roomID].childRoom[childID]].Left > 0)
                        {
                            if (Map[-nextLines + roomSplit[roomSplit[roomID].childRoom[childID]].Left, NextPos] == Tile.wall)
                            {
                                Map[-nextLines + roomSplit[roomSplit[roomID].childRoom[childID]].Left, NextPos] = Tile.aisle;
                            }
                        }
                        else
                            break;


                    }


                    // 縦ライン作成
                    for (int lines = 0; ; lines++)
                    {
                        // NOWとNEXT、どちらの方が高さが大きいか調べ、縦ラインを作成する
                        if (NowPos >= NextPos)  //NOWの方が多い時（次の部屋の通路の方が上側）
                        {
                            if (NextPos + lines < NowPos)
                            {
                                if (Map[roomSplit[roomID].childSplitPos[childID], NextPos + lines] == Tile.wall)
                                {
                                    Map[roomSplit[roomID].childSplitPos[childID], NextPos + lines] = Tile.aisle;
                                }
                            }
                            else
                            {
                                RoadExtend(false, roomSplit[roomID].childSplitPos[childID], NextPos + lines);
                                break;
                            }
                        }
                        else    //NEXTの方が大きいとき（現在の部屋の通路の方が上側）
                        {
                            if (NowPos + lines < NextPos)
                            {
                                if (Map[roomSplit[roomID].childSplitPos[childID], NowPos + lines] == Tile.wall)
                                {
                                    Map[roomSplit[roomID].childSplitPos[childID], NowPos + lines] = Tile.aisle;
                                }
                            }
                            else
                            {
                                RoadExtend(false, roomSplit[roomID].childSplitPos[childID], NowPos + lines);
                                break;
                            }
                        }
                    }
                }
                else
                {
                    // Y分割された
                    NowPos = roomSplit[roomID].Right - roomSplit[roomID].Left;
                    NowPos = Random.Range(1, NowPos) + roomSplit[roomID].Left;
                    NextPos = roomSplit[roomSplit[roomID].childRoom[childID]].Right - roomSplit[roomSplit[roomID].childRoom[childID]].Left;
                    NextPos = Random.Range(1, NextPos) + roomSplit[roomSplit[roomID].childRoom[childID]].Left;

                    NowDis = roomSplit[roomID].childSplitPos[childID] - roomSplit[roomID].Bottom + 1;
                    NextDis = roomSplit[roomSplit[roomID].childRoom[childID]].Top - roomSplit[roomID].childSplitPos[childID] + 1;

                    // ラインの修正
                    // ライン補正
                    if (roomSplit[roomID].Bottom + 1 < MapSize.y)
                    {
                        if (NowPos + 1 < MapSize.x)
                            if (Map[NowPos + 1, roomSplit[roomID].Bottom + 1] == Tile.aisle)
                                NowPos++;
                        if (NowPos - 1 > 0)
                            if (Map[NowPos - 1, roomSplit[roomID].Bottom + 1] == Tile.aisle)
                                NowPos--;
                    }

                    if (roomSplit[roomSplit[roomID].childRoom[childID]].Top + 1 > 0)
                    {
                        if (NowPos + 1 < MapSize.x)
                            if (Map[NowPos + 1, roomSplit[roomSplit[roomID].childRoom[childID]].Top + 1] == Tile.aisle)
                                NowPos++;
                        if (NowPos - 1 > 0)
                            if (Map[NowPos - 1, roomSplit[roomSplit[roomID].childRoom[childID]].Top + 1] == Tile.aisle)
                                NowPos--;
                    }


                    // 縦ライン作成
                    // ↓ライン作成
                    for (nowLines = 0; nowLines < NowDis; nowLines++)
                    {
                        if (nowLines + roomSplit[roomID].Bottom < MapSize.y)
                        {
                            if (Map[NowPos, nowLines + roomSplit[roomID].Bottom] == Tile.wall)
                            {
                                Map[NowPos, nowLines + roomSplit[roomID].Bottom] = Tile.aisle;
                            }
                        }


                    }

                    // ↑ライン作成
                    for (nextLines = 0; nextLines < NextDis; nextLines++)
                    {
                        if (-nextLines + roomSplit[roomSplit[roomID].childRoom[childID]].Top > 0)
                        {
                            if (Map[NextPos, -nextLines + roomSplit[roomSplit[roomID].childRoom[childID]].Top] == Tile.wall)
                            {
                                Map[NextPos, -nextLines + roomSplit[roomSplit[roomID].childRoom[childID]].Top] = Tile.aisle;
                            }
                        }
                    }

                    // 横ライン作成
                    for (int lines = 0; ; lines++)
                    {
                        // NOWとNEXT、どちらの方が高さが大きいか調べ、縦ラインを作成する
                        if (NowPos >= NextPos)  //NOWの方が多い時（次の部屋の通路の方が上側）
                        {
                            if (NextPos + lines < NowPos)
                            {
                                if (Map[NextPos + lines, roomSplit[roomID].childSplitPos[childID]] == Tile.wall)  //読み込み元のIDが壁ならば（Y座標が変動）
                                    Map[NextPos + lines, roomSplit[roomID].childSplitPos[childID]] = Tile.aisle;   //そのIDを通路にする
                            }
                            else
                            {
                                RoadExtend(true, NextPos + lines, roomSplit[roomID].childSplitPos[childID]);
                                break;
                            }
                        }
                        else    //NEXTの方が大きいとき（現在の部屋の通路の方が上側）
                        {
                            if (NextPos > NowPos + lines)
                            {
                                if (Map[NowPos + lines, roomSplit[roomID].childSplitPos[childID]] == Tile.wall)    //読み込み元のIDが壁ならば（Y座標が変動）
                                    Map[NowPos + lines, roomSplit[roomID].childSplitPos[childID]] = Tile.aisle;    //そのIDを通路にする
                            }
                            else
                            {
                                RoadExtend(true, NowPos + lines, roomSplit[roomID].childSplitPos[childID]);
                                break;
                            }

                        }
                    }

                }
            }
        }
    }


    private void RoadExtend(bool isX, int x, int y)
    {
        bool isHit = false;
        int extendLine = 0;
        if (isX)
        {
            x++;
            for (; x + extendLine < MapSize.x; extendLine++)
            {
                if (Map[x + extendLine, y] == Tile.aisle || Map[x + extendLine, y] >= Tile.room)
                {
                    isHit = true;
                    break;
                }
                else if (Map[x + extendLine, y + 1] == Tile.aisle || Map[x + extendLine, y + 1] >= Tile.room ||
                    Map[x + extendLine, y - 1] == Tile.aisle || Map[x + extendLine, y - 1] >= Tile.room)
                {
                    extendLine++;
                    isHit = true;
                    break;
                }
            }
            if (isHit)
            {
                for (int Line = 0; Line < extendLine; Line++)
                    Map[x + Line, y] = Tile.aisle;
            }
        }
        else
        {
            y++;
            for (; y + extendLine < MapSize.y; extendLine++)
            {
                if (Map[x, y + extendLine] == Tile.aisle || Map[x, y + extendLine] >= Tile.room)
                {
                    isHit = true;
                    break;
                }
                else if (Map[x + 1, y + extendLine] == Tile.aisle || Map[x + 1, y + extendLine] >= Tile.room ||
                    Map[x - 1, y + extendLine] == Tile.aisle || Map[x - 1, y + extendLine] >= Tile.room)
                {
                    isHit = true;
                    extendLine++;
                    break;
                }
            }
            if (isHit)
            {
                for (int Line = 0; Line < extendLine; Line++)
                    Map[x, y + Line] = Tile.aisle;
            }
        }
    }

    private void CreateDungeon()
    {
        GameObject obj;
        //player、goal、enemy生成可能な座標List
        List<Vector2> instablePosList = new List<Vector2>();
        //エネミーの通過点(部屋の入口)座標List
        List<Vector2> waypointPosList = new List<Vector2>();
        for (int i = 0; i < MapSize.x; i++)
        {
            for (int j = 0; j < MapSize.y; j++)
            {
                if (Map[i, j] == Tile.wall)
                {
                    for (int height = 0; height < WallHeght; height++)
                    {
                        obj = Instantiate(wall, new Vector3(i - MapSize.x / 2, j - MapSize.y / 2, 0.1f), Quaternion.identity);
                        obj.transform.SetParent(wallParent);
                    }
                }
                else if (Map[i, j] == Tile.aisle)
                {
                    obj = Instantiate(aisle, new Vector3(i - MapSize.x / 2, j - MapSize.y / 2, 0.1f), Quaternion.identity);
                    obj.transform.SetParent(aisleParent);
                }
                //部屋だけ区別できるようにIDを割り振っている(1の位が部屋数)
                else if((int)Map[i, j] >= (int)Tile.room)
                {
                    obj = Instantiate(floor, new Vector3(i - MapSize.x / 2, j - MapSize.y / 2, 0.1f), Quaternion.identity);
                    obj.name += (int)Map[i, j];
                    obj.transform.SetParent(floorParent);
                    if (Map[i + 1, j] == Tile.aisle || Map[i, j + 1] == Tile.aisle || Map[i - 1, j] == Tile.aisle || Map[i, j - 1] == Tile.aisle)
                    {
                        waypointPosList.Add(new Vector2(i - MapSize.x / 2, j - MapSize.y / 2));
                        continue;
                    }
                    instablePosList.Add(new Vector2(i - MapSize.x / 2, j - MapSize.y / 2));
                }
            }
        }
        foreach (Vector2 waypointPos in waypointPosList)
        {
            obj = Instantiate(waypoint, waypointPos, Quaternion.identity);
            obj.transform.SetParent(waypointParent);
        }
        if (Random.Range(0, 100) < 7) {
            obj = Instantiate(treasure, RandomPosition(instablePosList), Quaternion.identity);
            obj.transform.SetParent(treasureParent);
        }
        obj = Instantiate(player, RandomPosition(instablePosList), Quaternion.identity);
        obj.transform.SetParent(playerParent);
        playerMovement = obj.GetComponent<ActorMovement>();
        obj = Instantiate(goal, RandomPosition(instablePosList), Quaternion.identity);
        obj.transform.SetParent(goalParent);
        //オブジェクトが生成可能な座標に敵を配置する
        PutEnemyAtRandom(instablePosList);
        isFinish = true;
    }

    //オブジェクトが生成可能な座標を一つ返す
    private Vector2 RandomPosition(List<Vector2> instablePosList)
    {
        int randomIndex = Random.Range(0, instablePosList.Count);
        Vector2 randomPos = instablePosList[randomIndex];
        instablePosList.RemoveAt(randomIndex);
        return randomPos;
    }

    //オブジェクトが生成可能な座標にオブジェクトを配置する
    private void PutEnemyAtRandom(List<Vector2> instablePosList)
    {
        for (int i = 0; i < enemyCount; i++)
        {
            //原因不明だが引継ぎ後始めるとnullになるので、null合体演算子を使う
            GameObject obj = WeightedLottery.DrawInRange(GameManager.instance?.game.stage ?? 1, enemyArray);
            obj = Instantiate(obj, RandomPosition(instablePosList), Quaternion.identity);
            obj.name += i + 1;
            obj.transform.SetParent(enemyParent);
        }
    }

    //進行方向に壁がないかチェック(ptは左下を原点にした座標)
    public bool IsWall(Vector2 pt, EDir d) {
        return d switch {
            EDir.RightUp => Map[(int)pt.x, (int)pt.y] == Tile.wall || Map[(int)pt.x - 1, (int)pt.y] == Tile.wall || Map[(int)pt.x, (int)pt.y - 1] == Tile.wall,
            EDir.RightDown => Map[(int)pt.x, (int)pt.y] == Tile.wall || Map[(int)pt.x - 1, (int)pt.y] == Tile.wall || Map[(int)pt.x, (int)pt.y + 1] == Tile.wall,
            EDir.LeftDown => Map[(int)pt.x, (int)pt.y] == Tile.wall || Map[(int)pt.x + 1, (int)pt.y] == Tile.wall || Map[(int)pt.x, (int)pt.y + 1] == Tile.wall,
            EDir.LeftUp => Map[(int)pt.x, (int)pt.y] == Tile.wall || Map[(int)pt.x + 1, (int)pt.y] == Tile.wall || Map[(int)pt.x, (int)pt.y - 1] == Tile.wall,
            _ => Map[(int)pt.x, (int)pt.y] == Tile.wall
        };
    }

    //指定の座標にキャラクターがいたらそのゲームオブジェクトを返す
    //いなかったらnullを返す(newPosは中心を原点にした座標)
    public GameObject GetExistActor(Vector3 newPos) {
        if (newPos.x == playerMovement.newPos.x && newPos.y == playerMovement.newPos.y)
            return playerMovement.gameObject;
        foreach (ActorMovement enemyMovement in enemyParent.GetComponentsInChildren<ActorMovement>()) {
            if (newPos.x == enemyMovement.newPos.x && newPos.y == enemyMovement.newPos.y)
                return enemyMovement.gameObject;
        }
        return null;
    }

    #region Save&Load
    public object SaveState() {
        List<Vector3> position = new List<Vector3>();
        List<string> name = new List<string>();
        foreach (Transform t in transform.GetGrandchild()) {
            position.Add(t.position);
            name.Add(t.name);
        }
        return new SaveData(position, name, Map);
    }

    public void LoadState(object state) {
        SaveData saveData = (SaveData)state;
        Map = saveData.Map;
        List<SavableEntity> savables = new List<SavableEntity>();
        for (int i = 0; i < saveData.position.Count; i++) {
            GameObject obj = allPrefab.Find(v => v.name == saveData.name[i].ReplaceAll(@"\(Clone\)|[0-9]+", ""));
            Transform parent = obj.CompareTag("Floor") ? floorParent :
                obj.CompareTag("Aisle") ? aisleParent :
                obj.CompareTag("Wall") ? wallParent :
                obj.CompareTag("Player") ? playerParent :
                obj.CompareTag("Goal") ? goalParent :
                obj.CompareTag("Treasure") ? treasureParent :
                obj.CompareTag("Enemy") ? enemyParent :
                obj.CompareTag("Item") ? itemParent :
                waypointParent;
            obj = Instantiate(obj, parent);
            obj.transform.position = saveData.position[i];
            obj.name = saveData.name[i];
            if(obj.CompareTag("Player"))
                playerMovement = obj.GetComponent<ActorMovement>();
            if (obj.CompareTag("Enemy"))
                savables.Add(obj.GetComponent<SavableEntity>());
        }
        //敵ステータスをロード
        SaveLoadSystem.Instance.MultiLoad(savables);
        isFinish = true;
    }

    [System.Serializable]
    private struct SaveData {
        public List<Vector3> position;
        public List<string> name;
        public Tile[,] Map;

        public SaveData(List<Vector3> position, List<string> name, Tile[,] Map) {
            this.position = position;
            this.name = name;
            this.Map = Map;
        }
    }
    #endregion
}
public class RoomInfo
{
    public int Top = 0;     //部屋の上側
    public int Left = 0;    //部屋の左側
    public int Bottom = 0;  //部屋の下側
    public int Right = 0;   //部屋の右側
    public int area = 0;//部屋の面積
    public int parentRoom = 0;  //親の部屋ID
    public List<int> childRoom = new List<int>();    //子の部屋ID
    public List<bool> isSplitX = new List<bool>();   //子の部屋の分割座標軸　true...X軸　false...Y軸
    public List<int> childSplitPos = new List<int>(); //子の部屋の分割元座標　※Topなどの部屋の情報は変更するので変更する前の座標を保持するため
}