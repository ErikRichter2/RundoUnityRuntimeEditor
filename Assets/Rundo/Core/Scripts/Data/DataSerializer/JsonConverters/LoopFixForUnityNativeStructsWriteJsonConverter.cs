using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Rundo.Core.Data
{
    public class LoopFixForUnityNativeStructsWriteJsonConverter : JsonConverter
    {
        private struct ColorWrapper
        {
            public float r;
            public float g;
            public float b;
            public float a;

            public ColorWrapper(Color value)
            {
                r = value.r;
                g = value.g;
                b = value.b;
                a = value.a;
            }
        }
        private struct Vector2Wrapper
        {
            public float x;
            public float y;

            public Vector2Wrapper(Vector2 value)
            {
                x = value.x;
                y = value.y;
            }
        }
        private struct Vector3Wrapper
        {
            public float x;
            public float y;
            public float z;

            public Vector3Wrapper(Vector3 value)
            {
                x = value.x;
                y = value.y;
                z = value.z;
            }
        }
        private struct Vector2IntWrapper
        {
            public int x;
            public int y;

            public Vector2IntWrapper(Vector2Int value)
            {
                x = value.x;
                y = value.y;
            }
        }
        private struct Vector3IntWrapper
        {
            public int x;
            public int y;
            public int z;

            public Vector3IntWrapper(Vector3Int value)
            {
                x = value.x;
                y = value.y;
                z = value.z;
            }
        }
        
        public override bool CanRead => false;
        
        public override bool CanConvert(Type objectType)
        {
            return typeof(Color) == objectType ||
                   typeof(Vector3) == objectType ||
                   typeof(Vector3Int) == objectType ||
                   typeof(Vector2) == objectType ||
                   typeof(Vector2Int) == objectType;
        }
        
#nullable enable
        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
#nullable disable
        {
            throw new NotImplementedException();
        }
        
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is Color color)
                serializer.Serialize(writer, new ColorWrapper(color));
            else if (value is Vector2 vector2)
                serializer.Serialize(writer, new Vector2Wrapper(vector2));
            else if (value is Vector2Int vector2int)
                serializer.Serialize(writer, new Vector2IntWrapper(vector2int));
            else if (value is Vector3 vector3)
                serializer.Serialize(writer, new Vector3Wrapper(vector3));
            else if (value is Vector3Int vector3int)
                serializer.Serialize(writer, new Vector3IntWrapper(vector3int));
            else
                throw new Exception($"Unhandled type {value.GetType().Name}");
        }
    }
}

