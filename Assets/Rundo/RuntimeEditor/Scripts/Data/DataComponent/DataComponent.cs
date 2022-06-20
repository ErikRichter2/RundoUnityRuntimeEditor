using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rundo.Core.Data;
using Rundo.Core.Events;
using Rundo.RuntimeEditor.Behaviours;
using Rundo.Core.Utils;
using UnityEngine;

namespace Rundo.RuntimeEditor.Data
{
    public abstract class DataComponent : BaseData, IDataSerializerPopulateHandler, ICustomDataDispatcher, ICustomInstantiate
    {
        public DataComponentType DataComponentType;
        public DataComponentPrefab DataComponentPrefab { get; set; }

        [JsonProperty]
        protected object _componentData;

        [JsonIgnore] public object ComponentData => _componentData;

        [JsonIgnore]
        public DataGameObject DataGameObject { get; private set; }

        [JsonIgnore]
        public bool SkipUpdateOfRuntimeBehaviour { get; set; }

        public bool IsOverriden()
        {
            return DataComponentPrefab != null && ((GetData() is IComponentIsAlwaysOverriden) || DataComponentPrefab.OverridePrefabComponent);
        }

        public bool IsReadOnly()
        {
            return DataComponentPrefab != null && DataComponentPrefab.OverridePrefabComponent == false && IsOverridable();
        }

        public bool IsOverridable()
        {
            return DataComponentPrefab != null && (GetData() is IComponentIsAlwaysOverriden == false);
        }

        public void FromPrefabDataToInstanceData()
        {
            if (DataComponentPrefab == null)
                return;
            
            RundoEngine.DataSerializer.Populate(DataComponentPrefab.GetPrefabData(), _componentData);
        }
        
        public object GetData()
        {
            return _componentData;
        }

        public void SetDataGameObjectParent(DataGameObject dataGameObject)
        {
            DataGameObject = dataGameObject;
            if (_componentData != null)
            {
                // set data component data
                if (_componentData is DataComponentMonoBehaviour dataComponentMonoBehaviour)
                {
                    dataComponentMonoBehaviour.DataGameObject = DataGameObject;
                    dataComponentMonoBehaviour.DataComponent = this;
                }
                else
                {
                    var member = ReflectionUtils.GetMemberInfo(_componentData.GetType(),
                        nameof(DataComponentMonoBehaviour.DataComponent));
                    if (member != null)
                        ReflectionUtils.SetValue(_componentData, member, this);

                    member = ReflectionUtils.GetMemberInfo(_componentData.GetType(),
                        nameof(DataComponentMonoBehaviour.DataGameObject));
                    if (member != null)
                        ReflectionUtils.SetValue(_componentData, member, DataGameObject);
                }
            }
        }
        
        public void Populate(JObject jObject, JsonSerializer serializer)
        {
            var componentData = _componentData;
            serializer.Populate(jObject.CreateReader(), this);
            
            _componentData = componentData;

            var reader = jObject[nameof(_componentData)].CreateReader();
            serializer.Populate(reader, _componentData);
        }
        

        public void Populate(string data, JsonSerializerSettings jsonSerializerSettings)
        {
            var componentData = _componentData;
            JsonConvert.PopulateObject(data, this, jsonSerializerSettings);
            
            _componentData = componentData;

            var jobject = JObject.Parse(data);
            var reader = jobject[nameof(_componentData)].CreateReader();
            JsonSerializer.CreateDefault(jsonSerializerSettings).Populate(reader, _componentData);
        }

        public static DataComponent Instantiate(Type componentType, IParentable parent)
        {
            if (componentType == null)
                return null;
            
            var genericType = typeof(DataComponent<>).MakeGenericType(componentType);
            var instance = (DataComponent)RundoEngine.DataFactory.Instantiate(genericType, parent);
            instance.Init(componentType);
            return instance;
        }
        
        public static object Instantiate(JObject jObject, IParentable parent)
        {
            return Instantiate(GetComponentTypeFromJObject(jObject), parent);
        }
        
        private void Init(Type type)
        {
            DataComponentType.Init(type);
            _componentData = RundoEngine.DataFactory.Instantiate(GetComponentType(), this);
            SetDataGameObjectParent(DataGameObject);
        }
        
