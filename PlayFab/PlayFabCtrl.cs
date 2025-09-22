using Cysharp.Threading.Tasks;
using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.Purchasing;
using UnityEngine.UI;

/// <summary>
/// PlayFabにアタッチ
/// </summary>
public class PlayFabCtrl : MonoBehaviour, IStoreListener {
    public static PlayFabCtrl instance;
    [HideInInspector] public List<CatalogItem> catalog;
    private string playFabId;
    [HideInInspector] public bool isLogin = false;
    [HideInInspector] public bool isGetInventry = false;
    [HideInInspector] public int diaCount;
    [HideInInspector] public int typeCount;
    public List<OwnedItem> items = new List<OwnedItem>();
    [SerializeField] private Text diamondText;
    private PlayerParamsController playerParams;

    async void Awake() {
        if (instance == null)
            instance = this;
        InitializationOptions options = new InitializationOptions().SetEnvironmentName("production");
        await UnityServices.InitializeAsync(options);
        await Login();
        if (!PlayerPrefs.HasKey("initLogin")) { //初回起動
            ChageUserName(DateTime.Now.Ticks.ToString("x"));
            PlayerPrefs.SetInt("initLogin", 1);
        }
        SubmitData(GameManager.instance.game.stage);
        await GetCatalog();
        GetInventry();
        GetAdPlacements(AdsInitializer._gameId, "RewardAd");
        PlayerPrefs.Save();
        playerParams = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerParamsController>();
    }

    public async UniTask Login() {
        PlayFabClientAPI.LoginWithCustomID(new LoginWithCustomIDRequest {
            TitleId = PlayFabSettings.TitleId,
            CustomId = GameManager.instance.game.customId,
            CreateAccount = true
        }, result => {
            isLogin = true;
            playFabId = result.PlayFabId;
        }, error => {
            Debug.Log(error.GenerateErrorReport());
            isLogin = true;
        });
        await UniTask.WaitUntil(() => isLogin, cancellationToken: this.GetCancellationTokenOnDestroy());
    }

    public async UniTask GetCatalog() {
        bool isFinish = false;
        PlayFabClientAPI.GetCatalogItems(new GetCatalogItemsRequest() {
            CatalogVersion = "Main",
        }, result => {
            catalog = result.Catalog;
            InitializePurchasing(); //Unity IAPの初期化
            isFinish = true;
        }, error => { isFinish = true; });
        await UniTask.WaitUntil(() => isFinish, cancellationToken: this.GetCancellationTokenOnDestroy());
    }

    public void GetInventry() {
        PlayFabClientAPI.GetUserInventory(new GetUserInventoryRequest()
        , result => {
            diaCount = result.VirtualCurrency["DM"];
            List<ItemInstance> inventory = result.Inventory;
            diamondText.text = diaCount.ToString();
            typeCount = inventory.Count;
            foreach (ItemInstance item in inventory) {
                string json = catalog.Find(v => v.DisplayName == item.DisplayName).CustomData;
                CustomData custData = JsonUtility.FromJson<CustomData>(json);
                items.Add(new OwnedItem(item.ItemId, item.DisplayName, custData.ability, (int)item.RemainingUses, custData.eigval));
            }
            isGetInventry = true;
        }, error => {
            Debug.Log(error.GenerateErrorReport());
            isGetInventry = true;
        });
    }

    public async UniTask AddVirtualCurrency(int amount) {
        bool isFinish = false;
        PlayFabClientAPI.AddUserVirtualCurrency(new AddUserVirtualCurrencyRequest {
            VirtualCurrency = "DM",
            Amount = amount
        }, result => {
            diaCount += amount;
            isFinish = true;
        }, error => { isFinish = true; });
        await UniTask.WaitUntil(() => isFinish, cancellationToken: this.GetCancellationTokenOnDestroy());
        diamondText.text = diaCount.ToString();
    }

