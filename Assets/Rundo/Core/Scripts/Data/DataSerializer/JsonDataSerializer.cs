using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rundo.Core.Utils;
using UnityEngine;
using Formatting = Newtonsoft.Json.Formatting;
using Object = UnityEngine.Object;

namespace Rundo.Core.Data
{
    public class JsonDataSerializer
    {
        private static readonly Dictionary<Type, List<MemberInfo>> SerializableMembersCache =
            new Dictionary<Type, List<MemberInfo>>();

        private readonly JsonSerializerSettings _jsonSerializerSettings;

        private readonly List<JsonConverter> _readConverters = new List<JsonConverter>()
        {
            new StronglyTypedJsonConverter(),
            new DataReferenceReadJsonConverter(),
            new DataCollectionReadJsonConverter(),
            new PolymorphismInstanceReadJsonConverter(),
        };

        public JsonDataSerializer()
        {
            _jsonSerializerSettings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ObjectCreationHandling = ObjectCreationHandling.Replace,
                ContractResolver = new MonoBehaviourSerializerContractResolver(),
            };
        }

        private List<JsonConverter> GetWriteConverters()
        {
            return new List<JsonConverter>
            {
                new LoopFixForUnityNativeStructsWriteJsonConverter(),
                new StronglyTypedJsonConverter(),
                new DataCollectionWriteJsonConverter(),
                new DataReferenceWriteJsonConverter(),
            };
        }
        
        private List<JsonConverter> GetReadConverters()
        {
            return new List<JsonConverter>
            {
                new StronglyTypedJsonConverter(),
                new DataReferenceReadJsonConverter(),
                new DataCollectionReadJsonConverter(),
                new PolymorphismInstanceReadJsonConverter(),
            };
        }

        public void AddDefaultReadConverter(JsonConverter jsonConverter, int priority = -1)
        {
            if (priority >= 0)
                _readConverters.Insert(priority, jsonConverter);
            else
                _readConverters.Add(jsonConverter);
        }
        
        public string SerializeObject(object obj)
        {
            return SerializeObject(obj, Formatting.None, null);
        }

        public void PopulateObject(string data, object obj)
        {
            throw new NotImplementedException();
        }

        public string SerializeObject(object obj, Formatting formatting, params JsonConverter[] converters)
        {
            _jsonSerializerSettings.Converters.Clear();
            
            if (converters != null)
                foreach (var converter in converters)
                    _jsonSerializerSettings.Converters.Add(converter);

            foreach (var converter in GetWriteConverters())
                _jsonSerializerSettings.Converters.Add(converter);
            
            var serialized = JsonConvert.SerializeObject(obj, formatting, _jsonSerializerSettings);

            return serialized;
        }

        public string SerializeObject(object obj, Formatting formatting)
        {
            return SerializeObject(obj, formatting, null);
        }
        
        public string SerializeObject(object obj, params JsonConverter[] converters)
        {
            return SerializeObject(obj, Formatting.None, converters);
        }
        
        public T DeserializeObject<T>(string data)
        {
            return (T)DeserializeObject(data, typeof(T));
        }

        public object DeserializeObject(string data, Type type, params JsonConverter[] converters)
        {
            return DeserializeObjectInternal(data, type, false, converters);
        }

        private object DeserializeObjectInternal(string data, Type type, bool isClone, params JsonConverter[] converters)
        {
            _jsonSerializerSettings.Converters.Clear();
            
            if (converters != null)
                foreach (var converter in converters)
                    _jsonSerializerSettings.Converters.Add(converter);
            
            foreach (var converter in GetReadConverters())
                _jsonSerializerSettings.Converters.Add(converter);
            
            var instance = JsonConvert.DeserializeObject(data, type, _jsonSerializerSettings);
            
            DeserializationPostprocess(instance, isClone);
            
            return instance;
        }

        public object CreateInstance(Type type)
        {
            if (type.IsPrimitive || ReflectionUtils.IsList(type))
                return RundoEngine.DataFactory.Instantiate(type);

