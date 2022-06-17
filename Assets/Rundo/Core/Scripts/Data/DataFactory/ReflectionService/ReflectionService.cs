using System;
using System.Collections.Generic;
using System.Reflection;
using Rundo.RuntimeEditor.Attributes;
using Rundo.RuntimeEditor.Behaviours.UI;
using Rundo.Ui;
using UnityEngine;

namespace Rundo.Core.Data
{
    public class ReflectionService
    {
        private bool _wasInit;
        private readonly Dictionary<string, Type> _serializedDataTypeIdLookup = new Dictionary<string, Type>();
        private readonly Dictionary<Type, string> _serializedDataTypeIdInverseLookup = new Dictionary<Type, string>();
        private readonly Dictionary<Type, PropertyInfo> _serializedDataModelTypeLookup = new Dictionary<Type, PropertyInfo>();
        private readonly List<Type> _allowedComponents = new List<Type>();
        private readonly List<Type> _potentialPolymorphed = new List<Type>();
        private readonly List<Type> _isNotPolymorphed = new List<Type>();
        private readonly List<string> _monoBehaviourMembers = new List<string>();
        private readonly List<(Type, CustomInspectorAttribute)> _customInspectors = new List<(Type, CustomInspectorAttribute)>();

        public List<(Type, CustomInspectorAttribute)> GetCustomInspectors()
        {
            Init();
            return _customInspectors;
        }
        
        public bool CanTypeBePolymorphed(Type type)
        {
            if (_potentialPolymorphed.Contains(type))
                return true;

            if (_serializedDataTypeIdInverseLookup.ContainsKey(type))
                return true;

            if (_isNotPolymorphed.Contains(type))
                return false;
            
            foreach (var it in _serializedDataTypeIdInverseLookup.Keys)
                if (it.IsSubclassOf(type))
                {
                    _potentialPolymorphed.Add(type);
                    return true;
                }

            _isNotPolymorphed.Add(type);
            return false;
        }

        public bool IsAllowedComponent(Type type)
        {
            Init();
            return _allowedComponents.Contains(type);
        }

        public List<Type> GetAllowedComponents()
        {
            Init();
            return _allowedComponents;
        }

        public string GetSerializedDataTypeIdByType(Type serializedDataType)
        {
            Init();
            if (serializedDataType == null)
                return null;
            if (_serializedDataTypeIdInverseLookup.TryGetValue(serializedDataType, out var typeId))
                return typeId;
            return null;
        }
        
        public Type GetTypeBySerializedDataTypeId(string serializedDataTypeId)
        {
            Init();
            if (string.IsNullOrEmpty(serializedDataTypeId))
                return default;
            if (_serializedDataTypeIdLookup.TryGetValue(serializedDataTypeId, out var type))
                return type;
            return default;
        }

        public PropertyInfo GetSerializedDataExplicitModelPropertyInfoBySerializedDataType(Type serializedDataType)
        {
            Init();
            if (_serializedDataModelTypeLookup.TryGetValue(serializedDataType, out var propertyInfo))
                return propertyInfo;
            return default;
        }
        
        private void Init()
        {
            if (_wasInit)
                return;

            _wasInit = true;

            foreach (var memberInfo in typeof(MonoBehaviour).GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                _monoBehaviourMembers.Add(memberInfo.Name);
            }
            
            foreach(var assembly in AppDomain.CurrentDomain.GetAssemblies())
            foreach (var type in assembly.GetTypes())
            {
                var attr = type.GetCustomAttribute<DataTypeIdAttribute>();
                if (attr != null)
                {
                    if (_serializedDataTypeIdLookup.ContainsKey(attr.DataTypeId))
                        throw new Exception(
                            $"Cannot add {nameof(type)} with TypeId {attr.DataTypeId}, already used in {_serializedDataTypeIdLookup[attr.DataTypeId].Name}");
                    _serializedDataTypeIdLookup[attr.DataTypeId] = type;
                    _serializedDataTypeIdInverseLookup[type] = attr.DataTypeId;
                }

                var props = GetPropertiesWithAttribute<ExplicitModelAttribute>(type);
                if (props != null && props.Count > 0)
                    _serializedDataModelTypeLookup[type] = props[0].prop;

                if (IsComponentAllowed(type))
                {
                    var dataComponentAttr = type.GetCustomAttribute<DataComponentAttribute>();
                    if (dataComponentAttr != null)
                        AllowComponent(type);
                }

                var customInspector = type.GetCustomAttribute<CustomInspectorAttribute>();
                if (customInspector != null)
                {
                    if (typeof(IInspectorWindowElementBehaviour).IsAssignableFrom(type) == false)
                        throw new Exception($"Class {type.Name} with {nameof(CustomInspectorAttribute)} attribute must implement {nameof(IInspectorWindowElementBehaviour)}");
                    _customInspectors.Add((type, customInspector));
                }
            }
        }

        public static bool IsComponentAllowed(Type type, bool silent = true)
        {
            if (type.IsClass == false)
            {
                if (silent == false)
                    throw new Exception($"Cannot use type {type.Name} as component - type is not a class");
                return false;
            }

            if (type.IsAbstract)
            {
                if (silent == false)
                    throw new Exception($"Cannot use type {type.Name} as component - type is an abstract class");
                return false;
            }

            return true;
        }

        public void AllowComponent(Type type)
        {
            Init();
            
            if (IsComponentAllowed(type, false) == false)
                return;

            if (_allowedComponents.Contains(type))
                return;
            
            _allowedComponents.Add(type);
        }

        public void AllowComponent<T>()
        {
            AllowComponent(typeof(T));
        }

        public bool IsMonoBehaviourMember(string memberName)
        {
            Init();
            return _monoBehaviourMembers.Contains(memberName);
        }
        
        public static List<(PropertyInfo prop, TAttr attr)> GetPropertiesWithAttribute<TAttr>(Type type) where TAttr : Attribute
        {
            var res = new List<(PropertyInfo, TAttr)>();

            foreach (var property in type.GetProperties())
            {
                var attr = property.GetCustomAttribute<TAttr>();
                if (attr != null)
                {
                    res.Add((property, attr));
                }
            }

            return res;
        }
    }
}

