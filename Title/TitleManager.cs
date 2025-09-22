//using CSharpVitamins;
using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleManager : MonoBehaviour
{
    [SerializeField] private GameData game;
    [SerializeField] private InputField email;
    [SerializeField] private InputField password;
    [SerializeField] private Text errorText;
    [SerializeField] private GameObject msgPanel;
    [SerializeField] private Text msgText;
    [SerializeField] private GameObject bkPanel;
    [SerializeField] private PlayerParamsController player;
    [SerializeField] private Button eyeBtn;
    [SerializeField] private Sprite closeEye;
    [SerializeField] private Sprite openEye;
    [SerializeField] private GameObject blackPanel;

    private string savePath => $"{Application.persistentDataPath}/save.txt";

    void Start()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
            SystemBarController.Show();
#endif

        game = PlayerPrefs.HasKey("gameData")
             ? JsonUtility.FromJson<GameData>(PlayerPrefs.GetString("gameData"))
             : new GameData() { stage = 1, customId = (ShortGuid)Guid.NewGuid()};
        PlayerPrefs.SetString("gameData", JsonUtility.ToJson(game));
        PlayerPrefs.Save();
        Login();
    }

    private void Login() {
        PlayFabClientAPI.LoginWithCustomID(new LoginWithCustomIDRequest {
            TitleId = PlayFabSettings.TitleId,
            CustomId = game.customId,
            CreateAccount = false
        }, result => { 
        }, error => { Debug.Log("Error : " + error.Error.ToString()); });
    }

    private void Update() {
        blackPanel.SetActive(Application.internetReachability == NetworkReachability.NotReachable);
    }

    public void SetEmailPassword() {
        if (IsNotFillInputField())
            return;
        if (!PlayFabClientAPI.IsClientLoggedIn()) {
            errorText.text = "アカウントがまだ作成されていません";
            return;
        }

        PlayFabClientAPI.AddUsernamePassword(new AddUsernamePasswordRequest() {
            Username = game.customId,
            Email = email.text,
            Password = password.text
        }, result => {
            bkPanel.SetActive(false);
            msgPanel.SetActive(true);
            msgText.text = "登録に成功しました";
        }, error => {
            errorText.text = error.Error switch {
                PlayFabErrorCode.InvalidParams => "有効なメールアドレスと6～100文字以内のパスワードを入力してください",
                PlayFabErrorCode.EmailAddressNotAvailable => "このメールアドレスは既に使用されています",
                PlayFabErrorCode.InvalidEmailAddress => "このメールアドレスは使用できません",
                PlayFabErrorCode.InvalidPassword => "このパスワードは無効です",
                PlayFabErrorCode.AccountAlreadyLinked => "このアカウントは既に登録されています",
                _ => $"登録に失敗しました(エラーコード : {error.Error})",
            };
            Debug.Log("Error : " + error.GenerateErrorReport());
        });
    }

    public void LoginEmailPassword() {
        if (IsNotFillInputField())
            return;

        PlayFabClientAPI.LoginWithEmailAddress(new LoginWithEmailAddressRequest() {
            Email = email.text,
            Password = password.text,
            InfoRequestParameters = new GetPlayerCombinedInfoRequestParams {
                GetUserAccountInfo = true
            }
        }, result => {
            bkPanel.SetActive(false);
            //引継いだ場合、parameter、gameデータ、セーブファイルは初期化する
            player.InitializeParameter();
            SetUserData(player.initParameter);
            game.customId = result.InfoResultPayload.AccountInfo.CustomIdInfo.CustomId;
            game.stage = 1;
            PlayerPrefs.SetString("gameData", JsonUtility.ToJson(game));
            PlayerPrefs.SetInt("initLogin", 1);
            PlayerPrefs.Save();
            if (File.Exists(savePath))
                File.Delete(savePath);
            msgPanel.SetActive(true);
            msgText.text = "引継ぎに成功しました";
        }, error => {
            errorText.text = error.Error is PlayFabErrorCode.InvalidParams or PlayFabErrorCode.InvalidEmailOrPassword or PlayFabErrorCode.AccountNotFound
                                ? "メールアドレスかパスワードが正しくありません" : $"引継ぎに失敗しました(エラーコード : {error.Error})";
            Debug.Log("Error : " + error.Error.ToString());
        });
    }

    private bool IsNotFillInputField() {
        if (string.IsNullOrEmpty(email.text) || string.IsNullOrEmpty(password.text)) {
            errorText.text = "メールアドレスとパスワードを入力してください";
            return true;
        }
        return false;
    }

    public void SetUserData(Params parameter) {
        PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest {
            Data = new Dictionary<string, string>() {
                {"lv", $"{parameter.lv}"},
                {"hp", $"{parameter.hp}"},
                {"hpMax", $"{parameter.hpMax}"},
                {"str", $"{parameter.str}"},
                {"def", $"{parameter.def}"},
                {"nextLvUpExp", $"{parameter.nextLvUpExp}"},
                {"hasExp", $"{parameter.hasExp}"},
            }
        }, result => { Debug.Log("プレイヤーデータ登録成功");
        }, error => { Debug.Log(error.Error.ToString()); });
    }

    public void SwitchEye() {
        if (eyeBtn.image.sprite == closeEye) {
            eyeBtn.image.sprite = openEye;
            password.contentType = InputField.ContentType.Standard;
        } else {
            eyeBtn.image.sprite = closeEye;
            password.contentType = InputField.ContentType.Password;
        }
        StartCoroutine(ReloadInputField());
    }

    //全選択状態にしないためにコルーチンで待つ
    private IEnumerator ReloadInputField() {
        password.ActivateInputField();
        yield return null;
        password.MoveTextEnd(false);
    }

    public void OnPolicyBtn() {
        Application.OpenURL("https://mikity-gc.web.app/");
    }

    public void LoadScene() {
        SceneManager.LoadScene(1);
    }
}
