using PlayFab;
using PlayFab.ClientModels;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ShopPanelにアタッチ
/// </summary>
public class ShopPanel : MonoBehaviour
{
    [SerializeField] private Button longBtn, rangeBtn, absorbBtn;

    void Start()
    {
        Mdse[] panels = GetComponentsInChildren<Mdse>();
        foreach (Mdse panel in panels)
        {
            CatalogItem catalogItem = PlayFabCtrl.instance.catalog.Find(v => v.ItemId == panel.mdseId.ToString());
            panel.displayName.text = catalogItem.DisplayName;
            panel.description.text = catalogItem.Description;
            panel.buttonText.text = $"<sprite=1>{catalogItem.VirtualCurrencyPrices["DM"]}";
            panel.price = (int)catalogItem.VirtualCurrencyPrices["DM"];
        }
    }

    private void OnEnable() {
        foreach (PlayFabCtrl.OwnedItem item in PlayFabCtrl.instance.items) {
            if (item.itemId == "long_distance_attack" && item.rmngUses == 3)
                longBtn.interactable = false;
            if (item.itemId == "range_attack" && item.rmngUses == 3)
                rangeBtn.interactable = false;
            if (item.itemId == "absorb" && item.rmngUses == 3)
                absorbBtn.interactable = false;
        }
    }
}
