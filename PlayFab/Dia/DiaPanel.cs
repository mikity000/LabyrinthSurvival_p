using PlayFab;
using PlayFab.ClientModels;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// DiaPanelにアタッチ
/// </summary>
public class DiaPanel : MonoBehaviour
{
    void Start()
    {        
        DiaPack[] panels = GetComponentsInChildren<DiaPack>();
        foreach (DiaPack panel in panels)
        {
            CatalogItem catalogItem = PlayFabCtrl.instance.catalog.Find(v => v.ItemId == panel.diaId.ToString());
            panel.displayName.text = catalogItem.DisplayName;
            panel.description.text = catalogItem.Description;
            panel.buttonText.text = $"￥{catalogItem.VirtualCurrencyPrices["RM"]}";
        }
    }
}
