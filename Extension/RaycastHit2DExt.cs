using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RaycastHit2DExt
{
    public static Vector2 GetExitPointOfCircle(this RaycastHit2D hit, Vector3 origin, CircleCollider2D col)
    {
        Vector3 scale = col.transform.localScale;
        float diameter = col.radius * 2 * Mathf.Max(scale.x, scale.y);
        Vector3 nomalPoint = hit.point - hit.normal * diameter;
        return nomalPoint.PerpPointWith(origin, hit.point);
    }
}
