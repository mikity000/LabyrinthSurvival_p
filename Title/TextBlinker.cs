using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// StartTextにアタッチ
/// スタートを点滅させる
/// </summary>
public class TextBlinker : MonoBehaviour
{
    [SerializeField] private Text startText;
    private float duration = 1.1f;

    void Start()
    {
        startText.DOFade(0.0f, duration)
            .SetEase(Ease.InCubic)
            .SetLoops(-1, LoopType.Yoyo)
            .SetLink(gameObject);
    }
}