            return RundoEngine.DataSerializer.DeserializeObject("{}", type);
        }

        public object DeserializeObject(string data, Type type)
        {
            return DeserializeObject(data, type, null);
        }

        public void PopulateObject(object fromObj, object toObj)
        {
            Populate(SerializeObject(fromObj), toObj);
        }

        public void Populate(JsonSerializer serializer, JObject jObject, object dataInstance)
        {
            if (dataInstance is IDataSerializerPopulateHandler dataSerializerPopulateHandler)
                dataSerializerPopulateHandler.Populate(jObject, serializer);
            else
                serializer.Populate(jObject.CreateReader(), dataInstance);
        }

        public void Populate(string data, object obj)
        {
            _jsonSerializerSettings.Converters.Clear();
            
            foreach (var converter in GetReadConverters())
                _jsonSerializerSettings.Converters.Add(converter);

            if (obj is IDataSerializerPopulateHandler dataSerializerPopulateHandler)
                dataSerializerPopulateHandler.Populate(data, _jsonSerializerSettings);
            else
                JsonConvert.PopulateObject(data, obj, _jsonSerializerSettings);
            
            DeserializationPostprocess(obj, false);
        }

        public T Clone<T>(T obj)
        {
            return (T)Clone(obj.GetType(), SerializeObject(obj));
        }

        public object Clone(Type type, string serializedObject)
        {
            return DeserializeObjectInternal(serializedObject, type, true);
        }

        public T Copy<T>(T obj)
        {
            return (T)DeserializeObject(SerializeObject(obj), obj.GetType());
        }

