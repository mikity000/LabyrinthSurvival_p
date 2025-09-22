using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AdsInitializer : MonoBehaviour, IUnityAdsInitializationListener {
    public static AdsInitializer instance;
    string _androidGameId = "";
    string _iOSGameId = "";
    [SerializeField] bool _testMode;
    public static string _gameId;
    [SerializeField] RewardAds rewardAds;
    [SerializeField] BannerAds bannerAds;

    void Start() {
        if (instance != null) {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeAds();
    }

    public void InitializeAds() {
        _gameId = Application.platform == RuntimePlatform.IPhonePlayer ? _iOSGameId : _androidGameId;
        Advertisement.Initialize(_gameId, _testMode, this);
    }

    public void OnInitializationComplete() {
        bannerAds.LoadAd();
        rewardAds.LoadAd();
    }

    public void OnInitializationFailed(UnityAdsInitializationError error, string message) {
        Debug.Log($"UnityAdsèâä˙âªé∏îs : {error} - {message}");
    }
}
