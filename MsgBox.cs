using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MsgBox : MonoBehaviour
{
    private static GameObject msgPanel;
    private static Text msgText;

    private void Start() {
        Transform parent = GameObject.Find("GameCanvas").transform;
        msgPanel = parent.Find("MsgPanel").gameObject;
        msgText = msgPanel.GetComponentInChildren<Text>();
    }

    public static void Show(string msg) {
        msgPanel.SetActive(true);
        msgText.text = msg;
    }
}
