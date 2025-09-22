using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public static class TransformExt
{
    public static void LookAt2D(this Transform transform, Transform target)
    {
        LookAt2D(transform, target.position, Vector3.forward, 0);
    }

    public static void LookAt2D(this Transform transform, Vector2 target)
    {
        LookAt2D(transform, target, Vector3.forward, 0);
    }

    public static void LookAt2D(this Transform transform, Transform target, float angle)
    {
        LookAt2D(transform, target.position, Vector3.forward, angle);
    }

    public static void LookAt2D(this Transform transform, Vector2 target, float angle)
    {
        LookAt2D(transform, target, Vector3.forward, angle);
    }

    public static void LookAt2D(this Transform transform, Transform target, Vector3 axis)
    {
        LookAt2D(transform, target.position, axis, 0);
    }

    public static void LookAt2D(this Transform transform, Vector2 target, Vector3 axis)
    {
        LookAt2D(transform, target, axis, 0);
    }

    public static void LookAt2D(this Transform transform, Transform target, Vector3 axis, float angle)
    {
        LookAt2D(transform, target.position, axis, angle);
    }

    public static void LookAt2D(this Transform transform, Vector3 target, Vector3 axis, float angle)
    {
        Vector2 dir = target - transform.position;
        angle += Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90;
        transform.rotation = Quaternion.AngleAxis(angle, axis);
    }
    
    public static string GetGameObjectPath(this Transform transform) {
        StringBuilder sb = new StringBuilder();
        sb.Append(transform.name);
        Transform parent = transform.parent;
        while (parent) {
            sb.Insert(0, "/").Insert(0, parent.name);
            parent = parent.parent;
        }
        return sb.ToString();
    }

    public static Transform[] GetGrandchild(this Transform transform) {
        List<Transform> list = new List<Transform>();
        foreach (Transform child in transform) {
            foreach (Transform grandchild in child) {
                list.Add(grandchild);
            }
        }
        return list.ToArray();
    }

    public static Transform[] GetDescendants(this Transform transform) {
        return transform.GetComponentsInChildren<Transform>().Skip(1).ToArray();
    }
}
