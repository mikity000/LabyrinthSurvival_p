using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// LogScrollオブジェクトにアタッチ
/// イベントログを表示するクラス
/// </summary>
public class Log : MonoBehaviour {
    private static Text logText;
    private static List<string> logs = new List<string>();
    private static Scrollbar verticalScrollbar; //縦のスクロールバー

    private void Awake() {
        logText = GetComponentInChildren<Text>();
        verticalScrollbar = GetComponentInChildren<Scrollbar>();
        AlignText();
    }

    public static void Add(string log) {
        //ログを先頭に追加し、30個だけ取得
        logs = logs.Prepend(log).Take(30).ToList();
        AlignText();
    }

    //新しい順にログを表示
    private static void AlignText() {
        logText.text = string.Join(Environment.NewLine, logs);
        // スクロールバーの位置の更新
        verticalScrollbar.value = 1f;
    }

    public static void EnlargeText(int size) {
        logText.fontSize = size;
    }
}