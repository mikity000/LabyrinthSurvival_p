using PlayFab.ClientModels;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Contentオブジェクトに代入
/// </summary>
public class RankingDialog : MonoBehaviour
{
    [SerializeField] private GameObject parent;
    [SerializeField] private Text endText;
    [SerializeField] private Text myPosText;
    [SerializeField] private InputField myDisplayNameInput;
    [SerializeField] private Text myStatValText;
    [SerializeField] private RankingPanel rankPanel;
    [SerializeField] private Button rankBtn;

    //表示順位変更テキストボックス入力完了時の処理
    public void Refresh(InputField input)
    {
        DestroyChildren();
        int startPos = int.Parse(input.text) - 1;
        if (startPos < 0)
        {
            startPos = 0;
            input.text = "1";
        }
        endText.text = $"位から{startPos + 100}位まで表示";
        Open(startPos);
    }

    // ランキングボタン押下時の処理
    public async void Open(int startPos)
    {
        rankBtn.interactable = false;
        parent.SetActive(true);
        await PlayFabCtrl.instance.Login();
        SetMyRankInfo();
        List<PlayerLeaderboardEntry> ranks = await PlayFabCtrl.instance.GetLeaderboard(startPos);

        //ランキングを必要な分だけ複製
        for (int i = 0; i < ranks.Count; i++) {
            //StatValueが同じならPositionも同じにする
            if (i > 0 && ranks[i].StatValue == ranks[i - 1].StatValue)
                ranks[i].Position = ranks[i - 1].Position;
            Instantiate(rankPanel, transform).Rank = ranks[i];
        }
    }

    //自分のランク情報を設定
    private async void SetMyRankInfo()
    {
        PlayerLeaderboardEntry myRank = await PlayFabCtrl.instance.GetLeaderboardAroundPlayer();
        myPosText.text = $"{myRank.Position + 1}位";
        myDisplayNameInput.text = myRank.DisplayName;
        myStatValText.text = myRank.StatValue.ToString();
    }

    //ランキングダイアログを閉じるときの処理
    public void Close(InputField input)
    {
        rankBtn.interactable = true;
        input.text = "1";
        endText.text = "位から100位まで表示";
        DestroyChildren();
        parent.SetActive(false);
    }

    //RefreshまたはClose時に子のランクパネルを破棄
    private void DestroyChildren()
    {
        RankingPanel[] rankPanels = GetComponentsInChildren<RankingPanel>();
        foreach (RankingPanel rankPanel in rankPanels)
            Destroy(rankPanel.gameObject);
    }
}
