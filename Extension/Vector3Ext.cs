using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Vector3Ext
{
    public static Vector3 Round(this Vector3 vector)
    {
        vector.x = Mathf.Round(vector.x);
        vector.y = Mathf.Round(vector.y);
        vector.z = Mathf.Round(vector.z);
        return vector;
    }

    public static Vector3 Clamp(this Vector3 vector, Vector3 min, Vector3 max)
    {
        vector.x = Mathf.Clamp(vector.x, min.x, max.x);
        vector.y = Mathf.Clamp(vector.y, min.y, max.y);
        vector.z = Mathf.Clamp(vector.z, min.z, max.z);
        return vector;
    }

    public static Vector3 PerpPointWith(this Vector3 p, Vector3 a, Vector3 b)
        => a + Vector3.Project(p - a, b - a);

    public static Vector3 NearestPointWith(this Vector3 p, Vector3 a, Vector3 b)
    {
        Vector3 ab = b - a;
        float length = ab.magnitude;
        ab.Normalize();

        float k = Vector3.Dot(p - a, ab);
        k = Mathf.Clamp(k, 0, length);
        return a + k * ab;
    }
}