    public async UniTask SetUserData(Params parameter) {
        bool isFinish = false;
        PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest{
            Data = new Dictionary<string, string>() {
                {"lv", $"{parameter.lv}"},
                {"hp", $"{parameter.hp}"},
                {"ahp", parameter.ahp == null ? null : $"{parameter.ahp}"},
                {"hpMax", $"{parameter.hpMax}"},
                {"str", $"{parameter.str}"},
                {"def", $"{parameter.def}"},
                {"nextLvUpExp", $"{parameter.nextLvUpExp}"},
                {"hasExp", $"{parameter.hasExp}"},
            }
        }, result => { isFinish = true;
        },error => {
            Debug.Log(error.GenerateErrorReport());
            isFinish = true;
        });
        await UniTask.WaitUntil(() => isFinish, cancellationToken: this.GetCancellationTokenOnDestroy());
    }

    public async UniTask<Dictionary<string, UserDataRecord>> GetUserData() {
        bool isFinish = false;
        Dictionary<string, UserDataRecord> record = new Dictionary<string, UserDataRecord>();
        PlayFabClientAPI.GetUserData(new GetUserDataRequest()
        , result => {
            record = result.Data;
            isFinish = true;
        },error => {
            Log.Add(error.Error.ToString());
            isFinish = true;
        });
        await UniTask.WaitUntil(() => isFinish, cancellationToken: this.GetCancellationTokenOnDestroy());
        return record;
    }

    public async void GrantItem(string itemId) {
        //初めて獲得するアイテムの場合
        if (!items.Any(v => v.itemId == itemId)) 
            SetFirstItemInfo(itemId);

        bool isFinish = false;
        OwnedItem ownedItem = items.Find(v => v.itemId == itemId);
        PlayFabServerAPI.GrantItemsToUser(new PlayFab.ServerModels.GrantItemsToUserRequest {
            ItemIds = new List<string> { itemId },
            PlayFabId = playFabId
        }, result => {
            ownedItem.rmngUses++;
            playerParams.SetAffectedParameter();
            isFinish = true;
        }, error => {
            Debug.Log(error.GenerateErrorReport());
            isFinish = true;
        });
        await UniTask.WaitUntil(() => isFinish, cancellationToken: this.GetCancellationTokenOnDestroy());
        Log.Add($"<color=#69ABDB>{ownedItem.dispName}</color>がLv<color=#69ABDB>{ownedItem.rmngUses}</color>になった！");
    }

    public void AdjustItemInfo(ItemInstance item) {
        diaCount -= (int)item.UnitPrice;
        diamondText.text = diaCount.ToString();
        Regex regex = new Regex("_[0-9]+");
        int quantity = regex.IsMatch(item.ItemId)
                        ? catalog.Find(v => v.ItemId == item.ItemId).Bundle.BundledItems.Count
                        : 1;
        if (quantity == 1 && !items.Any(v => v.itemId == item.ItemId))
            SetFirstItemInfo(item.ItemId);
        else if(quantity > 1 && !items.Any(v => v.itemId == item.BundleContents[0]))
            SetFirstItemInfo(item.BundleContents[0]);
        OwnedItem ownedItem = items.Find(v => v.itemId == regex.Replace(item.ItemId, ""));
        ownedItem.rmngUses += quantity;
    }

    private void SetFirstItemInfo(string itemId) {
        CatalogItem item = catalog.Find(v => v.ItemId == itemId);
        CustomData custData = JsonUtility.FromJson<CustomData>(item.CustomData);
        items.Add(new OwnedItem(item.ItemId, item.DisplayName, custData.ability, 0, custData.eigval));
        typeCount++;
    }

    public void ChageUserName(InputField input) {
        ChageUserName(input.text);
    }

    public void ChageUserName(string userName) {
        PlayFabClientAPI.UpdateUserTitleDisplayName(new UpdateUserTitleDisplayNameRequest {
            DisplayName = userName
        }, result => {
        }, error => {
            switch (error.Error) {
                case PlayFabErrorCode.InvalidParams:
                    MsgBox.Show("ユーザ名は3文字以上25文字以下で設定してください");
                    break;
                case PlayFabErrorCode.NameNotAvailable:
                    MsgBox.Show("そのユーザ名は既に登録されています\n別のユーザ名を設定してください");
                    break;
            }
            Debug.Log(error.GenerateErrorReport());
        });
    }

    #region ランキング
    public void SubmitData(int stage) {
        PlayFabClientAPI.UpdatePlayerStatistics(new UpdatePlayerStatisticsRequest {
            Statistics = new List<StatisticUpdate>(){
                new StatisticUpdate{
                    StatisticName = "HighestStage",
                    Value = stage
                }
            }
        }, result => {
        }, error => { Debug.Log(error.GenerateErrorReport()); });
    }

    public async UniTask<List<PlayerLeaderboardEntry>> GetLeaderboard(int startPos) {
        bool isFinish = false;
        List<PlayerLeaderboardEntry> ranks = new List<PlayerLeaderboardEntry>();
        PlayFabClientAPI.GetLeaderboard(new GetLeaderboardRequest {
            StatisticName = "HighestStage",
            StartPosition = startPos,
            MaxResultsCount = 100
        }, result => { ranks = result.Leaderboard;
            isFinish = true;
        }, error => { isFinish = true; });
        await UniTask.WaitUntil(() => isFinish, cancellationToken: this.GetCancellationTokenOnDestroy());
        return ranks;
    }

    public async UniTask<PlayerLeaderboardEntry> GetLeaderboardAroundPlayer() {
        bool isFinish = false;
        List<PlayerLeaderboardEntry> myRank = new List<PlayerLeaderboardEntry>();
        PlayFabClientAPI.GetLeaderboardAroundPlayer(new GetLeaderboardAroundPlayerRequest {
            StatisticName = "HighestStage",
            MaxResultsCount = 1
        }, result => {
            myRank = result.Leaderboard;
            isFinish = true;
        }, error => { isFinish = true; });
        await UniTask.WaitUntil(() => isFinish, cancellationToken: this.GetCancellationTokenOnDestroy());
        return myRank[0];
    }
    #endregion

    #region 購入処理
    private ConfigurationBuilder builder;
    private static IStoreController storeController;
    private IExtensionProvider extensionProvider;
    public void PurchaseDia(string itemId) {
        if (!IsInitialized)
            throw new Exception("IAP Service is not initialized!");

        storeController.InitiatePurchase(itemId);
    }

    public void InitializePurchasing() {
        if (IsInitialized)
            return;

        builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

        // Register each item from the catalog
        foreach (CatalogItem item in catalog.FindAll(x => x.ItemClass == "ConsumableBundle"))
            builder.AddProduct(item.ItemId, ProductType.Consumable);

        // Trigger IAP service initialization
        UnityPurchasing.Initialize(this, builder);
    }

    public void OnInitializeFailed(InitializationFailureReason error) {
        Debug.Log("OnInitializeFailed InitializationFailureReason:" + error);
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs e) {
        if (!IsInitialized)
            return PurchaseProcessingResult.Complete;


        // Test edge case where product is unknown
        if (e.purchasedProduct == null) {
            Debug.Log("Attempted to process purchase with unknown product. Ignoring");
            return PurchaseProcessingResult.Complete;
        }

        // Test edge case where purchase has no receipt
        if (string.IsNullOrEmpty(e.purchasedProduct.receipt)) {
            Debug.Log("Attempted to process purchase with no receipt: Ignoring");
            return PurchaseProcessingResult.Complete;
        }

        Debug.Log("Processing transaction: " + e.purchasedProduct.transactionID);

        GooglePurchase googleReceipt = GooglePurchase.FromJson(e.purchasedProduct.receipt);

        PlayFabClientAPI.ValidateGooglePlayPurchase(new ValidateGooglePlayPurchaseRequest() {
            CurrencyCode = e.purchasedProduct.metadata.isoCurrencyCode,
            PurchasePrice = (uint)(e.purchasedProduct.metadata.localizedPrice * 100),
            ReceiptJson = googleReceipt.PayloadData.json,
            Signature = googleReceipt.PayloadData.signature
        }, result => {
            string itemId = result.Fulfillments[0].FulfilledItems[0].ItemId;
            int quantity = (int)catalog.Find(v => v.ItemId == itemId).Bundle.BundledVirtualCurrencies["DM"];
            diaCount += quantity;
            diamondText.text = diaCount.ToString();
            Debug.Log("Validation successful!");
        }, error => {
            Debug.Log("Validation failed: " + error.GenerateErrorReport());
        });
        return PurchaseProcessingResult.Complete;
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason) {
        Debug.Log($"OnPurchaseFailed: FAIL. Product: '{product.definition.storeSpecificId}', PurchaseFailureReason: {failureReason}");
    }

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions) {
        storeController = controller;
        extensionProvider = extensions;
    }

    public bool IsInitialized {
        get => storeController != null && extensionProvider != null && catalog != null;
    }
    #endregion

    #region リワード広告
    private string placementId;
    private string rewardId;
    //Ad Placements取得
    public void GetAdPlacements(string gameId, string placementName) {
        PlayFabClientAPI.GetAdPlacements(new GetAdPlacementsRequest { AppId = gameId
        }, result => {
            AdPlacementDetails placement = result.AdPlacements.Find(x => x.PlacementName == placementName);
            placementId = placement.PlacementId;
            rewardId = placement.RewardId;
        }, error => {
            Debug.Log(error.GenerateErrorReport());
        });
    }

    //広告視聴状態を送信
    public void ReportAdActivity(AdActivity activity) {
        PlayFabClientAPI.ReportAdActivity(new ReportAdActivityRequest { 
            PlacementId = placementId,
            RewardId = rewardId,
            Activity = activity
        }, result => {
            if (activity == AdActivity.End)
                RewardAdActivity();
        }, error => { Debug.Log(error.GenerateErrorReport()); });
    }

    //報酬を授与
    public void RewardAdActivity() {
        PlayFabClientAPI.RewardAdActivity(new RewardAdActivityRequest {
            PlacementId = placementId,
            RewardId = rewardId
        }, result => {
            int getCount = result.RewardResults.GrantedVirtualCurrencies["DM"];
            diaCount += getCount;
            diamondText.text = diaCount.ToString();
            MsgBox.Show($"ダイヤを{getCount}個獲得しました");
            GetAdPlacements(AdsInitializer._gameId, "RewardAd");
        }, error => { Debug.Log(error.GenerateErrorReport()); });
    }
    #endregion

    public class OwnedItem {
        public string itemId;
        public string dispName; //アイテム説明
        public string ability; //効果説明
        public int rmngUses; //個数
        public float eigval; //固有値

        public OwnedItem(string itemId, string dispName, string ability, int rmngUses, float eigval) {
            this.itemId = itemId;
            this.dispName = dispName;
            this.ability = ability;
            this.rmngUses = rmngUses;
            this.eigval = eigval;
        }
    }
    public class CustomData {
        public string ability;
        public float eigval;
    }
}

public class JsonData {
    public string orderId;
    public string packageName;
    public string productId;
    public long purchaseTime;
    public int purchaseState;
    public string purchaseToken;
}

public class PayloadData {
    public JsonData JsonData;

    // JSON Fields, ! Case-sensitive
    public string signature;
    public string json;

    public static PayloadData FromJson(string json) {
        var payload = JsonUtility.FromJson<PayloadData>(json);
        payload.JsonData = JsonUtility.FromJson<JsonData>(payload.json);
        return payload;
    }
}

public class GooglePurchase {
    public PayloadData PayloadData;

    // JSON Fields, ! Case-sensitive
    public string Store;
    public string TransactionID;
    public string Payload;

    public static GooglePurchase FromJson(string json) {
        var purchase = JsonUtility.FromJson<GooglePurchase>(json);
        purchase.PayloadData = PayloadData.FromJson(purchase.Payload);
        return purchase;
    }
}
