using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCD : MonoBehaviour
{
    [SerializeField] private LayerMask floorMask;
    [HideInInspector]public string stayingRoomName;

    private void OnTriggerEnter2D(Collider2D c) {
        //アイテムの上だと床の判定を取れないので、LayerMaskで指定する
        Collider2D hit = Physics2D.OverlapCircle(transform.position, 0.5f, floorMask);
        stayingRoomName = hit ? hit.name : "Not Floor1";
    }
}
