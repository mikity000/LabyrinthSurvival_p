using PlayFab;
using PlayFab.ClientModels;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ダイヤパックパネルにアタッチ
/// </summary>
public class DiaPack : MonoBehaviour
{
    public DiaId diaId;
    public enum DiaId
    {
        dia_pack_a,
        dia_pack_b,
        dia_pack_c,
        dia_pack_d
    }
    public Text displayName;
    public Text description;
    public Text buttonText;

    public void PurchaseDia(PlayFabCtrl playFab)
    {
        playFab.PurchaseDia(diaId.ToString());
    }
}