        public Type GetComponentType()
        {
            return DataComponentType.GetComponentType();
        }

        private static Type GetComponentTypeFromJObject(JObject jObject)
        {
            Type res = null;
            
            if (jObject.TryGetValue(nameof(DataComponentType), out var dataComponentType))
                res = DataComponentType.GetComponentTypeFromJObject((JObject)dataComponentType);

            return res;
        }

        public void CopyFrom(object obj)
        {
            RundoEngine.DataSerializer.PopulateObject(obj, _componentData);
        }

        public void DispatchEvent(IEventSystem eventDispatcher, bool wasProcessed)
        {
            eventDispatcher.Dispatch(this, wasProcessed);
        }
    }

    public sealed class DataComponent<T> : DataComponent, IDataComponent<T>
    {
        [JsonIgnore]
        public T Data => (T)GetData();
        
        private DataComponent() {}
    }

    public interface IDataComponent
    {
        DataGameObject DataGameObject { get; }
        object GetData();
        void SetDataGameObjectParent(DataGameObject dataGameObject);
        void Populate(string data, JsonSerializerSettings jsonSerializerSettings);
        Type GetComponentType();
        void CopyFrom(object obj);
        bool IsReadOnly();
        DataComponentPrefab DataComponentPrefab { get; set; }
    }
    
    public interface IDataComponent<out T> : IDataComponent
    {
        T Data { get; }
        
        /// <summary>
        /// When true then the implicit call of data -> behaviour whe the data is changed is skipped.
        /// </summary>
        bool SkipUpdateOfRuntimeBehaviour { get; set; }
    }

    public struct DataComponentType
    {
        public string TypeId;
        public string AssemblyQualifiedName;

        public Type GetComponentType()
        {
            Type finalType = null;
            
            if (string.IsNullOrEmpty(TypeId) == false)
                finalType = RundoEngine.ReflectionService.GetTypeBySerializedDataTypeId(TypeId);
            
            if (finalType == null && string.IsNullOrEmpty(AssemblyQualifiedName) == false)
                finalType = Type.GetType(AssemblyQualifiedName);

            if (finalType == null)
                throw new Exception($"Type {TypeId} {AssemblyQualifiedName} not found !");
            
            return finalType;
        }

        public void Init(Type type)
        {
            AssemblyQualifiedName = type.AssemblyQualifiedName;
            TypeId = RundoEngine.ReflectionService.GetSerializedDataTypeIdByType(type);
        }

        public static Type GetComponentTypeFromJObject(JObject jObject)
        {
            Type componentType = null;
            string componentTypeId = null;
            string componentAssemblyQualifiedName = null;

            if (jObject.TryGetValue(nameof(TypeId), out var componentTypeIdToken))
            {
                componentTypeId = componentTypeIdToken.ToString();
                if (string.IsNullOrEmpty(componentTypeId) == false)
                    componentType = RundoEngine.ReflectionService.GetTypeBySerializedDataTypeId(componentTypeId);
            }
            else if (jObject.TryGetValue(nameof(AssemblyQualifiedName), out var componentAssemblyQualifiedNameToken))
            { 
                componentAssemblyQualifiedName = componentAssemblyQualifiedNameToken.ToString();
                if (string.IsNullOrEmpty(componentAssemblyQualifiedName) == false)
                    componentType = Type.GetType(componentAssemblyQualifiedName);
            }

            if (componentType == null)
                Debug.LogError(
                    $"Type for ID: {componentTypeId}, Name: {componentAssemblyQualifiedName} not found !");

            return componentType;
        }
    }

    public class DataComponentPrefab
    {
        private static readonly Dictionary<string, string> PrefabDataCache = new Dictionary<string, string>();

        [JsonProperty]
        private TGuid<object> _internalId { get; set; }
        
        public bool OverridePrefabComponent { get; set; }

        public DataComponentPrefab()
        {
            _internalId = TGuid<object>.Create();
        }

        public string GetPrefabData()
        {
            return PrefabDataCache[_internalId.ToStringRawValue()];
        }

        public void SetPrefabData(string prefabData)
        {
            PrefabDataCache[_internalId.ToStringRawValue()] = prefabData;
        }
    }
    
}

