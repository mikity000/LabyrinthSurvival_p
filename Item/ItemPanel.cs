using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ItemPanelオブジェクトにアタッチ
/// アイテムの画像、文字を表示するクラス
/// </summary>
public class ItemPanel : MonoBehaviour
{
    [SerializeField] private ItemTypeSpriteMap[] itemSprites; // 各アイテム用の画像を指定するフィールド
    [SerializeField] private Image image;
    [SerializeField] private Text dispName;
    [SerializeField] private Text ability; //能力

    private PlayFabCtrl.OwnedItem ownedItem;
    public PlayFabCtrl.OwnedItem OwnedItem
    {
        set
        {
            ownedItem = value;

            // アイテムが割り当てられたかどうかでアイテム画像や所持個数の表示を切り替える
            bool isEmpty = ownedItem == null;
            image.gameObject.SetActive(!isEmpty);
            dispName.gameObject.SetActive(!isEmpty);
            ability.gameObject.SetActive(!isEmpty);
            if (!isEmpty)
            {
                image.sprite = itemSprites.First(x => x.type.ToString() == ownedItem.itemId).sprite;
                dispName.text = $"{ownedItem.dispName} Lv{ownedItem.rmngUses}";
                float eigval = ownedItem.eigval * ownedItem.rmngUses;
                ability.text = ownedItem.dispName switch {
                    "遠距離攻撃" => ownedItem.ability.Replace("X", $"{eigval + 1}"),
                    "範囲攻撃" when ownedItem.rmngUses == 1 => ownedItem.ability.Replace("X", $"前{eigval + 1}"),
                    "範囲攻撃" when ownedItem.rmngUses == 2 => ownedItem.ability.Replace("X", $"前左右{eigval + 1}"),
                    "範囲攻撃" when ownedItem.rmngUses == 3 => ownedItem.ability.Replace("X", $"前後左右{eigval + 1}"),
                    "吸収" => ownedItem.ability.Replace("X", $"{eigval}"),
                    _ => $"{ownedItem.ability}\n({Mathf.RoundToInt(eigval * Mathf.Pow(1.01f, ownedItem.rmngUses))}%UP)"
                };
            }
        }
    }
    
    // アイテムの種類とSpriteをインスペクタで紐付けられるようにするためのクラス
    [Serializable]
    public class ItemTypeSpriteMap
    {
        public Item.ItemType type;
        public Sprite sprite;
    }
}