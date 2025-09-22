using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// アイテムプレファブにアタッチ
/// ドロップアニメーション、所持品の追加・保存するクラス
/// </summary>
public class Item : MonoBehaviour
{
    [SerializeField] private AnimationCurve customEase;
    [SerializeField] private ItemType type;

    private List<Vector3> dropablePoses = new List<Vector3>(); //ドロップ可能座標List
    private static Vector3[] staticDropablePoses; //ドロップ可能座標配列
    public static List<Vector3> itemPoses = new List<Vector3>(); //アイテムが存在している座標List
    private Vector3 dropPos; //アイテムドロップ先座標
    private GameObject player;
    private BoxCollider2D boxCollider;

    private void Start()
    {
        SceneManager.sceneLoaded += SceneLoaded;
    }

    void SceneLoaded(Scene nextScene, LoadSceneMode mode){
        //static変数はシーン遷移時に自動的に初期化されないため明示的に初期化
        itemPoses = new List<Vector3>();
    }

    private void OnEnable()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        boxCollider = GetComponent<BoxCollider2D>();

        Transform parent = GameObject.Find("GameManager").transform;
        //アイテムをドロップできる座標を格納
        foreach (Transform child in parent.GetGrandchild())
            if (child.CompareTag("Floor") || child.CompareTag("Aisle"))
                dropablePoses.Add((Vector2)child.position);
    }

    // アイテムの種類定義
    public enum ItemType
    {
        sword, // 剣
        armor, // 鎧
        apple, //生命の実
        long_distance_attack, //遠距離攻撃
        range_attack, //範囲攻撃
        absorb, //吸収
        book, //本
        chemicals, //薬品
        coin //ラッキーコイン
    }

    // ドロップ処理
    public void drop(int i)
    {
        //アイテムドロップ座標取得
        dropPos = i == 0 ? GetDropPos() : staticDropablePoses[i];
        itemPoses.Add(dropPos);

        // アイテム生成アニメーション
        Sequence seq = DOTween.Sequence();
        if (dropPos != transform.position)
            seq.Append(transform.DOMove(dropPos, 0.3f))
                .OnStart(() => boxCollider.enabled = false)
                .OnComplete(() => boxCollider.enabled = true);
        seq.Append(transform.DOScale(Vector3.zero, 0f));
        seq.Append(transform.DOScale(transform.localScale, 0.5f).SetEase(customEase));
    }

    private Vector2 GetDropPos()
    {
        //アイテムとプレイヤーが存在せず、一番近い座標を返す
        staticDropablePoses = dropablePoses.Except(itemPoses)
            .Where(v => v != player.transform.position)
            .OrderBy(v => (v - transform.position).sqrMagnitude)
            .ToArray();
        return staticDropablePoses[0];
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;
        
        //itemPosesから獲得したアイテムの座標を除外する
        itemPoses.Remove(dropPos);

        SoundManager.instance.PlaySound(SoundManager.instance.pickUp);
        if (CompareTag("Diamond"))
            return;

        // プレイヤーの所持品として追加
        PlayFabCtrl.instance.GrantItem(type.ToString());

        //マップ処理のため少し破棄を遅らせる
        gameObject.SetActive(false);
        Destroy(gameObject, 2f);
    }
}
