using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public abstract class ActorParamsController : MonoBehaviour
{
    public abstract Params parameter { get; set; }
    public abstract void BeAttacked(ActorParamsController otherParam);
    public virtual TextMeshPro PopupDamage(GameObject damageContainer) {
        GameObject damageParent = Instantiate(damageContainer);
        Transform damageTrans = damageParent.transform;
        damageTrans.position = transform.position;
        TextMeshPro damageText = damageTrans.GetChild(0).GetComponent<TextMeshPro>();
        //ダメージアニメーション
        Sequence seq = DOTween.Sequence();
        seq.Append(damageTrans.DOMoveY(0.75f, 0.8f).SetRelative())
        .Join(damageTrans.DOScale(0.65f, 0.8f))
        .Join(damageText.DOFade(0.25f, 0.8f).SetEase(Ease.Linear))
        .AppendCallback(() => { Destroy(damageParent); });
        seq.Play();
        return damageText;
    }
    public virtual void RecoveryHp() { }
    public virtual void LevelUp() { }
}
