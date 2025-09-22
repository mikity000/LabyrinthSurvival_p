using Cysharp.Threading.Tasks;
using DG.Tweening;
using PlayFab.ClientModels;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Playerプレファブにアタッチ
/// プレイヤーのステータスを制御
/// </summary>
public class PlayerParamsController : ActorParamsController {
    public Params Parameter; //インスペクターで設定
    public override Params parameter {
        get => Parameter;
        set => Parameter = value;
    }
    [HideInInspector] public Params initParameter = new Params();
    [HideInInspector] public Params affectedParameter = new Params();
    public float normalRecoveryPer;
    private Image hpGauge;
    private Text hpText, lvText, strText, defText, nextLvText;
    [SerializeField] private GameObject LvUpContainer;
    [SerializeField] private GameObject damageContainer;
    private SequenceManager sm;
    private bool isDead = false;
    private bool isStarting = true;

    private async void Start() {
        InitializeParameter();
        await UniTask.WaitUntil(() => PlayFabCtrl.instance.isGetInventry);
        await SetPlayerParameter();
        hpGauge = GameObject.Find("HPGauge").GetComponent<Image>();
        hpText = GameObject.Find("PlayerHPText").GetComponent<Text>();
        lvText = GameObject.Find("LvText").GetComponent<Text>();
        strText = GameObject.Find("STRText").GetComponent<Text>();
        defText = GameObject.Find("DEFText").GetComponent<Text>();
        nextLvText = GameObject.Find("NextLvText").GetComponent<Text>();
        SetAffectedParameter(true);
        hpGauge.fillAmount = affectedParameter.hp / affectedParameter.hpMax;
        lvText.text = "Lv" + parameter.lv;
        nextLvText.text = $"次のLvまで\n{parameter.nextLvUpExp - parameter.hasExp}";
        sm = GameObject.Find("SequenceManager").GetComponent<SequenceManager>();
        isStarting = false;
    }

    //初期ステータスを値渡し
    public void InitializeParameter() {
        //ゲームオーバー時のため初期ステータスを代入
        initParameter.lv = parameter.lv;
        initParameter.hpMax = parameter.hpMax;
        initParameter.hp = parameter.hp;
        initParameter.str = parameter.str;
        initParameter.def = parameter.def;
        initParameter.hasExp = parameter.hasExp;
        initParameter.nextLvUpExp = parameter.nextLvUpExp;
    }

    private async UniTask SetPlayerParameter() {
        Dictionary<string, UserDataRecord> record = await PlayFabCtrl.instance.GetUserData();
        if (record.Count == 0)
            return;
        parameter.lv = int.Parse(record["lv"].Value);
        parameter.hpMax = int.Parse(record["hpMax"].Value);
        parameter.hp = int.Parse(record["hp"].Value);
        parameter.str = int.Parse(record["str"].Value);
        parameter.def = int.Parse(record["def"].Value);
        parameter.hasExp = int.Parse(record["hasExp"].Value);
        parameter.nextLvUpExp = int.Parse(record["nextLvUpExp"].Value);
        parameter.ahp = record.ContainsKey("ahp") ? int.Parse(record["ahp"].Value) : null;
    }

    public void SetAffectedParameter(bool isStart = false) {
        //アイテムがない時のため強化されていないパラメータを代入
        affectedParameter.str = parameter.str;
        affectedParameter.def = parameter.def;
        affectedParameter.hpMax = parameter.hpMax;
        if (isStart)
            affectedParameter.hp = parameter.ahp ?? parameter.hp;
        //アイテム効果をパラメータに適用
        foreach (PlayFabCtrl.OwnedItem item in PlayFabCtrl.instance.items) {
            if (item.itemId == "sword")
                affectedParameter.str = parameter.str.RatioPlusToInt(Mathf.RoundToInt(item.eigval * item.rmngUses * Mathf.Pow(1.01f, item.rmngUses)));
            if (item.itemId == "armor")
                affectedParameter.def = parameter.def.RatioPlusToInt(Mathf.RoundToInt(item.eigval * item.rmngUses * Mathf.Pow(1.01f, item.rmngUses)));
            if (item.itemId == "apple") {
                affectedParameter.hpMax = parameter.hpMax.RatioPlusToInt(Mathf.RoundToInt(item.eigval * item.rmngUses * Mathf.Pow(1.01f, item.rmngUses)));
                if (isStart)
                    affectedParameter.hp = parameter.ahp ?? parameter.hpMax.RatioPlusToInt(Mathf.RoundToInt(item.eigval * item.rmngUses * Mathf.Pow(1.01f, item.rmngUses)));
            }
            if (item.itemId == "book")
                parameter.expUp = Mathf.RoundToInt(item.eigval * item.rmngUses * Mathf.Pow(1.01f, item.rmngUses));
            if (item.itemId == "chemicals")
                parameter.statusUp = Mathf.RoundToInt(item.eigval * item.rmngUses * Mathf.Pow(1.01f, item.rmngUses));
            if (item.itemId == "coin")
                parameter.dropUp = Mathf.RoundToInt(item.eigval * item.rmngUses * Mathf.Pow(1.01f, item.rmngUses));
            if (item.itemId == "long_distance_attack")
                parameter.longLv = item.rmngUses;
            if (item.itemId == "range_attack")
                parameter.rangeLv = item.rmngUses;
            if (item.itemId == "absorb")
                parameter.absorbLv = item.rmngUses;
        }
        hpText.text = $"HP {affectedParameter.hp}/{affectedParameter.hpMax}";
        strText.text = $"攻撃力 {affectedParameter.str}";
        defText.text = $"防御力 {affectedParameter.def}";
    }

