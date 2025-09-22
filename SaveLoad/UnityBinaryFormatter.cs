using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class UnityBinaryFormatter : MonoBehaviour {
    public static BinaryFormatter BinaryFormatter => GetBinaryFormatter();

    private static BinaryFormatter GetBinaryFormatter() {
        BinaryFormatter formatter = new BinaryFormatter();
        SurrogateSelector selector = new SurrogateSelector();
        UnitySerializer.Register(selector);
        formatter.SurrogateSelector = selector;
        return formatter;
    }
}
