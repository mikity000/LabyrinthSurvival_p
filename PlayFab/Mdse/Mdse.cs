using PlayFab;
using PlayFab.ClientModels;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 商品パネルにアタッチ
/// </summary>
public class Mdse : MonoBehaviour
{
    public MdseId mdseId;
    public enum MdseId
    {
        sword,
        sword_5,
        armor,
        armor_5,
        long_distance_attack,
        range_attack,
        absorb
    }
    public Text displayName; //ShopPanelで使う
    public Text description; //ShopPanelで使う
    public TextMeshProUGUI buttonText; //ShopPanelで使う
    [SerializeField] private Button longBtn, rangeBtn, absorbBtn;
    [HideInInspector] public int price; //ShopPanelで使う
    private PlayerParamsController playerParams;
    private bool isOn = false;

    private void Start() {
        playerParams = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerParamsController>();
    }

    public void PurchaseMdse()
    {
        //二重押下禁止
        if (isOn)
            return;
        isOn = true;

        PlayFabClientAPI.PurchaseItem(new PurchaseItemRequest()
        {
            CatalogVersion = "Main",
            StoreId = "diamond_store",
            ItemId = mdseId.ToString(),
            VirtualCurrency = "DM",
            Price = price
        }, result => {
            ItemInstance item = result.Items[0];
            item.UnitPrice = (uint)Mathf.Max(price, item.UnitPrice); //原因不明の0のため大きい方を代入
            CheckSkillLv(item); //購入したアイテムがスキルならLvチェック
            PlayFabCtrl.instance.AdjustItemInfo(item);
            MsgBox.Show($"{item.DisplayName}を獲得しました");
            playerParams.SetAffectedParameter();
            isOn = false;
        }, error => {
            if (error.Error == PlayFabErrorCode.InsufficientFunds)
                MsgBox.Show("ダイヤが不足しています");
            Debug.Log(error.GenerateErrorReport());
            isOn = false;
        });
    }

    //スキルがLvMaxになったら非アクティブにする
    private void CheckSkillLv(ItemInstance item) {
        if (item.ItemId == "long_distance_attack" && item.RemainingUses >= 3)
            longBtn.interactable = false;
        if (item.ItemId == "range_attack" && item.RemainingUses >= 3)
            rangeBtn.interactable = false;
        if (item.ItemId == "absorb" && item.RemainingUses >= 3)
            absorbBtn.interactable = false;
    }
}
