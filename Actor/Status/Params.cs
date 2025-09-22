using UnityEngine;

[System.Serializable]
public class Params {
    public string name;
    public int lv;    // レベル(敵はなし)
    public float Hp;    // HP
    public float hp {
        get => Hp;
        set => Hp = Mathf.Clamp(value, 0, hpMax);
    }
    public float? ahp;
    public float hpMax; // 最大HP
    public int str;   // 攻撃力
    public int def;   // 防御力(敵はなし)
    public int hasExp;   // 獲得した経験値
    public int nextLvUpExp; // レベルアップに必要な経験値(敵はなし)
    [HideInInspector] public float expUp = 1;
    [HideInInspector] public float statusUp = 1;
    [HideInInspector] public float dropUp = 1;
    [HideInInspector] public int longLv;
    [HideInInspector] public int rangeLv;
    [HideInInspector] public int absorbLv;
};