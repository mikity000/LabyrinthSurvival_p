using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerCD : MonoBehaviour
{
    [SerializeField] PlayerParamsController playerPrams;
    [HideInInspector] public string stayingRoomName;
    [SerializeField] private GameObject diamondContainer;
    [SerializeField] Sprite openedTreasure;
    private SequenceManager sm;
    [SerializeField] private LayerMask floorMask;

    private void Start() {
        sm = GameObject.Find("SequenceManager").GetComponent<SequenceManager>();
    }

    private async void OnTriggerEnter2D(Collider2D c) {
        GameManager gm = GameManager.instance;
        if (c.CompareTag("Goal")) {
            sm.enabled = false;
            SaveLoadSystem.Instance.DeleteFile();
            playerPrams.parameter.ahp = playerPrams.affectedParameter.hp;
            await PlayFabCtrl.instance.SetUserData(playerPrams.parameter);
            gm.game.stage++;
            PlayerPrefs.SetString("gameData", JsonUtility.ToJson(gm.game));
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            return;
        } else if (c.CompareTag("Diamond")) {
            c.transform.parent = null;
            await PlayFabCtrl.instance.AddVirtualCurrency(1);
            Log.Add($"<color=#69ABDB>ダイヤ</color>が<color=#69ABDB>{PlayFabCtrl.instance.diaCount}</color>個になった！");
            c.gameObject.SetActive(false);
            Destroy(c.gameObject, 2f);
        } else if (c.CompareTag("Treasure")) {
            SoundManager.instance.PlaySound(SoundManager.instance.treasure);
            c.enabled = false; //連続獲得しないようにするため
            c.transform.parent = null; //獲得してすぐ再起動しても宝箱を生成させないため
            int diamondNum = Random.Range(1, 4);
            await PlayFabCtrl.instance.AddVirtualCurrency(diamondNum);
            Log.Add($"<color=#69ABDB>ダイヤ</color>を<color=#69ABDB>{diamondNum}</color>個獲得して<color=#69ABDB>{PlayFabCtrl.instance.diaCount}</color>個になった！");
            PopupDiamond(diamondNum.ToString());

            //宝箱を開けるアニメーション
            Transform treasureTrans = c.transform;
            Sequence seq = DOTween.Sequence();
            seq.Append(treasureTrans.DOJump(Vector3.zero, 0.5f, 1, 0.75f).SetRelative())
            .Join(treasureTrans.DOScale(Vector3.one * 1.4f, 0.5f).SetEase(Ease.OutBack))
            .InsertCallback(0, () => treasureTrans.GetComponent<SpriteRenderer>().sprite = openedTreasure)
            .Insert(0.5f, treasureTrans.DOScale(treasureTrans.localScale, 0.25f).SetEase(Ease.OutCirc));

            DOVirtual.DelayedCall(2f, () => treasureTrans.gameObject.SetActive(false));
            Destroy(treasureTrans.gameObject, 4f);
        }

        //アイテムの上だと床の判定を取れないので、接触しているオブジェクトを複数取得する
        Collider2D hit = Physics2D.OverlapCircle(transform.position, 0.5f, floorMask);
        stayingRoomName = hit ? hit.name : "Not Floor2";
    }

    private void PopupDiamond(string diamondNum) {
        GameObject diamondParent = Instantiate(diamondContainer, transform);
        diamondParent.transform.position += new Vector3(-0.3f, 1f);
        TextMeshPro damageText = diamondParent.transform.GetChild(0).GetComponent<TextMeshPro>();
        damageText.SetText($"<sprite=1>+ {diamondNum}");
        Destroy(diamondParent, 2.5f);
    }
}
