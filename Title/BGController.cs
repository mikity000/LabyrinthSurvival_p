using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// BackGroundにアタッチ
/// 背景を動かす
/// </summary>
public class BGController : MonoBehaviour
{
    private float speed = -1.5f;

    void Update()
    {
        transform.Translate(new Vector3(speed * Time.deltaTime, 0));
        if (transform.position.x < -6.5)
            transform.position = new Vector3(6.5f, 0);
    }
}
