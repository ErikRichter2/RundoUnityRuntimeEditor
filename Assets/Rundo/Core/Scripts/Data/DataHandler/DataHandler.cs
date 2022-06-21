using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Rundo.Core.Commands;
using Rundo.Core.Events;
using Rundo.Core.Utils;
using UnityEngine.Assertions;

namespace Rundo.Core.Data
{
    public class DataHandler
    {
        public struct PathData
        {
            public string Name;
            public int ListIndex;
            public bool IsListIndex;
        }
        
        public Func<bool> ReadOnlyProvider;

        private List<object> _rootData = new List<object>();
        private List<PathData> _pathFromRoot = new List<PathData>();
        private Action _onDataChange;
        private readonly List<IEventListener> _listeners = new List<IEventListener>();
        
        public readonly ICommandProcessor CommandProcessor;

        public DataHandler(ICommandProcessor commandProcessor)
        {
            CommandProcessor = commandProcessor;
        }

        public void OnRootDataChange(Action onDataChange)
        {
            _onDataChange = onDataChange;
            RegisterListeners();
        }

        private void RegisterListeners()
        {
            RemoveListeners();

            if (_onDataChange != null)
                foreach (var it in _rootData)
                    _listeners.Add(CommandProcessor.EventDispatcher.Register(OnData, it));
        }
        
        private void OnData(object data)
        {
            if (_rootData.Contains(data))
                _onDataChange?.Invoke();
        }

        public void RemoveListeners()
        {
            foreach (var it in _listeners)
                it.Remove();
                
            _listeners.Clear();
        }

        public List<object> GetRootData()
        {
            return _rootData;
        }
        
        public List<T> GetRootDataTyped<T>()
        {
            var res = new List<T>();

            foreach (var it in _rootData)
                if (it is T t)
                    res.Add(t);
            
            return res;
        }

        public void ClearRootData()
        {
            _rootData?.Clear();
            RemoveListeners();
        }
        
        public void SetRootData(object data)
        {
            _rootData.Add(data);
            RegisterListeners();
        }

        public void SetRootDataList(List<object> data)
        {
            _rootData.AddRange(data);
            RegisterListeners();
        }

        public DataHandler Copy()
        {
            var res = new DataHandler(CommandProcessor);
            res._rootData = _rootData;
            res._pathFromRoot = new List<PathData>(_pathFromRoot);
            return res;
        }

        public DataHandler AddListElement(int listIndex)
        {
            _pathFromRoot.Add(new PathData
            {
                ListIndex = listIndex,
                IsListIndex = true,
            });

            return this;
        }

        public DataHandler AddMember(string memberName)
        {
            _pathFromRoot.Add(new PathData
            {
                Name = memberName, 
            });

            return this;
        }

        public void SetValue(IDataSetValue value, params MemberInfo[] childMembers)
        {
            Assert.IsNotNull(CommandProcessor);
            
            for (var i = 0; i < _rootData.Count; ++i)
            {
                var data = GetDataForValueSet(value.ValueDynamic, i, childMembers);

                if (value.ListIndex != null)
                {
                    var list = (IList)ReflectionUtils.GetValue(data.ParentReferenceObject, data.ParentReferenceMember);
                    var command = new SetValueToListCommand(list, value.ListIndex.Value, value.ValueDynamic);
                    command.IgnoreUndoRedo = value.IgnoreUndoRedo;
                    command.AddDispatcherData(_rootData[i]);
                    CommandProcessor.Process(command);
                }
                else if (data.IsList)
                {
                    var command = new SetValueToListCommand((IList)data.ParentReferenceObject, data.ListIndex, data.Value);
                    command.IgnoreUndoRedo = value.IgnoreUndoRedo;
                    command.AddDispatcherData(_rootData[i]);
                    CommandProcessor.Process(command);
                }
                else
                {
                    var command = new SetValueToMemberCommand(data.ParentReferenceObject, data.ParentReferenceMember, data.Value);
                    command.IgnoreUndoRedo = value.IgnoreUndoRedo;
                    command.AddDispatcherData(_rootData[i]);
                    CommandProcessor.Process(command);
                }
            }
        }

        public void SetValueWithoutCommand(object value, MemberInfo childMember)
        {
            for (var i = 0; i < _rootData.Count; ++i)
            {
                var data = GetDataForValueSet(value, i, childMember);
                if (data.IsList)
                {
                    ((IList)data.ParentReferenceObject)[data.ListIndex] = data.Value;
                }
                else
                {
                    ReflectionUtils.SetValue(data.ParentReferenceObject, data.ParentReferenceMember, data.Value);
                }
            }
        }

