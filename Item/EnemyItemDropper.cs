using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Enemyプレファブにアタッチ
/// 敵のアイテムドロップを管理するクラス
/// </summary>
public class EnemyItemDropper : MonoBehaviour
{
    [SerializeField] [Range(0, 1)] private float dropRate; // アイテム出現確率
    [SerializeField] private Map<Item>[] map; //アイテムとドロップ率のマッピング
    [SerializeField] private int number; // アイテム出現個数
    private static int id;
    private Transform itemParent;
    private Params playerParam;

    private void Start()
    {
        id = 0;
        itemParent = GameObject.Find("ItemParent").transform;
        playerParam = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerParamsController>().parameter;
    }

    // 確率でアイテムドロップ
    public void DropItem()
    {
        //階数が上がるごとにドロップ率上昇
        dropRate += GameManager.instance.game.stage / 1000f;
        if (dropRate <= Random.value)
            return;

        //ドロップ率上昇アイテムによってドロップ個数が増える
        number = number.RatioPlusToInt(playerParam.dropUp);
        // 指定個数分のアイテムを出現
        for (int i = 0; i < number; i++)
        {
            Item item = Instantiate(WeightedLottery.Draw(map), transform.position, Quaternion.identity);
            item.transform.SetParent(itemParent);
            item.name += id++;
            item.drop(i);
        }
    }
}