    public override void LevelUp() {
        SoundManager.instance.PlaySound(SoundManager.instance.lvUp);
        while (parameter.hasExp >= parameter.nextLvUpExp) {
            parameter.lv++;
            parameter.hpMax += Random.Range(2, 5).RatioPlusToInt(parameter.statusUp);
            parameter.str += Random.Range(2, 5).RatioPlusToInt(parameter.statusUp);
            parameter.def += Random.Range(2, 5).RatioPlusToInt(parameter.statusUp);
            parameter.hasExp -= parameter.nextLvUpExp;
            parameter.nextLvUpExp = Mathf.RoundToInt(parameter.nextLvUpExp * 1.2f);
        }
        strText.text = $"攻撃力 {parameter.str}";
        defText.text = $"防御力 {parameter.def}";
        lvText.text = "Lv" + parameter.lv;
        PopupLevelUp();
        Log.Add($"<color=#69ABDB>勇者</color>がLv<color=#69ABDB>{parameter.lv}</color>になった！");
        SetAffectedParameter();
    }

    private void PopupLevelUp() {
        GameObject lvUpParent = Instantiate(LvUpContainer, transform);
        lvUpParent.transform.position += Vector3.up;
        Destroy(lvUpParent, 2.5f);
    }

    //ダメージを受ける
    public override void BeAttacked(ActorParamsController enemy) {
        SoundManager.instance.PlaySound(SoundManager.instance.enemyAttack);
        int dmg = CalcDamage(enemy.parameter.str, affectedParameter.def);
        dmg = Mathf.Max(dmg, 1); //最低ダメージを1にする
        affectedParameter.hp -= dmg;
        hpText.text = $"HP {affectedParameter.hp}/{affectedParameter.hpMax}";
        hpGauge.fillAmount = affectedParameter.hp / affectedParameter.hpMax;
        PopupDamage(dmg.ToString());
        Log.Add($"<color=#e82727>{enemy.parameter.name}</color>は<color=#69ABDB>勇者</color>に<color=#e82727>{dmg}</color>のダメージ");
        if (affectedParameter.hp == 0)
            Death();
    }

    public void PopupDamage(string damage) {
        TextMeshPro damageText = base.PopupDamage(damageContainer);
        damageText.color = new Color32(255, 30, 0, 255);
        damageText.SetText(damage);
    }

    //ダメージ計算
    private static int CalcDamage(int str, int def) {
        return Mathf.CeilToInt(10 * str / def);
    }

    private async void Death() {
        isDead = true;
        sm.enabled = false;
        initParameter.ahp = null; //再開したときHP全回復するように削除
        await PlayFabCtrl.instance.SetUserData(initParameter);
        GetComponent<Animator>().SetTrigger("Die");
        await UniTask.Delay(1000);
        GetComponent<SpriteRenderer>().DOFade(0, 1);
        await UniTask.Delay(1000);
        GameManager.instance.GameOver();
    }

    //HP自然回復
    public override void RecoveryHp() {
        float recovery = Mathf.CeilToInt(affectedParameter.hpMax / normalRecoveryPer);
        RecoveryHp(recovery);
    }

    public void RecoveryHp(float recovery) {
        affectedParameter.hp += recovery;
        hpText.text = $"HP {affectedParameter.hp}/{affectedParameter.hpMax}";
        hpGauge.fillAmount = affectedParameter.hp / affectedParameter.hpMax;
    }

    private async void OnApplicationFocus(bool focus) {
        if (isStarting)
            return;
        //起動直後HPが0なのでhp!=0を条件に入れる、ゲームオーバー時に終了したらHPを0にする
        if (affectedParameter.hp != 0 || isDead)
            parameter.ahp = affectedParameter.hp;
        if (!focus)
            await PlayFabCtrl.instance.SetUserData(parameter);
    }
}