        private DataHandlerBeforeSet GetDataForValueSet(object value, int dataIndex, params MemberInfo[] childMembers)
        {
            var obj = _rootData[dataIndex];
            Assert.IsNotNull(obj);

            var refObject = obj;
            var refObjectListIndex = 0;
            MemberInfo refObjectChildMember = null;
            MemberInfo memberInfo = null;

            List<ValueObjectPath> valueObjectPath = null;

            var dataPath = new List<PathData>(_pathFromRoot);
            if (childMembers != null)
                foreach (var childMember in childMembers)
                    dataPath.Add(new PathData{Name = childMember.Name});

            for (var i = 0; i < dataPath.Count; ++i)
            {
                if (dataPath[i].IsListIndex)
                {
                    refObjectListIndex = dataPath[i].ListIndex;
                    obj = ((IList)obj)[refObjectListIndex];
                }
                else
                {
                    memberInfo = ReflectionUtils.GetMemberInfo(obj.GetType(), dataPath[i].Name);
                    Assert.IsNotNull(memberInfo);

                    if (valueObjectPath != null && valueObjectPath.Count > 0)
                        valueObjectPath[valueObjectPath.Count - 1].Child = memberInfo;

                    if (i < dataPath.Count - 1)
                        obj = ReflectionUtils.GetValue(obj, memberInfo);
                }


                if (i < dataPath.Count - 1)
                {
                    if (obj.GetType().IsClass)
                    {
                        refObject = obj;
                        valueObjectPath?.Clear();
                    }
                    else
                    {
                        if (valueObjectPath == null || valueObjectPath.Count == 0)
                            refObjectChildMember = memberInfo;
                        
                        valueObjectPath ??= new List<ValueObjectPath>();
                        valueObjectPath.Add(new ValueObjectPath{Value = obj});
                    }
                }
            }

            if (valueObjectPath == null)
            {
                return new DataHandlerBeforeSet
                {
                    ParentReferenceObject = refObject,
                    ParentReferenceMember = memberInfo,
                    Value = value
                };
            }
            else
            {
                var lastIndex = valueObjectPath.Count - 1;
                var valueObjectPathItem = valueObjectPath[lastIndex];
                var valueObjectPathItemValue = valueObjectPathItem.Value;
                    
                ReflectionUtils.SetValue(valueObjectPathItemValue, valueObjectPath[lastIndex].Child, value);

                valueObjectPathItem.Value = valueObjectPathItemValue;
                valueObjectPath[lastIndex] = valueObjectPathItem;

                for (var i = valueObjectPath.Count - 2; i >= 0; --i)
                {
                    valueObjectPathItem = valueObjectPath[i];
                    valueObjectPathItemValue = valueObjectPathItem.Value;
                    
                    ReflectionUtils.SetValue(valueObjectPathItemValue, valueObjectPath[i].Child, valueObjectPath[i + 1].Value);

                    valueObjectPathItem.Value = valueObjectPathItemValue;
                    valueObjectPath[i] = valueObjectPathItem;
                }

                if (ReflectionUtils.IsList(refObject.GetType()))
                {
                    return new DataHandlerBeforeSet
                    {
                        ParentReferenceObject = refObject,
                        Value = valueObjectPath[0].Value,
                        ListIndex = refObjectListIndex,
                        IsList = true,
                    };
                }
                else
                {
                    return new DataHandlerBeforeSet
                    {
                        ParentReferenceObject = refObject,
                        ParentReferenceMember = refObjectChildMember,
                        Value = valueObjectPath[0].Value
                    };
                }
            }
        }

        public DataHandlerValue GetValue(int listIndex)
        {
            var firstList = (IList)GetValueInternal(0, null);
            if (firstList == null)
                throw new Exception($"Use GetValue(int listIndex) only when DataHandler is pointing to a IList type");
            
            if (_rootData.Count == 1)
                return new DataHandlerValue { Value = firstList[listIndex] };

            object firstValue = default;
            string firstValueSerialized = null;
            
            for (var i = 0; i < _rootData.Count; ++i)
            {
                var list = (IList)GetValueInternal(i, null);

                if (i == 0)
                {
                    firstValue = list[listIndex];
                }
                else
                {
                    var value = list[listIndex];
                    
                    if (firstValue.GetType() != value.GetType())
                        return new DataHandlerValue { IsUndefined = true };

                    if (firstValue.GetType().IsClass)
                    {
                        if (string.IsNullOrEmpty(firstValueSerialized))
                            firstValueSerialized = RundoEngine.DataSerializer.SerializeObject(firstValue);
                        var valueSerialized = RundoEngine.DataSerializer.SerializeObject(value);
                        if (firstValueSerialized != valueSerialized)
                            return new DataHandlerValue { IsUndefined = true };
                    }
                    else if (firstValue.GetType().IsValueType)
                    {
                        if (firstValue.Equals(value) == false)
                            return new DataHandlerValue { IsUndefined = true };
                    }
                }
            }

            return new DataHandlerValue { Value = firstValue };
        }

