using System;
using System.Globalization;
using System.Reflection;
using Newtonsoft.Json;
using Rundo.Core.Commands;

namespace Rundo.Core.Data
{
     /// <summary>
     /// Core class to represent json data that supports:<br/>
     /// - polymorphism<br/>
     /// - child -> parent hierarchy<br/>
     /// - implicit/explicit runtime-only model<br/>
     /// - using commands for data manipulation (modify, add, remove)<br/>
     /// - undo/redo system<br/><br/>
     /// Rules:<br/>
     /// - always keep constructor protected to avoid using new()<br/>
     /// - create new instances only by SerializedData.Instantiate<br/>
     /// - never instantiate serialized data in the commands !<br/>
     /// - modify command ignores serialized data list and references<br/>
     /// </summary>
     public class BaseData : IInstantiable, IParentable, ICommandProcessorGetter, IPolymorphismBase, IDataModelProvider
    {
        [JsonIgnore]
        public IParentable Parent { get; private set; }

        /// Runtime-only model to store runtime-only values, helpers functions...
        [JsonIgnore]
        private DataModel _model;
        
        protected BaseData() {}

        public virtual void OnInstantiated()
        {
            if (IsExplicitModelDeclaredAsSetter())
                GetOrCreateExplicitModel();
        }
        
        protected DataModel GetOrCreateExplicitModel()
        {
            if (_model != null)
                return _model;
            
            // get model type from lookup table
            var propertyInfo = RundoEngine.ReflectionService.GetSerializedDataExplicitModelPropertyInfoBySerializedDataType(GetType());
            if (propertyInfo != null)
            {
                _model = DataModel.Instantiate(propertyInfo.PropertyType, this);
                if (IsExplicitModelDeclaredAsSetter())
                    propertyInfo.SetValue(this, _model, BindingFlags.Public | BindingFlags.NonPublic,  (Binder) null, (object[]) null, (CultureInfo) null);
                _model.OnInstantiated();
            }

            return _model;
        }

        private bool IsExplicitModelDeclaredAsSetter()
        {
            var propertyInfo = RundoEngine.ReflectionService.GetSerializedDataExplicitModelPropertyInfoBySerializedDataType(GetType());
            if (propertyInfo != null)
            {
                var setMethod = propertyInfo.GetSetMethod();
                if (setMethod != null)
                    return true;
            }

            return false;
        }
        
        /// Returns either explicit model, or creates and returns implicit model if explicit does not exists.
        public IDataModel<T> GetModel<T>()
        {
            IDataModel<T> AssertModelTypeAndReturn()
            {
                if (_model is IDataModel<T> == false)
                    throw new Exception(
                        $"Requested model type {typeof(IDataModel<T>).Name} is not assignable from instantiated model type {_model.GetType()}");
                
                return (IDataModel<T>)_model;
            }

            if (_model != null)
                return AssertModelTypeAndReturn();

            GetOrCreateExplicitModel();
            if (_model != null)
                return AssertModelTypeAndReturn();
            
            // if model is not instantiated, attribute SerializedDataExplicitModelAttribute for this class has not been
            // found, so we need to create implicit model where the model type is defined as
            // SerializedDataModel<this.GetType()> (so the generic part of type is the final class of this object)
            _model = DataModel.Instantiate(typeof(DataModel<>).MakeGenericType(GetType()), this);
            _model.OnInstantiated();

            return AssertModelTypeAndReturn();
        }

        public virtual void SetParent(IParentable parent)
        {
            if (Parent == parent)
                return;
            
            if (parent == this)
                throw new Exception($"Cannot set itself as a parent");

            var tempParent = parent;
            while (tempParent != null)
            {
                if (tempParent == this)
                    throw new Exception($"Cannot set itself as a parent to an serialized data where this is child");

                tempParent = tempParent.Parent;
            }
            
            Parent = parent;
        }
        
        public T Instantiate<T>()
        {
            return (T)Instantiate(typeof(T));
        }
        
        public object Instantiate(Type type)
        {
            return RundoEngine.DataFactory.Instantiate(type, this);
        }

        public DataList<T> InstantiateList<T>()
        {
            var serializedDataList = (DataList<T>)RundoEngine.DataFactory.Instantiate(typeof(DataList<T>), this);
            return serializedDataList;
        }

        /// Returns parent of type T including this
        public T GetParentInHierarchy<T>()
        {
            if (this is T thisObj)
                return thisObj;
            
            var parent = Parent;
            
            while (parent != null)
            {
                if (parent is T parentObj)
                    return parentObj;

                if (parent is IParentable parentableParent)
                    parent = parentableParent.Parent;
                else
                    parent = null;
            }

            return default;
        }
        
        /// Returns model of parent of type T including this
        public IDataModel<T> GetParentModelInHierarchy<T>() where T: IDataModelProvider
        {
            if (this is T thisObj)
                return thisObj.GetModel<T>();
            
            var parent = Parent;
            
            while (parent != null)
            {
                if (parent is T parentObj)
                    return parentObj.GetModel<T>();

                if (parent is IParentable parentableParent)
                    parent = parentableParent.Parent;
                else
                    parent = null;
            }

            return default;
        }

        public virtual ICommandProcessor GetCommandProcessor()
        {
            if (this is ICommandProcessorProvider commandProcessorProvider)
                return commandProcessorProvider.CommandProcessor;
            
            return GetParentInHierarchy<ICommandProcessorProvider>()?.CommandProcessor;
        }
    }

}

