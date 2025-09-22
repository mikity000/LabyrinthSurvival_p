using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class NumberExt
{
    public static int RatioPlusToInt(this int num, float ratio) {
        return Mathf.RoundToInt(num + num * ratio / 100);
    }

    public static int RatioPlusToInt(this float num, float ratio) {
        return Mathf.RoundToInt(num + num * ratio / 100);
    }

    public static float RatioPlusToFloat(this int num, float ratio) {
        return num + num * ratio / 100;
    }

    public static float RatioPlusToFloat(this float num, float ratio) {
        return num + num * ratio / 100;
    }

    public static int GetRatio(this int num, float ratio) {
        return (int)Mathf.Lerp(0, num, ratio / 100);
    }
}