        private DataHandlerValue GetValueInternal(params MemberInfo[] childMembers)
        {
            if (_rootData.Count == 1)
                return new DataHandlerValue { Value = GetValueInternal(0, childMembers) };

            object firstValue = default;
            string firstValueSerialized = null;
            for (var i = 0; i < _rootData.Count; ++i)
            {
                var value = GetValueInternal(i, childMembers);

                if (i == 0)
                {
                    firstValue = value;
                }
                else
                {
                    if (firstValue.GetType() != value.GetType())
                        return new DataHandlerValue { IsUndefined = true };
                    
                    if (firstValue.GetType().IsClass)
                    {
                        if (string.IsNullOrEmpty(firstValueSerialized))
                            firstValueSerialized = RundoEngine.DataSerializer.SerializeObject(firstValue);
                        var valueSerialized = RundoEngine.DataSerializer.SerializeObject(value);
                        if (firstValueSerialized != valueSerialized)
                            return new DataHandlerValue { IsUndefined = true };
                    }
                    else if (firstValue.GetType().IsValueType)
                    {
                        if (firstValue.Equals(value) == false)
                            return new DataHandlerValue { IsUndefined = true };
                    }
                }
            }

            return new DataHandlerValue { Value = firstValue };
        }

        public DataHandlerValue GetValue(string childMemberName)
        {
            var type = GetDataType();
            var memberInfo = ReflectionUtils.GetMemberInfo(type, childMemberName);
            return GetValue(memberInfo);
        }

        public DataHandlerValue GetValue(params MemberInfo[] childMembers)
        {
            return GetValueInternal(childMembers);
        }

        public DataHandlerValue GetValue()
        {
            return GetValueInternal();
        }

        public string GetLastMemberName()
        {
            if (_rootData.Count <= 0)
                return null;
            if (_pathFromRoot.Count <= 0)
                return null;

            return _pathFromRoot[_pathFromRoot.Count - 1].Name;
        }

        public Type GetDataType(string memberName)
        {
            if (_rootData.Count <= 0)
                return null;

            var obj = _rootData[0];
            var res = obj.GetType();

            var dataPath = new List<PathData>(_pathFromRoot);
            if (string.IsNullOrEmpty(memberName) == false)
                dataPath.Add(new PathData{Name = memberName});
            for (var i = 0; i < dataPath.Count; ++i)
            {
                if (dataPath[i].IsListIndex)
                {
                    obj = ((IList)obj)[dataPath[i].ListIndex];
                    continue;
                }

                var memberInfo = ReflectionUtils.GetMemberInfo(obj.GetType(), dataPath[i].Name);
                obj = ReflectionUtils.GetValue(obj, memberInfo);
            }
            
            return obj.GetType();
        }

        public Type GetDataType()
        {
            return GetDataType(null);
        }

        public T GetRawValue<T>(int dataIndex)
        {
            return (T)GetValueInternal(dataIndex, null);
        }

        private object GetValueInternal(int dataIndex, params MemberInfo[] childMembers)
        {
            var obj = _rootData[dataIndex];
            Assert.IsNotNull(obj);

            var dataPath = new List<PathData>(_pathFromRoot);
            
            if (childMembers != null)
                foreach (var childMember in childMembers)
                    dataPath.Add(new PathData{Name = childMember.Name});

            for (var i = 0; i < dataPath.Count; ++i)
            {
                if (dataPath[i].IsListIndex)
                {
                    obj = ((IList)obj)[dataPath[i].ListIndex];
                    continue;
                }
                
                var memberInfo = ReflectionUtils.GetMemberInfo(obj.GetType(), dataPath[i].Name);
                Assert.IsNotNull(memberInfo);
                obj = ReflectionUtils.GetValue(obj, memberInfo);

                if (obj == null)
                {
                    if (ReflectionUtils.GetMemberType(memberInfo) == typeof(string))
                        return "";
                    return default;
                }
            }
            
            return obj;
        }
    }

    public struct DataHandlerBeforeSet
    {
        public object ParentReferenceObject;
        public MemberInfo ParentReferenceMember;
        public object Value;
        public int ListIndex;
        public bool IsList;
    }

    public struct DataHandlerValue
    {
        public object Value;
        public bool IsUndefined;
    }

    public class ValueObjectPath
    {
        public object Value;
        public MemberInfo Child;
    }



}
