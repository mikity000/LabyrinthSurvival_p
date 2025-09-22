using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Advertisements;
using PlayFab.ClientModels;
using System;
using DG.Tweening;

/// <summary>
/// AdManagerオブジェクトにアタッチ
/// </summary>
public class RewardAds : MonoBehaviour, IUnityAdsLoadListener, IUnityAdsShowListener {
    [SerializeField] Button rewardBtn;
    [SerializeField] Button viewBtn;
    string androidAdUnitId = "Rewarded_Android";
    string iOSAdUnitId = "Rewarded_iOS";
    string adUnitId = null; // サポートされていないプラットフォームではnull

    void Awake() {
        adUnitId = Application.platform == RuntimePlatform.IPhonePlayer ? iOSAdUnitId : androidAdUnitId;
        //広告が表示可能になるまでボタンを無効
        rewardBtn.interactable = false;
        BtnMotion();
    }

    // 広告ユニットに広告をロード
    public void LoadAd() {
        // 重要! 広告のロードは初期化後にしてください(初期化は別のスクリプトで実行している)
        Advertisement.Load(adUnitId, this);
    }

    // 広告が正常にロードされたら、ボタンにリスナーを追加して有効にする
    public void OnUnityAdsAdLoaded(string adUnitId) {
        if (adUnitId.Equals(this.adUnitId)) {
            viewBtn.onClick.AddListener(ShowAd);
            rewardBtn.interactable = true;
        }
    }

    //広告を表示する
    public void ShowAd() {
        rewardBtn.interactable = false;
        Advertisement.Show(adUnitId, this);
    }

    //広告表示が終わったら呼ばれ、状態をPlayFabに送信
    public void OnUnityAdsShowComplete(string adUnitId, UnityAdsShowCompletionState showCompletionState) {
        switch (showCompletionState) {
            case UnityAdsShowCompletionState.COMPLETED:
                PlayFabCtrl.instance.ReportAdActivity(AdActivity.End);
                viewBtn.onClick.RemoveAllListeners();
                Advertisement.Load(this.adUnitId, this); //別の広告をロード
                rewardBtn.gameObject.SetActive(false); //再表示時間がくるまでボタンを非表示にする
                break;
            case UnityAdsShowCompletionState.SKIPPED:
                PlayFabCtrl.instance.ReportAdActivity(AdActivity.Closed);
                break;
            case UnityAdsShowCompletionState.UNKNOWN:
                PlayFabCtrl.instance.ReportAdActivity(AdActivity.Closed);
                break;
        }
        //バナー広告を再度表示
        Advertisement.Banner.Show(BannerAds._adUnitId);
    }

    // 広告のロードに失敗
    public void OnUnityAdsFailedToLoad(string adUnitId, UnityAdsLoadError error, string message) {
        Debug.Log($"広告Unitロード失敗 {adUnitId}: {error} - {message}");
    }

    // 広告の表示に失敗
    public void OnUnityAdsShowFailure(string adUnitId, UnityAdsShowError error, string message) {
        Debug.Log($"広告Unit表示失敗 {adUnitId}: {error} - {message}");
    }

    //広告表示開始
    public void OnUnityAdsShowStart(string adUnitId) {
        PlayFabCtrl.instance.ReportAdActivity(AdActivity.Start);
        //リワード広告が表示されたらバナー広告を非表示
        Advertisement.Banner.Hide();
    }
    public void OnUnityAdsShowClick(string adUnitId) { }

    #region ボタン移動
    [SerializeField] int dispSpan;
    float totalTime;
    Sequence seq;
    private void Update() {
        if (rewardBtn.IsActive())
            return;
        totalTime += Time.deltaTime;
        //広告再表示時間がきたらボタンを可視化する
        if (totalTime > dispSpan) {
            rewardBtn.gameObject.SetActive(true);
            seq.Restart();
            totalTime = 0;
        }
    }

    private void BtnMotion() {
        seq = DOTween.Sequence();
        seq.Append(rewardBtn.transform.DOLocalMoveY(10, 0.4f).SetRelative())
            .Append(rewardBtn.transform.DORotate(new Vector3(0, 0, 30), 0.2f))
            .Append(rewardBtn.transform.DORotate(new Vector3(0, 0, -30), 0.2f))
            .Append(rewardBtn.transform.DORotate(Vector3.zero, 0.2f))
            .Append(rewardBtn.transform.DOLocalMoveY(-10, 0.4f).SetRelative())
            .SetLoops(-1, LoopType.Restart).SetLink(rewardBtn.gameObject)
            .AppendInterval(3);
    }
    #endregion
}