using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Unity.Burst.Intrinsics.X86;

/// <summary>
/// Canvasオブジェクトにアタッチ
/// UIボタンを処理するクラス
/// </summary>
public class Menu : MonoBehaviour {
    //ボタン
    [SerializeField] private ItemsDialog itemsDialog;
    [SerializeField] private Button itemsButton;
    [SerializeField] private DrawGrid drawGrid;
    [SerializeField] private Transform status;
    [SerializeField] private Transform log;
    [SerializeField] private Button switchButton;
    [SerializeField] private Text switchButtonText;

    //トグル
    [SerializeField] private Toggle miniMapToggle;
    [SerializeField] private GameObject miniMap;
    private Image[] mapImgs;
    [SerializeField] private Toggle gridToggle;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private Toggle bgmToggle;
    [SerializeField] private Toggle seToggle;

    //スライダー
    [SerializeField] private Slider logSlider;

    //設定保存用インスタンス
    public static OptionData option;

    private void Start() {
        //トグル
        option = PlayerPrefs.HasKey("optionData")
                ? JsonUtility.FromJson<OptionData>(PlayerPrefs.GetString("optionData"))
                : new OptionData() { logSize = (int)logSlider.value, isOnBGM = bgmToggle.isOn, isOnSE = seToggle.isOn, isOnMiniMap = miniMapToggle.isOn, isOnGrid = gridToggle.isOn };
        logSlider.value = option.logSize;
        Log.EnlargeText(option.logSize);
        bgmToggle.isOn = option.isOnBGM;
        seToggle.isOn = option.isOnSE;
        miniMapToggle.isOn = option.isOnMiniMap;
        gridToggle.isOn = option.isOnGrid;
    }

    // アイテム画面の開閉
    public void OpenItemsDialog() {
        itemsDialog.Open();
    }

    //ログとステータス画面の切り替え
    public void SwitchStatusAndLogToEachOther() {
        if (switchButtonText.text.Equals("ログ")) {
            log.SetSiblingIndex(status.GetSiblingIndex());
            switchButtonText.text = "ステータス";
        } else {
            status.SetSiblingIndex(log.GetSiblingIndex());
            switchButtonText.text = "ログ";
        }
    }

    public void AdjustLogSize() {
        option.logSize = (int)logSlider.value;
        Log.EnlargeText(option.logSize);
        Save();
    }

    public void BGM() {
        option.isOnBGM = bgmToggle.isOn;
        if (option.isOnBGM) audioSource.Play();
        else audioSource.Stop();
        Save();
    }

    public void SE() {
        option.isOnSE = seToggle.isOn;
        Save();
    }

    //ミニマップを表示
    public void DisplayMiniMap() {
        mapImgs = miniMap.GetComponentsInChildren<Image>(true);
        option.isOnMiniMap = miniMapToggle.isOn;
        foreach (Image mapImg in mapImgs)
            mapImg.enabled = option.isOnMiniMap;
        Save();
    }

    //Gridを表示
    public void DrawGrid() {
        option.isOnGrid = gridToggle.isOn;
        drawGrid.Draw(option.isOnGrid);
        Save();
    }

    private void Save() {
        PlayerPrefs.SetString("optionData", JsonUtility.ToJson(option));
        PlayerPrefs.Save();
    }
}