        public List<MemberInfo> GetSerializableMembers(Type type)
        {
            if (SerializableMembersCache.TryGetValue(type, out var res))
                return res;
        
            res = new List<MemberInfo>();
        
            foreach (var memberInfo in type.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                if (IsMemberSerializable(type, memberInfo))
                    res.Add(memberInfo);

            SerializableMembersCache[type] = res;
        
            return res;
        }

        private static bool IsMemberSerializable(Type memberParent, MemberInfo memberInfo)
        {
            if (memberInfo.MemberType != MemberTypes.Field &&
                memberInfo.MemberType != MemberTypes.Property)
                return false;
            if (memberInfo.GetCustomAttribute<JsonIgnoreAttribute>() != null)
                return false;
            if (memberInfo.GetCustomAttribute<JsonPropertyAttribute>() != null)
                return true;
            if (typeof(Object).IsAssignableFrom(memberParent))
                if (RundoEngine.ReflectionService.IsMonoBehaviourMember(memberInfo.Name))
                    return false;

            var memberSerialization = MemberSerialization.OptOut;

            var jsonObjectAttr = memberParent.GetCustomAttribute<JsonObjectAttribute>();
            if (jsonObjectAttr != null)
                memberSerialization = jsonObjectAttr.MemberSerialization;

            if (memberSerialization == MemberSerialization.OptIn)
                return false;
            if (memberSerialization == MemberSerialization.Fields && memberInfo.MemberType != MemberTypes.Field)
                return false;

            bool isPublic = false;

            if (memberInfo is FieldInfo fieldInfo)
            {
                if (fieldInfo.IsStatic || fieldInfo.FieldType.IsArray)
                    return false;
            
                isPublic = fieldInfo.IsPublic;
            }
            else if (memberInfo is PropertyInfo propertyInfo)
            {
                if (propertyInfo.PropertyType.IsArray)
                    return false;

                var accessors = propertyInfo.GetAccessors(true);
                foreach (var it in accessors)
                {
                    if (it.ReturnType == typeof(void))
                    {
                        isPublic = it.IsPublic;
                        break;
                    }
                }
            }

            return isPublic;
        }
        
        private static bool CanPostprocessType(Type type)
        {
            if (type.IsPrimitive)
                return false;
            if (type.IsEnum)
                return false;
            if (type.IsArray)
                return false;
            if (type == typeof(string))
                return false;

            return true;
        }

        private static bool CanPostprocess(object obj)
        {
            if (obj == null)
                return false;
            if (CanPostprocessType(obj.GetType()) == false)
                return false;
            if (obj is string)
                return false;

            return true;
        }
    
        private void PostprocessInternal(object obj, IParentable parent)
        {
            if (CanPostprocess(obj) == false)
                return;

            if (obj is IDataSerializerPostProcessHandler dataSerializerPostProcessHandler)
                dataSerializerPostProcessHandler.PostProcessDataSerialization();

            if (obj is IParentable parentable)
            {
                parentable.SetParent(parent);
                parent = parentable;
            }
            else
            {
                parent = null;
            }

            if (ReflectionUtils.IsList(obj.GetType()))
            {
                foreach (var item in (IList)obj)
                    PostprocessInternal(item, parent);
                return;
            }
            
            foreach (var member in GetSerializableMembers(obj.GetType()))
            {
                var memberType = ReflectionUtils.GetMemberType(member);
            
                if (CanPostprocessType(memberType) == false)
                    continue;

                if (typeof(IEnumerable).IsAssignableFrom(memberType))
                {
                    var enumerable = (IEnumerable)ReflectionUtils.GetValue(obj, member);
                    if (enumerable != null)
                    {
                        var enumerableParentable = enumerable as IParentable;
                        enumerableParentable?.SetParent(parent);
                        foreach (var it in enumerable)
                            PostprocessInternal(it, enumerableParentable);
                    }
                }
                else
                {
                    PostprocessInternal(ReflectionUtils.GetValue(obj, member), parent);
                }
            }
        }

        private void DeserializationPostprocess(object obj, bool isClone)
        {
            IParentable parent = null;
            if (obj is IParentable parentable)
                parent = parentable.Parent;
            
            // when cloning replace guids marked with UniqueGuid attribute
            if (isClone)
                ReplaceUniqueGuids(obj);
        
            PostprocessInternal(obj, parent);
        }

        private void ReplaceUniqueGuidsPass(object obj, bool isFirstPass, Dictionary<IGuid, IGuid> guidMap)
        {
            var queue = new Queue<object>();
            queue.Enqueue(obj);

            while (queue.Count > 0)
            {
                obj = queue.Dequeue();
                
                if (obj == null)
                    continue;
                
                if (CanPostprocess(obj.GetType()) == false)
                    continue;

                if (ReflectionUtils.IsList(obj.GetType()))
                {
                    QueueUtils.EnqueueList(queue, (IEnumerable)obj);
                    continue;
                }

                foreach (var member in GetSerializableMembers(obj.GetType()))
                {
                    var memberType = ReflectionUtils.GetMemberType(member);

                    if (typeof(IGuid).IsAssignableFrom(memberType))
                    {
                        var guid = (IGuid)ReflectionUtils.GetValue(obj, member);
                        if (guid.IsNullOrEmpty == false)
                        {
                            if (isFirstPass)
                            {
                                if (ReflectionUtils.HasAttribute<GenerateNewGuidWhenCloneAttribute>(member))
                                    guidMap[guid] = guid.ReturnNewGUID();
                            }
                            else
                            {
                                if (guidMap.TryGetValue(guid, out var generatedGuid))
                                {
                                    guid.SetGUID(generatedGuid.ToStringRawValue());
                                    ReflectionUtils.SetValue(obj, member, guid);
                                }
                            }
                        }
                    }

                    if (CanPostprocessType(ReflectionUtils.GetMemberType(member)) == false)
                        continue;

                    if (typeof(IEnumerable).IsAssignableFrom(memberType))
                        QueueUtils.EnqueueList(queue, (IEnumerable)ReflectionUtils.GetValue(obj, member));
                    else
                        queue.Enqueue(ReflectionUtils.GetValue(obj, member));
                }
            }
        }
        
        private void ReplaceUniqueGuids(object obj)
        {
            var guidMap = new Dictionary<IGuid, IGuid>();
            ReplaceUniqueGuidsPass(obj, true, guidMap);
            ReplaceUniqueGuidsPass(obj, false, guidMap);
        }
    }
}