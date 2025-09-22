using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// GameManagerオブジェクトにアタッチ
/// 現在の階、ゲームオーバーを管理するクラス
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public Text diamondText;
    public Text stageText;
    public Text stageImageText;
    public GameObject stageImage;
    public GameData game;
    [SerializeField] private SequenceManager sm;
    [SerializeField] private GameObject blackPanel;

    private async void Awake()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
            SystemBarController.Show();
#endif

        if (instance == null)
            instance = this;
        //PlayerPrefs.DeleteKey("gameData");
        game = PlayerPrefs.HasKey("gameData")
                ? JsonUtility.FromJson<GameData>(PlayerPrefs.GetString("gameData"))
                : new GameData() { stage = 1, customId = (ShortGuid)Guid.NewGuid() };
        PlayerPrefs.SetString("gameData", JsonUtility.ToJson(game));
        await UniTask.WaitUntil(() => DungeonGenerator.isFinish);
        
        InitGame();
    }

    //シーン遷移後のUI表示を更新する
    private async void InitGame()
    {
        sm.enabled = false;
        diamondText.text = PlayFabCtrl.instance.diaCount.ToString();
        stageText.text = $"{game.stage}階";
        stageImageText.text = stageText.text;
        stageImage.SetActive(true);
        await UniTask.Delay(2000);
        stageImage.SetActive(false);
        sm.enabled = true;
    }

    private void Update() {
        blackPanel.SetActive(Application.internetReachability == NetworkReachability.NotReachable);
    }

    public async void GameOver()
    {
        SoundManager.instance.PauseBGM();
        SaveLoadSystem.Instance.DeleteFile();
        game.stage = 1;
        PlayerPrefs.SetString("gameData", JsonUtility.ToJson(game));
        PlayerPrefs.Save();
        stageImageText.text = "GameOver";
        stageImage.SetActive(true);
        SoundManager.instance.PlaySound(SoundManager.instance.gameover);
        await UniTask.Delay(3000);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
