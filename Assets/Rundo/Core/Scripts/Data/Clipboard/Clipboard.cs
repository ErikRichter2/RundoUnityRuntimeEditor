using System;
using System.Collections.Generic;
using System.Reflection;
using Rundo.Core.Utils;
using UnityEngine;
using UnityEngine.Assertions;

namespace Rundo.Core.Data
{
    public static class Clipboard
    {
        public static void Set(object obj)
        {
            GUIUtility.systemCopyBuffer = obj != null 
                ? RundoEngine.DataSerializer.SerializeObject(ClipboardData.Create(obj)) 
                : null;
        }

        public static T Clone<T>()
        {
            if (CanUse<T>() == false)
                return default;

            var clipboardBaseData = TryGetClipboardDataBase();
            if (clipboardBaseData.IsList)
                return default;

            return (T)CloneInternal();
        }

        public static List<T> CloneList<T>()
        {
            if (CanUse<T>() == false)
                return default;

            var res = CloneInternal();
            if (ReflectionUtils.IsList(res.GetType()) == false)
                res = new List<T>{(T)res};

            return (List<T>)res;
        }

        public static T Copy<T>()
        {
            if (CanUse<T>() == false)
                return default;

            var data = TryGetClipboardDataBase();
            if (data.IsList)
                return default;

            return (T)CopyInternal();
        }

        public static List<T> CopyList<T>()
        {
            if (CanUse<T>() == false)
                return default;

            var res = CopyInternal();
            if (ReflectionUtils.IsList(res.GetType()) == false)
                res = new List<T>{(T)res};

            return (List<T>)res;
        }

        private static object CloneInternal()
        {
            var clipboardData =
                RundoEngine.DataSerializer.DeserializeObject<ClipboardData>(GUIUtility
                    .systemCopyBuffer);

            var dataType = clipboardData.IsList ? typeof(List<>).MakeGenericType(clipboardData.GetDataType()) : clipboardData.GetDataType();
            var data = RundoEngine.DataSerializer.Clone(dataType, clipboardData.Data);
            return data;
        }

        private static object CopyInternal()
        {
            var clipboardData =
                RundoEngine.DataSerializer.DeserializeObject<ClipboardData>(GUIUtility
                    .systemCopyBuffer);

            var dataType = clipboardData.IsList ? typeof(List<>).MakeGenericType(clipboardData.GetDataType()) : clipboardData.GetDataType();
            var data = RundoEngine.DataSerializer.DeserializeObject(clipboardData.Data, dataType);
            return data;
        }

        private static bool CanUse<T>()
        {
            if (IsValid() == false)
                return false;
            if (IsType<T>() == false)
                return false;
            return true;
        }
/*
        public static T Paste<T>()
        {
            if (IsValid() == false)
                return default;
            if (IsType<T>() == false)
                return default;

            var obj = Paste();

            if (obj is T t)
                return t;

            return default;
        }

        public static object Paste()
        {
            if (IsValid() == false)
                return default;
            
            var clipboardData =
                RundoEngine.DataSerializer.DeserializeObject<ClipboardData>(GUIUtility
                    .systemCopyBuffer);

            var dataType = clipboardData.GetDataType();
            var data = RundoEngine.DataSerializer.Clone(dataType, clipboardData.Data);
            return data;
        }

        public static string PasteAsSerialized()
        {
            if (IsValid() == false)
                return default;
            
            var clipboardData =
                RundoEngine.DataSerializer.DeserializeObject<ClipboardData>(GUIUtility.systemCopyBuffer);

            return clipboardData.Data;
        }
*/
        private static bool IsValid()
        {
            return GetDataType() != null;
        }

        public static Type GetDataType()
        {
            return TryGetClipboardDataBase()?.GetDataType();
        }

        private static ClipboardDataBase TryGetClipboardDataBase()
        {
            try
            {
                return
                    RundoEngine.DataSerializer.DeserializeObject<ClipboardDataBase>(GUIUtility
                        .systemCopyBuffer);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static bool IsType<T>()
        {
            return IsType(typeof(T));
        }

        public static bool IsType(Type type)
        {
            var dataType = GetDataType();
            if (dataType != null)
                return type.IsAssignableFrom(dataType);
            return false;
        }
    }

    public class ClipboardDataBase
    {
        public bool IsList;
        public string TypeId;
        public string AssemblyQualifiedName;

        public Type GetDataType()
        {
            if (string.IsNullOrEmpty(TypeId) == false)
            {
                return RundoEngine.ReflectionService.GetTypeBySerializedDataTypeId(TypeId);
            }

            if (string.IsNullOrEmpty(AssemblyQualifiedName) == false)
            {
                return Type.GetType(AssemblyQualifiedName);
            }

            return null;
        }
    }

    public class ClipboardData : ClipboardDataBase
    {
        public string Data;

        public static ClipboardData Create(object obj)
        {
            Assert.IsNotNull(obj);

            var res = new ClipboardData();

            res.IsList = ReflectionUtils.IsList(obj.GetType());

            var type = obj.GetType();
            if (res.IsList)
                type = ReflectionUtils.GetListType(type);
            
            res.AssemblyQualifiedName = type.AssemblyQualifiedName;
            var typeIdAttr = type.GetCustomAttribute<DataTypeIdAttribute>();
            res.TypeId = typeIdAttr?.DataTypeId;
            res.Data = RundoEngine.DataSerializer.SerializeObject(obj);
            return res;
        }
    }
}

