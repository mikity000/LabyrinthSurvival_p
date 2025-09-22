using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 角度と解像度を指定して扇形にRayCastする
/// (angle,resolution)=(45f,1f)、(90f,2f)、(135f,3f)、(180f,4f)
/// </summary>
public static class Physics2DExt
{
    public static Collider2D[] FanShapeCast(Vector3 origin, float angle, float resolution)
    {
        float radius = Mathf.Infinity;
        int layerMask = Physics2D.AllLayers;
        return FanShapeCast(origin, angle, resolution, radius, layerMask);
    }

    public static Collider2D[] FanShapeCast(Vector3 origin, float angle, float resolution, float radius)
    {
        int layerMask = Physics2D.AllLayers;
        return FanShapeCast(origin, angle, resolution, radius, layerMask);
    }

    public static Collider2D[] FanShapeCast(Vector3 origin, float angle, float resolution, float radius, int layerMask)
    {
        List<Collider2D> hits = new List<Collider2D>();
        for (float tmpAngle = -angle; tmpAngle <= angle; tmpAngle += angle / resolution)
        {
            Vector3 dir = Quaternion.AngleAxis(tmpAngle, Vector3.forward) * Vector3.up;
            RaycastHit2D hit = Physics2D.Raycast(origin, dir, radius, layerMask);
            if (hit && !hits.Contains(hit.collider))
                hits.Add(hit.collider);
        }
        return hits.ToArray();
    }

    public static void FanShapeDrawRay(Vector3 origin, float angle, float resolution, float radius)
    {
        Color color = Color.white;
        float duration = 0.0f;
        FanShapeDrawRay(origin, angle, resolution, radius, color, duration);
    }

    public static void FanShapeDrawRay(Vector3 origin, float angle, float resolution, float radius, Color color)
    {
        float duration = 0.0f;
        FanShapeDrawRay(origin, angle, resolution, radius, color, duration);
    }

    public static void FanShapeDrawRay(Vector3 origin, float angle, float resolution, float radius, Color color, float duration)
    {
        for (float tmpAngle = -angle; tmpAngle <= angle; tmpAngle += angle / resolution)
        {
            Vector3 dir = Quaternion.AngleAxis(tmpAngle, Vector3.forward) * Vector3.up;
            Debug.DrawRay(origin, dir * radius, color, duration);
        }
    }
}