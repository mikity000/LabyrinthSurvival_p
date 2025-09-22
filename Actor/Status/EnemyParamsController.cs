using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Enemyプレファブにアタッチ
/// 敵のステータスを制御
/// </summary>
public class EnemyParamsController : ActorParamsController, ISavable {
    public Params Parameter; //インスペクターで設定
    public override Params parameter {
        get => Parameter;
        set => Parameter = value;
    }
    [SerializeField] private EnemyItemDropper dropper;
    [SerializeField] private GameObject damageContainer;
    private Text nextLvText;

    private void Start() {
        nextLvText = GameObject.Find("NextLvText").GetComponent<Text>();
        HPContainer.instance.Add(this);
        //敵のセーブデータが存在すればreturn
        if (SaveLoadSystem.Instance.HasKey(name))
            return;
        SetParameter(parameter);
    }

    // パラメーターを纏めて設定する
    public void SetParameter(Params p) {
        float enhance = Mathf.Pow(1.1f, GameManager.instance.game.stage);
        parameter.name = p.name;
        parameter.hpMax = Mathf.RoundToInt(p.hpMax * enhance);
        parameter.hp = Mathf.RoundToInt(p.hp * enhance);
        parameter.str = Mathf.RoundToInt(p.str * enhance * 1.2f);
        parameter.hasExp = Mathf.RoundToInt(p.hasExp * enhance);
    }

    //ダメージを受ける(override)
    public override void BeAttacked(ActorParamsController actor) {
        SoundManager.instance.PlaySound(SoundManager.instance.playerAttack);
        PlayerParamsController player = (PlayerParamsController)actor;
        int dmg = player.affectedParameter.str;
        if (player.parameter.absorbLv > 0) {
            int recovery = dmg.GetRatio(player.parameter.absorbLv * 10);
            player.RecoveryHp(recovery);
            Log.Add($"<color=#69ABDB>勇者</color>は<color=#00ff2a>{recovery}</color>回復した");
            PopupRecovery(recovery.ToString(), player);
        }
        parameter.hp -= dmg;
        Log.Add($"<color=#69ABDB>勇者</color>は<color=#e82727>{parameter.name}</color>に<color=#e82727>{dmg}</color>のダメージ");
        PopupDamage(dmg.ToString());
        if (parameter.hp == 0)
            Death(player);
    }

    public void PopupDamage(string damage) {
        TextMeshPro damageText = base.PopupDamage(damageContainer);
        damageText.SetText(damage);
    }

    public void PopupRecovery(string recovery, PlayerParamsController player) {
        TextMeshPro damageText = base.PopupDamage(damageContainer);
        damageText.transform.parent.position = player.transform.position;
        damageText.color = new Color32(0, 255, 44, 255);
        damageText.SetText(recovery);
    }

    private void Death(PlayerParamsController player) {
        HPContainer.instance.Remove(this);
        dropper.DropItem();
        int getExp = Mathf.RoundToInt(parameter.hasExp.RatioPlusToFloat(player.parameter.expUp));
        player.parameter.hasExp += getExp;
        Log.Add($"<color=#69ABDB>{getExp}</color>の経験値を獲得");
        if (player.parameter.hasExp >= player.parameter.nextLvUpExp)
            player.LevelUp();
        nextLvText.text = $"次のLvまで\n{player.parameter.nextLvUpExp - player.parameter.hasExp}";
        //マップ処理のため少し破棄を遅らせる
        gameObject.SetActive(false);
        Destroy(gameObject, 2f);
    }

    #region Save&Load
    public object SaveState() {
        return new SaveData(parameter);
    }

    public void LoadState(object state) {
        SaveData saveData = (SaveData)state;
        parameter = JsonUtility.FromJson<Params>(saveData.parameter);
    }

    [System.Serializable]
    private struct SaveData {
        public string parameter;
        public SaveData(Params parameter) {
            this.parameter = JsonUtility.ToJson(parameter);
        }
    }
    #endregion
}
