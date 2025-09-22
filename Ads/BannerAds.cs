using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Advertisements;

public class BannerAds : MonoBehaviour
{
    string _androidAdUnitId = "Banner_Android";
    string _iOSAdUnitId = "Banner_iOS";
    public static string _adUnitId = null; // サポートされていないプラットフォームではnull

    void Awake()
    {
        _adUnitId = Application.platform == RuntimePlatform.IPhonePlayer ? _iOSAdUnitId : _androidAdUnitId;
    }

    public void LoadAd() {
        Advertisement.Banner.SetPosition(BannerPosition.BOTTOM_CENTER);
        Advertisement.Banner.Load(_adUnitId, new BannerLoadOptions {
        loadCallback = OnBannerLoaded,
        errorCallback = OnBannerError
        });
    }

    private void OnBannerLoaded() {
        Advertisement.Banner.Show(_adUnitId);
    }

    private void OnBannerError(string message) {
        Debug.Log($"バナー広告エラー : {message}");
    }
}
