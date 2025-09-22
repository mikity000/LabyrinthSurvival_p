using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// セーブしたいオブジェクトにアタッチ
/// </summary>
public class SavableEntity : MonoBehaviour {
    public string Key => name;

    public object SaveState() {
        Dictionary<string, object> state = new Dictionary<string, object>();
        //一つのオブジェクトにISavableを実装したコンポーネントが複数あるときループさせる
        foreach (ISavable savable in GetComponents<ISavable>()) {
            string typeName = savable.GetType().ToString();
            state[typeName] = savable.SaveState();
        }
        return state;
    }

    public void LoadState(object state) {
        Dictionary<string, object> stateDictionary = (Dictionary<string, object>)state;
        foreach (ISavable savable in GetComponents<ISavable>()) {
            string typeName = savable.GetType().ToString();
            if (stateDictionary.TryGetValue(typeName, out object savedState))
                savable.LoadState(savedState);
        }
    }
}