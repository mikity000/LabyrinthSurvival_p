using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

public static class UnitySerializer {
    public class Vector3Serialization : ISerializationSurrogate {
        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context) {
            Vector3 vec = (Vector3)obj;
            info.AddValue("x", vec.x);
            info.AddValue("y", vec.y);
            info.AddValue("z", vec.z);
        }

        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector) {
            Vector3 vec = (Vector3)obj;
            vec.x = info.GetSingle("x");
            vec.y = info.GetSingle("y");
            vec.z = info.GetSingle("z");
            return vec;
        }
    }

    public class Vector2Serialization : ISerializationSurrogate {
        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context) {
            Vector2 vec = (Vector2)obj;
            info.AddValue("x", vec.x);
            info.AddValue("y", vec.y);
        }

        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector) {
            Vector2 vec = (Vector2)obj;
            vec.x = info.GetSingle("x");
            vec.y = info.GetSingle("y");
            return vec;
        }
    }

    public class QuaternionSerialization : ISerializationSurrogate {
        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context) {
            Quaternion quat = (Quaternion)obj;
            info.AddValue("x", quat.x);
            info.AddValue("y", quat.y);
            info.AddValue("z", quat.z);
            info.AddValue("w", quat.w);
        }

        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector) {
            Quaternion quat = (Quaternion)obj;
            quat.x = info.GetSingle("x");
            quat.y = info.GetSingle("y");
            quat.z = info.GetSingle("z");
            quat.w = info.GetSingle("w");
            return quat;
        }
    }

    public class ColorSerialization : ISerializationSurrogate {
        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context) {
            Color color = (Color)obj;
            info.AddValue("r", color.r);
            info.AddValue("g", color.g);
            info.AddValue("b", color.b);
            info.AddValue("a", color.a);
        }

        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector) {
            Color color = (Color)obj;
            color.r = info.GetSingle("r");
            color.g = info.GetSingle("g");
            color.b = info.GetSingle("b");
            color.a = info.GetSingle("a");
            return color;
        }
    }

    //Transformを保存する場合、インスペクター上に最初からオブジェクトが存在する必要がある
    private class TransformSerialization : ISerializationSurrogate {
        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context) {
            Transform val = (Transform)obj;
            if (val.gameObject == null) {
                throw new ArgumentException("Transform must have an associated gameObject.");
            }
            string gameObjectPath = val.gameObject.GetGameObjectPath();
            info.AddValue("path", gameObjectPath);
        }

        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector) {
            string @string = info.GetString("path");
            GameObject val = GameObject.Find(@string);
            if (val == null) {
                throw new KeyNotFoundException("Could not locate Transform at path " + @string);
            }
            return val.transform;
        }
    }

    //RectTransformを保存する場合、インスペクター上に最初からオブジェクトが存在する必要がある
    private class RectTransformSerialization : ISerializationSurrogate {
        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context) {
            RectTransform val = (RectTransform)obj;
            string gameObjectPath = val.gameObject.GetGameObjectPath();
            info.AddValue("path", gameObjectPath);
        }

        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector) {
            string @string = info.GetString("path");
            GameObject val = GameObject.Find(@string);
            return val.GetComponent<RectTransform>();
        }
    }

    public static void Register(SurrogateSelector selector) {
        selector.AddSurrogate(typeof(Vector3), new StreamingContext(StreamingContextStates.All), new Vector3Serialization());
        selector.AddSurrogate(typeof(Vector2), new StreamingContext(StreamingContextStates.All), new Vector2Serialization());
        selector.AddSurrogate(typeof(Quaternion), new StreamingContext(StreamingContextStates.All), new QuaternionSerialization());
        selector.AddSurrogate(typeof(Color), new StreamingContext(StreamingContextStates.All), new ColorSerialization());
        //selector.AddSurrogate(typeof(Transform), new StreamingContext(StreamingContextStates.All), new TransformSerialization());
        //selector.AddSurrogate(typeof(RectTransform), new StreamingContext(StreamingContextStates.All), new RectTransformSerialization());
    }
}
