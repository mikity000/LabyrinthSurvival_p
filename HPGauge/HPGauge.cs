using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// HPBorderプレファブにアタッチ
/// HPの減少、HPゲージを敵の上に配置するクラス
/// </summary>
public class HPGauge : MonoBehaviour
{
    [SerializeField] private Image hpGauge;
    private RectTransform parentRectTransform;
    private Camera mainCamera;
    private EnemyParamsController enemyParams;
    Rect rect = new Rect(0, 0, 1, 1); // 画面内か判定するためのRect
    Image[] images; //HPBorder以下のImageコンポーネントが格納されるList
    Image hpBorder;

    void Start()
    {
        images = gameObject.GetComponentsInChildren<Image>();
        hpBorder = GetComponent<Image>();
    }

    void Update()
    {
            Refresh();
    }

    public void Initialize(RectTransform parentRectTransform, Camera mainCamera, EnemyParamsController enemyParams)
    {
        this.parentRectTransform = parentRectTransform;
        this.mainCamera = mainCamera;
        this.enemyParams = enemyParams;
    }

    private void Refresh()
    {
        hpGauge.fillAmount = enemyParams.parameter.hp / enemyParams.parameter.hpMax;
        Vector3 screenPoint = mainCamera.WorldToScreenPoint(enemyParams.transform.position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRectTransform, screenPoint, null, out Vector2 localPoint);
        transform.localPosition = localPoint + new Vector2(0, 50);
        
        //敵が画面内にいればHP表示、いなければHP非表示(操作パネルと重なるため)
        Vector3 viewportPos = mainCamera.ScreenToViewportPoint(screenPoint);
        if (rect.Contains(viewportPos) && hpBorder != null && !hpBorder.enabled)
            foreach (Image image in images)
                image.enabled = true;
        else if(!rect.Contains(viewportPos) && hpBorder != null && hpBorder.enabled)
            foreach (Image image in images)
                image.enabled = false;
    }
}
