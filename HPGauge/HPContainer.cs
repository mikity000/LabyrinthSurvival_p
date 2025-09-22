using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// EnemyHPオブジェクトにアタッチ
/// HPゲージを配置する敵を追加、削除するクラス
/// </summary>
public class HPContainer : MonoBehaviour
{
    public static HPContainer instance;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private HPGauge hpBorder;
    private RectTransform rectTransform;
    private readonly Dictionary<EnemyParamsController, HPGauge> enemyHPGaugeMap = new Dictionary<EnemyParamsController, HPGauge>();

    void Awake()
    {
        if (instance == null)
            instance = this;
        rectTransform = GetComponent<RectTransform>();
    }

    public void Add(EnemyParamsController enemyParams)
    {
        HPGauge hpGauge = Instantiate(hpBorder, transform);
        hpGauge.Initialize(rectTransform, mainCamera, enemyParams);
        enemyHPGaugeMap.Add(enemyParams, hpGauge);
    }

    public void Remove(EnemyParamsController enemyParams)
    {
        if (enemyHPGaugeMap.ContainsKey(enemyParams))
        {
            Destroy(enemyHPGaugeMap[enemyParams].gameObject);
            enemyHPGaugeMap.Remove(enemyParams);
        }
    }
}
