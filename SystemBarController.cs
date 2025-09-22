using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

/// <summary>
/// ステータスバーを表示し、ナビゲーションバーは非表示にする
/// Hide Navigation BarはOFFにする
/// </summary>
public class SystemBarController
{
    private const int FLAG_FORCE_NOT_FULLSCREEN = 2048;
    private const int FLAG_FULLSCREEN = 1024;

    public static void Show()
    {
        Screen.fullScreen = false;
        using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        using var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        activity.Call("runOnUiThread", new AndroidJavaRunnable(RunOnUiThread));
    }

    private static void RunOnUiThread()
    {
        int apiLevel = GetAPILevel();
        //Android 11未満
        if (apiLevel < 30)
        {
            using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            using var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            using var window = activity.Call<AndroidJavaObject>("getWindow");
            window.Call("addFlags", FLAG_FORCE_NOT_FULLSCREEN);
            window.Call("clearFlags", FLAG_FULLSCREEN);
            window.Call("setStatusBarColor", ToARGB(Color.black));
        }
        //Android 11以上
        else
        {
            using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            using var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            using var window = activity.Call<AndroidJavaObject>("getWindow");
            window.Call("setStatusBarColor", ToARGB(Color.black));
            using var controller = window.Call<AndroidJavaObject>("getInsetsController");
            using var type = new AndroidJavaClass("android.view.WindowInsets$Type");
            controller.Call("show", type.CallStatic<int>("statusBars"));
            controller.Call("hide", type.CallStatic<int>("navigationBars"));
        }
    }

    private static int GetAPILevel()
    {
        using var version = new AndroidJavaClass("android.os.Build$VERSION");
        return version.GetStatic<int>("SDK_INT");
    }

    private static int ToARGB(Color color)
    {
        Color32 c = color;
        byte[] b = new byte[] { c.b, c.g, c.r, c.a };
        return BitConverter.ToInt32(b, 0);
    }
}
