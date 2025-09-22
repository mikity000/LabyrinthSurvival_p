using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// MiniMapオブジェクトにアタッチ
/// 画面の右上にマップを表示
/// </summary>
public class MiniMap : MonoBehaviour, ISavable {

    private int goalIndex;
    [SerializeField] private GameObject wallImgPrefab;
    [SerializeField] private GameObject floorImgPrefab;
    [SerializeField] private GameObject aisleImgPrefab;
    [SerializeField] private GameObject itemImgPrefab;
    [SerializeField] private GameObject goalImgPrefab;
    [SerializeField] private GameObject treasureImgPrefab;
    [SerializeField] private GameObject playerImgPrefab;
    [SerializeField] private GameObject enemyImgPrefab;
    [SerializeField] private List<GameObject> allPrefab;
    private List<Transform> childs = new List<Transform>(); //GameManagerの子オブジェクトList
    private List<RectTransform> movingImgs = new List<RectTransform>(); //マップ上で動く画像のList(プレイヤー、エネミー、アイテム)
    private List<RectTransform> inactFloorImgs = new List<RectTransform>(); //部屋内の床List
    private GameObject goalImg;
    private GameObject treasureImg;
    private GameObject[,] mapImgs; //左下頂点を原点とした2次元配列[x座標, y座標]
    Vector2 gridSize;
    [SerializeField] private Transform parent;

    void Start()
    {
        //セーブデータに既にマップが保存されていればreturn
        if (SaveLoadSystem.Instance.HasKey(name))
            return;

        SetSize(); //ミニマップサイズ設定

        //GameManagerの子オブジェクトでMapに表示する必要のあるオブジェクトを格納
        foreach (Transform child in parent.GetGrandchild())
            if (!child.CompareTag("Waypoint"))
                childs.Add(child);

        goalIndex = childs.FindLastIndex(v => v.CompareTag("Goal")) - 1; //ゴールが何番目の子かを取得
        childs = childs.OrderBy(v => v.position.y).ToList(); //(-20, -20),(-19, -20),(-18, -20)のようにソート

        //マップ画像の2次元配列作成
        foreach (Transform child in childs)
        {
            if (child.CompareTag("Enemy") || child.CompareTag("Player") || child.CompareTag("Goal") || child.CompareTag("Treasure") || child.CompareTag("Item") || child.CompareTag("Diamond"))
                continue;

            int x = (int)child.position.x + (int)gridSize.x / 2;
            int y = (int)child.position.y + (int)gridSize.y / 2;
            if (child.CompareTag("Wall"))
                mapImgs[x, y] = CreateWallOnMap(child);
            else if (child.CompareTag("Floor"))
                mapImgs[x, y] = CreateFloorOnMap(child);
            else if (child.CompareTag("Aisle"))
                mapImgs[x, y] = CreateAisleOnMap(child);
        }
        childs.RemoveAll(v => v.CompareTag("Wall") || v.CompareTag("Floor") || v.CompareTag("Aisle"));

        //アイテムをプレイヤーや敵より上にしたいからこのタイミングでゴールを生成
        CreateGoalOnMap();

        //マップ上で動かしたり消したりするオブジェクトを生成
        foreach (Transform child in childs)
        {
            if (child.CompareTag("Player"))
                CreatePlayerOnMap(child);
            else if(child.CompareTag("Enemy"))
                CreateEnemyOnMap(child);
            else if (child.CompareTag("Treasure"))
                CreateTreasureOnMap(child);
            else if (child.CompareTag("Item") || child.CompareTag("Diamond"))
                CreateItemOnMap(child);
        }
    }

    private void SetSize() {
        gridSize = parent.GetComponent<DungeonGenerator>().MapSize;
        mapImgs = new GameObject[(int)gridSize.x, (int)gridSize.y];
        GetComponent<RectTransform>().sizeDelta = gridSize; //RectTransformのWidth、Height変更
    }

    void Update()
    {
        for (int i = 0; i < childs.Count; i++)
        {
            //childsとmovingImgsは同じ順番になっている
            RectTransform movingImg = movingImgs[i];
            Transform child = childs[i];
            //「倒した敵」または「獲得したアイテム」をMap上から消す
            if (!child.gameObject.activeSelf)
            {
                if (child.CompareTag("Enemy"))
                    CreateItemOnMap();
                childs.Remove(child);
                movingImgs.Remove(movingImg);
                Destroy(movingImg.gameObject);
                continue;
            }
            //Map上のプレイヤー、敵、アイテムを動かす
            movingImg.anchoredPosition = child.position;

            //プレイヤーの近くにあるオブジェクトを可視化する
            if (movingImg.CompareTag("Player"))
                ActivateDynamically(movingImg);
        }
    }

