using PlayFab.ClientModels;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// RankPanelプレファブにアタッチ
/// 順位、ユーザ名、階層を書き込む
/// </summary>
public class RankingPanel : MonoBehaviour
{
    [SerializeField] private Text positionText;
    [SerializeField] private Text displayNameText;
    [SerializeField] private Text statValueText;

    private PlayerLeaderboardEntry rank;
    public PlayerLeaderboardEntry Rank
    {
        set
        {
            rank = value;
            positionText.text = $"{rank.Position + 1}位";
            displayNameText.text = rank.DisplayName;
            statValueText.text = rank.StatValue.ToString();
        }
    }
}
