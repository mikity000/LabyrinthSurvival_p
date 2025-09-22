using UnityEngine;

/// <summary>
/// Contentオブジェクトにアタッチ
/// アイテム一覧を管理するクラス
/// </summary>
public class ItemsDialog : MonoBehaviour
{
    [SerializeField] private int panelNumber;
    [SerializeField] private ItemPanel itemPanel;
    private ItemPanel[] itemPanels;
    [SerializeField] private GameObject parent;

    private void Start()
    {
        // 初期状態は非表示
        parent.SetActive(false);
        
        // アイテム欄を必要な分だけ複製
        for (int i = 0; i < panelNumber - 1; i++)
            Instantiate(itemPanel, transform);

        // 子要素のItemPanelコンポーネントを一括取得、保持しておく
        itemPanels = GetComponentsInChildren<ItemPanel>();
    }

    // アイテムボタン押下時の処理
    public void Open()
    {
        // アイテム欄の表示/非表示を切り替える
        parent.SetActive(!parent.activeSelf);
        if (!parent.activeSelf)
            return;

        // panelNumberの数だけItemPanelに情報を代入
        for (int i = 0; i < panelNumber; i++)
        {
            // 獲得済みアイテムの情報だけ代入
            itemPanels[i].OwnedItem = PlayFabCtrl.instance.typeCount > i
                ? PlayFabCtrl.instance.items[i]
                : null; //未所持アイテムは黒背景
        }
    }
}