    private GameObject CreateWallOnMap(Transform child)
    {
        GameObject wallImg = Instantiate(wallImgPrefab);
        wallImg.transform.SetParent(transform, false);
        wallImg.GetComponent<RectTransform>().anchoredPosition = child.position;
        return wallImg;
    }

    private GameObject CreateFloorOnMap(Transform child)
    {
        GameObject floorImg = Instantiate(floorImgPrefab);
        floorImg.transform.SetParent(transform, false);
        floorImg.name += child.name.TakeOutNumber(); //プレイヤーが入った部屋全体を可視化するためFloor名を部屋番号にする
        floorImg.SetActive(false);
        RectTransform rect = floorImg.GetComponent<RectTransform>();
        rect.anchoredPosition = child.position;
        inactFloorImgs.Add(rect);
        return floorImg;
    }

    private GameObject CreateAisleOnMap(Transform child)
    {
        GameObject aisleImg = Instantiate(aisleImgPrefab);
        aisleImg.transform.SetParent(transform, false);
        aisleImg.GetComponent<RectTransform>().anchoredPosition = child.position;
        aisleImg.SetActive(false);
        return aisleImg;
    }

    private void CreateGoalOnMap()
    {
        Transform goal = childs.Find(v => v.CompareTag("Goal"));
        childs.RemoveAt(childs.IndexOf(goal));
        goalImg = Instantiate(goalImgPrefab);
        goalImg.transform.SetParent(transform, false);
        (goalImg.GetComponent<RectTransform>()).anchoredPosition = goal.position;
        goalImg.SetActive(false);
    }

    private void CreateTreasureOnMap(Transform child)
    {
        treasureImg = Instantiate(treasureImgPrefab);
        treasureImg.transform.SetParent(transform, false);
        RectTransform rect = treasureImg.GetComponent<RectTransform>();
        rect.anchoredPosition = child.position;
        movingImgs.Add(rect);
        treasureImg.SetActive(false);
    }

    private void CreateItemOnMap(Transform child) {
        treasureImg = Instantiate(itemImgPrefab);
        treasureImg.transform.SetParent(transform, false);
        RectTransform rect = treasureImg.GetComponent<RectTransform>();
        rect.anchoredPosition = child.position;
        movingImgs.Add(rect);
        treasureImg.SetActive(false);
    }

    private void CreatePlayerOnMap(Transform child)
    {
        GameObject playerImg = Instantiate(playerImgPrefab);
        playerImg.transform.SetParent(transform, false);
        RectTransform rect = playerImg.GetComponent<RectTransform>();
        rect.anchoredPosition = child.position;
        movingImgs.Add(rect);
    }

    private void CreateEnemyOnMap(Transform child)
    {
        GameObject enemyImg = Instantiate(enemyImgPrefab);
        enemyImg.transform.SetParent(transform, false);
        //敵を別々に動かすためユニークなオブジェクト名を代入、ロード時Prefab名と比較するため敵オブジェクト名は代入しない
        enemyImg.name += child.name.TakeOutNumber();
        RectTransform rect = enemyImg.GetComponent<RectTransform>();
        rect.anchoredPosition = child.position;
        movingImgs.Add(rect);
    }

    private void CreateItemOnMap()
    {
        foreach (Transform child in parent.GetGrandchild())
        {
            if (!child.CompareTag("Item") && !child.CompareTag("Diamond") || childs.Contains(child))
                continue;

            childs.Add(child);
            GameObject itemImg = Instantiate(itemImgPrefab);
            itemImg.transform.SetParent(transform, false);
            itemImg.transform.SetSiblingIndex(goalIndex); //アイテムの上に表示するのはプレイヤーか敵だけにする
            itemImg.name = child.name;
            itemImg.GetComponent<Image>().enabled = Menu.option.isOnMiniMap; //ミニマップ非表示設定だったら非表示にする
            RectTransform rect = itemImg.GetComponent<RectTransform>();
            rect.anchoredPosition = child.position;
            movingImgs.Add(rect);
        }
    }

    private void ActivateDynamically(RectTransform playerImg)
    {
        //プレイヤー画像が今いる座標を取得
        (int x, int y) index = GetIndexFromAnchoredPosition(playerImg.anchoredPosition);
        //プレイヤー画像の上下左右の画像を可視化する
        ActivateUpDownLeftRight(index);

        //プレイヤーが可視化された部屋内にいればreturn
        if (!inactFloorImgs.Contains(mapImgs[index.x, index.y].GetComponent<RectTransform>()))
            return;
        
        //プレイヤーがいる部屋全体を可視化にする
        foreach (RectTransform floorImg in inactFloorImgs)
        {
            //プレイヤーがいる部屋でなければcontinue
            if (!floorImg.name.Equals(mapImgs[index.x, index.y].name))
                continue;
            
            //プレイヤーが入った部屋内にゴールか宝箱があれば可視化する
            if (floorImg.position == goalImg.transform.position)
                goalImg.SetActive(true);
            if (treasureImg != null && floorImg.position == treasureImg.transform.position)
                treasureImg.SetActive(true);

            //隣接する通路も可視化する
            index = GetIndexFromAnchoredPosition(floorImg.anchoredPosition);
            ActivateUpDownLeftRight(index);
        }
        inactFloorImgs.RemoveAll(v => v.gameObject.activeSelf);
    }

