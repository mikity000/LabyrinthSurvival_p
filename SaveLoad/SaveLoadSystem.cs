using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

/// <summary>
/// SaveLoadSystemにアタッチ
/// </summary>
public class SaveLoadSystem : MonoBehaviour {

    public string SavePath => $"{Application.persistentDataPath}/save.txt";
    [SerializeField] private SavableEntity[] loadOrder; //ロードする順番
    private static SaveLoadSystem instance;
    public static SaveLoadSystem Instance {
        get {
            if (instance == null)
                instance = FindObjectOfType<SaveLoadSystem>();
            return instance;
        }
    }

    private void Awake() {
        if (File.Exists(SavePath))
            Load();
    }

    private void OnApplicationFocus(bool focus) {
        if (!focus)
            Save();
    }

    //現在ヒエラルキー上に存在するSaveDataをセーブする
    public void Save() {
        Dictionary<string, object> state = LoadFile();
        SaveState(state);
        SaveFile(state);
    }

    //現在ヒエラルキー上に存在するSaveDataをロードする
    public void Load() {
        Dictionary<string, object> state = LoadFile();
        foreach (SavableEntity savable in loadOrder) {
            LoadState(state, savable);
        }
    }

    //選択したSaveDataだけロードする
    public void SelectLoad(SavableEntity savable) {
        Dictionary<string, object> state = LoadFile();
        LoadState(state, savable);
    }

    //複数のSaveDataをロードする
    public void MultiLoad(List<SavableEntity> savables) {
        Dictionary<string, object> state = LoadFile();
        foreach (SavableEntity savable in savables) {
            LoadState(state, savable);
        }
    }

    public bool HasKey(string name) {
        Dictionary<string, object> state = LoadFile();
        return state.ContainsKey(name);
    }

    private void LoadState(Dictionary<string, object> state, SavableEntity savable) {
        if (state.TryGetValue(savable.Key, out object savedState))
            savable.LoadState(savedState);
    }

    private void SaveState(Dictionary<string, object> state) {
        foreach (SavableEntity savable in FindObjectsOfType<SavableEntity>()) {
            state[savable.Key] = savable.SaveState();
        }
    }

    private Dictionary<string, object> LoadFile() {
        if (!File.Exists(SavePath))
            return new Dictionary<string, object>();

        using FileStream stream = File.Open(SavePath, FileMode.Open);
        BinaryFormatter formatter = UnityBinaryFormatter.BinaryFormatter;
        return (Dictionary<string, object>)formatter.Deserialize(stream);
    }

    private void SaveFile(object state) {
        using FileStream stream = File.Open(SavePath, FileMode.Create);
        BinaryFormatter formatter = UnityBinaryFormatter.BinaryFormatter;
        formatter.Serialize(stream, state);
    }

    public void DeleteState(string key) {
        Dictionary<string, object> state = LoadFile();
        if (state.Remove(key))
            SaveFile(state);
    }

    public void DeleteFile() {
        if (File.Exists(SavePath))
            File.Delete(SavePath);
    }
}