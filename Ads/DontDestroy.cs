using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// RewardCanvasにアタッチ
/// リワード広告ボタンを破棄せず、シーン上に一つだけにする
/// </summary>
public class DontDestroy : MonoBehaviour
{
    public static DontDestroy instance;
    void Start()
    {
        if (instance != null) {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
