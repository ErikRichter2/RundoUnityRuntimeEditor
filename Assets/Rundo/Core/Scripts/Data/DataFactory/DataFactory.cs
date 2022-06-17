using System;
using System.Reflection;
using Newtonsoft.Json.Linq;
using UnityEngine.Assertions;

namespace Rundo.Core.Data
{
    public class DataFactory
    {
        public T Instantiate<T>(IParentable parent = null)
        {
            return (T)Instantiate(typeof(T), parent);
        }

        public object Instantiate(Type type, IParentable parent = null)
        {
            var instance = Activator.CreateInstance(type, true);
            
            if (parent != null)
                if (instance is IParentable parentable)
                    parentable.SetParent(parent);
            
            if (instance is IInstantiable instantiable)
                instantiable.OnInstantiated();

            return instance;
        }

        public object Instantiate(Type objectType, JObject jObject, IParentable parent)
        {
            if (typeof(IDataReference).IsAssignableFrom(objectType))
            {
                Assert.IsTrue(objectType.IsGenericType && objectType.GenericTypeArguments.Length == 1);
                return Instantiate(objectType.GenericTypeArguments[0], parent);
            }

            if (typeof(ICustomInstantiate).IsAssignableFrom(objectType))
            {
                var instantiateMethod = objectType.GetMethod(
                    "Instantiate", 
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy, 
                    (Binder) null,
                    new Type[] {typeof(JObject), typeof(IParentable)},
                    (ParameterModifier[]) null);
                
                if (instantiateMethod == null)
                    throw new Exception(
                        $"Type implementing {nameof(ICustomInstantiate)} must have a static method Instantiate(JObject, IParentable)");

                return instantiateMethod.Invoke(null, new object[] { jObject, parent });
            }

            if (jObject.TryGetValue(nameof(IDataTypeId._dataTypeId), out var dataTypeIdLiteral))
            {
                var valueStr = dataTypeIdLiteral.ToString();
                if (string.IsNullOrEmpty(valueStr) == false)
                {
                    var type = RundoEngine.ReflectionService.GetTypeBySerializedDataTypeId(valueStr);
                    if (type == null)
                        throw new Exception($"Type for ID: {valueStr} not found !");
                    return Instantiate(type, parent);
                }
            }
            
            return Instantiate(objectType, parent);
        }
    }

    public interface IDataTypeId
    {
        string _dataTypeId { get; }
    }

}

