using Cinemachine;
using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// VirtualCameraオブジェクトにアタッチ
/// Playerの動きに合わせてカメラを移動させるクラス
/// </summary>
public class CameraMover : MonoBehaviour
{
    private GameObject player;
    private CinemachineVirtualCamera cinemachine;

    private async void Start()
    {
        await UniTask.WaitUntil(() => DungeonGenerator.isFinish);
        player = GameObject.FindGameObjectWithTag("Player");
        cinemachine = GetComponent<CinemachineVirtualCamera>();
        cinemachine.Follow = player.transform;
    }
}
