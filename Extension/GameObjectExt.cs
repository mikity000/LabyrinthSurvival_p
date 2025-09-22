using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// GameObjectの拡張クラス
/// </summary>
public static class GameObjectExt
{

    /// <summary>
    /// 親や子オブジェクトから指定したコンポーネントを取得する
    /// </summary>
    public static T GetComponentInParentAndChildren<T>(this GameObject gameObject) where T : Component
    {

        if (gameObject.GetComponentInParent<T>() != null)
        {
            return gameObject.GetComponentInParent<T>();
        }
        if (gameObject.GetComponentInChildren<T>() != null)
        {
            return gameObject.GetComponentInChildren<T>();
        }

        return gameObject.GetComponent<T>();
    }

    /// <summary>
    /// 親や子オブジェクトから指定したコンポーネントを全て取得する
    /// </summary>
    public static List<T> GetComponentsInParentAndChildren<T>(this GameObject gameObject) where T : Component
    {
        List<T> _list = new List<T>(gameObject.GetComponents<T>());

        _list.AddRange(new List<T>(gameObject.GetComponentsInChildren<T>()));
        _list.AddRange(new List<T>(gameObject.GetComponentsInParent<T>()));

        return _list;
    }

    /// <summary>
    /// 親や子オブジェクトから非アクティブも含めて指定したコンポーネントを全て取得する
    /// </summary>
    public static List<T> GetComponentsInParentAndChildren<T>(this GameObject gameObject, bool includeInactive) where T : Component
    {
        List<T> _list = new List<T>(gameObject.GetComponents<T>());

        _list.AddRange(new List<T>(gameObject.GetComponentsInChildren<T>(includeInactive)));
        _list.AddRange(new List<T>(gameObject.GetComponentsInParent<T>(includeInactive)));

        return _list;
    }
    
    public static string GetGameObjectPath(this GameObject obj) {
        return obj.transform.GetGameObjectPath();
    }
}