    private (int, int) GetIndexFromAnchoredPosition(Vector2 anchoredPos)
    {
        //grid表示範囲におけるアンカー座標位置の割合(0～1)
        float xRatio = Mathf.InverseLerp(-gridSize.x / 2, gridSize.x / 2, anchoredPos.x);
        float yRatio = Mathf.InverseLerp(-gridSize.y / 2, gridSize.y / 2, anchoredPos.y);

        //grid表示された範囲の何ノード目か
        int x = Mathf.RoundToInt(gridSize.x * xRatio);
        int y = Mathf.RoundToInt(gridSize.y * yRatio);

        return (x, y);
    }

    private void ActivateUpDownLeftRight((int x, int y) index)
    {
        mapImgs[index.x, index.y + 1].SetActive(true);
        mapImgs[index.x, index.y - 1].SetActive(true);
        mapImgs[index.x - 1, index.y].SetActive(true);
        mapImgs[index.x + 1, index.y].SetActive(true);
    }

    #region Save&Load
    public object SaveState() {
        List<Vector2> position = new List<Vector2>();
        List<bool> active = new List<bool>();
        List<string> name = new List<string>();
        foreach (RectTransform t in transform) {
            position.Add(t.anchoredPosition);
            active.Add(t.gameObject.activeSelf);
            //アイテムの種類ごとにPrefabを作るのは面倒なので、名前をItemに統一
            name.Add(t.CompareTag("Item") ? $"Item{t.name.TakeOutNumber()}" : t.name);
        }
        return new SaveData(position, active, name, goalIndex);
    }

    public void LoadState(object state) {
        SetSize();
        //動くオブジェクトをchildsに追加
        foreach (Transform child in parent.GetGrandchild())
            if (child.CompareTag("Player") || child.CompareTag("Enemy") || child.CompareTag("Item") || child.CompareTag("Diamond") || child.CompareTag("Treasure"))
                childs.Add(child);

        SaveData saveData = (SaveData)state;
        goalIndex = saveData.goalIndex;
        for (int i = 0; i < saveData.position.Count; i++) {
            //Prefab名と保存した名前が一致したものを代入
            GameObject img = allPrefab.Find(v => v.name == saveData.name[i].ReplaceAll(@"\(Clone\)|[0-9]+", ""));
            img = Instantiate(img);
            RectTransform rect = img.GetComponent<RectTransform>();
            img.transform.SetParent(transform, false);
            rect.anchoredPosition = saveData.position[i];
            img.SetActive(saveData.active[i]);
            img.name = saveData.name[i];
            if (img.CompareTag("Player") || img.CompareTag("Enemy") || img.CompareTag("Item") || img.CompareTag("Treasure"))
                movingImgs.Add(rect);
            if (img.CompareTag("Floor")) inactFloorImgs.Add(rect);
            if (img.CompareTag("Goal")) goalImg = img;
            if (img.CompareTag("Item")) img.transform.SetSiblingIndex(goalIndex);
            if (img.CompareTag("Treasure")) treasureImg = img;
            int x = (int)rect.anchoredPosition.x + (int)gridSize.x / 2;
            int y = (int)rect.anchoredPosition.y + (int)gridSize.y / 2;
            if (img.CompareTag("Wall") || img.CompareTag("Floor") || img.CompareTag("Aisle")) {
                mapImgs[x, y] = img;
            }
        }
        //ステージ上のオブジェクトとマップオブジェクトが対応して動くように順番を同じにする
        childs = childs.OrderBy(v => v.position.y).ThenBy(v => v.position.x).ThenBy(v => v.name.TakeOutNumber()).ToList();
        movingImgs = movingImgs.OrderBy(v => v.position.y).ThenBy(v => v.position.x).ThenBy(v => v.name.TakeOutNumber()).ToList();
    }

    [System.Serializable]
    private struct SaveData {
        public List<Vector2> position;
        public List<bool> active;
        public List<string> name;
        public int goalIndex;

        public SaveData(List<Vector2> position, List<bool> active, List<string> name, int goalIndex) {
            this.position = position;
            this.active = active;
            this.name = name;
            this.goalIndex = goalIndex;
        }
    }
    #endregion
}