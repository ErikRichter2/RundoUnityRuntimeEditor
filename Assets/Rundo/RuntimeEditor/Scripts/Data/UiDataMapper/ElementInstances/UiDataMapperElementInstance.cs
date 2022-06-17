using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Rundo.Core.Data;
using Rundo.RuntimeEditor.Behaviours;
using Rundo.Core.Utils;
using UnityEngine;

namespace Rundo.RuntimeEditor.Data.UiDataMapper
{
    public struct ValueConverter
    {
        public Func<object, object> Converter;

        public object Convert(object data)
        {
            return Converter.Invoke(data);
        }
    }

    /// <summary>
    /// Provides a connection between a single UI element behaviour and a UiDataMapper engine.
    /// </summary>
    public abstract class UiDataMapperElementInstance
    {
        protected ValueConverter? _convertValueBeforeSetToData;
        protected ValueConverter? _convertValueBeforeSetToUi;

        public readonly UiDataMapper UiDataMapper;
        protected readonly IUiDataMapperElementBehaviour _elementBehaviour;
        
        protected Action<UiDataMapperElementValue<object>> _fromUiToDataCustom;
        protected Func<DataHandlerValue> _fromDataToUiCustom;
        
        protected int? _listIndex;

        protected UiDataMapperElementInstance(UiDataMapper uiDataMapper, IUiDataMapperElementBehaviour elementBehaviour)
        {
            UiDataMapper = uiDataMapper;
            _elementBehaviour = elementBehaviour;
        }
        
        public string Name { get; private set; }

        public void SetName(string name)
        {
            Name = name;
        }

        public bool IsElement(IUiDataMapperElementBehaviour elementBehaviour)
        {
            return _elementBehaviour == elementBehaviour;
        }
        
        public abstract void Redraw();

        public void BindListIndex(int listIndex)
        {
            _listIndex = listIndex;
/*            
            _fromUiToDataDynamic = value =>
            {
                var rawValue = value.Value;
                var list = (IList)_uiDataMapper.DataHandler.GetValue(0, null);
                var command = new SetValueToListCommand(list, value.MetaData.ElementIndex, rawValue);
                command.AddDispatcherData(_uiDataMapper.DataHandler.Data[0]);
                command.Process();
            };
            
            _fromDataToUiDynamic = data =>
            {
                var list = data.Data as IList;
                var value = list[data.MetaData.ElementIndex];
                if (value is Enum)
                    return value.ToString();
                if (value is ITEnum itEnum)
                    return itEnum.ToStringRawValue();
                return value;
            };
            */
        }

        protected bool _isReadOnly;
        protected MemberInfo[] _childMembers;

        public void SetReadOnly()
        {
            _isReadOnly = true;
        }

        public GameObject GameObject => _elementBehaviour.GameObject;

        protected void BindCustomDynamic(
            Action<UiDataMapperElementValue<object>> fromUiToData,
            Func<UiDataMapperElementData<object>, object> fromDataToUi)
        {
            _fromUiToDataCustom = value =>
            {
                if (_isReadOnly)
                    return;

                fromUiToData.Invoke(value);
            };
            
            _fromDataToUiCustom = () =>
            {
                var tmpData = new List<object>();
                for (var i = 0; i < UiDataMapper.DataHandler.GetRootData().Count; ++i)
                    tmpData.Add(fromDataToUi.Invoke(
                        new UiDataMapperElementData<object>(
                            UiDataMapper.DataHandler.GetRootData()[i], 
                            new UiDataMapperElementDataMetaData{DataIndex = i, ListIndex = _listIndex})));

                var dataHandler = new DataHandler(null);
                dataHandler.SetRootDataList(tmpData);
                return dataHandler.GetValue();
            };
        }

        public void BindDynamic(params MemberInfo[] memberInfos)
        {
            _childMembers = memberInfos;
        }

        protected object ConvertValueBeforeSetToData(object value)
        {
            if (_convertValueBeforeSetToData != null)
                return _convertValueBeforeSetToData.Value.Convert(value);
            return value;
        }

        protected object ConvertValueBeforeSetToUi(object value)
        {
            if (_convertValueBeforeSetToUi != null)
                return _convertValueBeforeSetToUi.Value.Convert(value);
            return value;
        }
    }
    
    public class UiDataMapperButtonInstance : UiDataMapperElementInstance
    {
        public UiDataMapperButtonInstance(UiDataMapperButtonElementBehaviour elementBehaviour) : base(null, elementBehaviour) {}
        
        public void OnClick(Action onClick)
        {
            var button = (UiDataMapperButtonElementBehaviour)_elementBehaviour;
            button.OnClick(() =>
            {
                if (_isReadOnly)
                    return;
                onClick?.Invoke();
            });
        }

        public override void Redraw() {}
    }
    
    public interface IUiDataMapperElementInstanceWithValue {}

    /**
     * Maps a single UI element behaviour to a dataset
     */
    public class UiDataMapperElementInstance<TValue> : UiDataMapperElementInstance, IUiDataMapperElementInstanceWithValue
    {
        public UiDataMapperElementInstance(UiDataMapper uiDataMapper, IUiDataMapperElementWithValueBehaviour elementBehaviour) : base(uiDataMapper, (IUiDataMapperElementBehaviour)elementBehaviour)
        {
            if (elementBehaviour == null)
                throw new Exception(
                    $"Cannot use dynamic constructor when element is not of type {nameof(IUiDataMapperElementWithValueBehaviour)}");
            
            elementBehaviour.OnSubmitDynamicValue(typeof(TValue), (dynamicValue) =>
            {
                if (dynamicValue.Value is TValue value)
                    OnElementBehaviourCommandSubmit(new UiDataMapperElementValue<TValue>(value, dynamicValue.MetaData));
                else
                    throw new Exception(
                        $"Provided dynamic value has type {dynamicValue.GetType().Name} but expects type {nameof(TValue)}");
            });
        }

        public UiDataMapperElementInstance(UiDataMapper uiDataMapper, IUiDataMapperElementBehaviour<TValue> elementBehaviour) : base(uiDataMapper, elementBehaviour)
        {
            elementBehaviour.OnSubmit(OnElementBehaviourCommandSubmit);
        }

        private void OnElementBehaviourCommandSubmit(UiDataMapperElementValue<TValue> value)
        {
            value.MetaData.ListIndex = _listIndex;

            if (_fromUiToDataCustom != null)
            {
                for (var i = 0; i < UiDataMapper.DataHandler.GetRootData().Count; ++i)
                {
                    value.MetaData.DataIndex = i;
                    _fromUiToDataCustom?.Invoke(value.ToDynamic());
                }
            }
            else
            {
                if (_listIndex != null)
                {
                    var valueDynamic = value.ToDynamic();
                    valueDynamic.Value = ConvertValueBeforeSetToData(valueDynamic.Value);
                    UiDataMapper.DataHandler.SetValue(valueDynamic, _childMembers);
                }
                else
                {
                    var valueDynamic = value.ToDynamic();
                    valueDynamic.Value = ConvertValueBeforeSetToData(valueDynamic.Value);
                    UiDataMapper.DataHandler.SetValue(valueDynamic, _childMembers);
                }
            }
            
            Redraw();
        }

        public override void Redraw()
        {
            if (UiDataMapper.DataHandler.GetRootData().Count == 0)
                return;
            
            if (_elementBehaviour is IUiDataMapperElementWithValueBehaviour elementWithValueBehaviour)
            {
                DataHandlerValue value;
                
                if (_fromDataToUiCustom != null)
                    value = _fromDataToUiCustom.Invoke();
                else if (_listIndex != null)
                    value = UiDataMapper.DataHandler.GetValue(_listIndex.Value);
                else
                    value = UiDataMapper.DataHandler.GetValue(_childMembers);
                
                if (value.IsUndefined == false)
                    value.Value = ConvertValueBeforeSetToUi(value.Value);
                
                elementWithValueBehaviour.SetValue(value);
            }
        }

        /**
         * Binds a field or property to this UI element mapper. Input member name must be a member of TData type.
         */
        public void Bind(params string[] memberNames)
        {
            var memberType = UiDataMapper.DataHandler.GetDataType();
            var memberInfos = new List<MemberInfo>();

            foreach (var memberName in memberNames)
            {
                var memberInfo = ReflectionUtils.GetMemberInfo(memberType, memberName);
                
                if (memberInfo == null)
                    throw new Exception($"Member {memberName} does not exist in the type {memberType.Name}");

                memberInfos.Add(memberInfo);
                memberType = ReflectionUtils.GetMemberType(memberInfo);
            }

            
            Bind(memberInfos.ToArray());
        }

        public UiDataMapperElementInstance<TValue> BindCustom<TData>(
            Action<TData, UiDataMapperElementValue<TValue>> fromUiToData,
            Func<UiDataMapperElementData<TData>, TValue> fromDataToUi)
        {
            BindCustomDynamic(
                (value) =>
                {
                    fromUiToData?.Invoke(
                        (TData)UiDataMapper.DataHandler.GetRootData()[value.DataIndex], 
                        new UiDataMapperElementValue<TValue>((TValue)value.Value, value.MetaData));
                },
                data => fromDataToUi != null 
                    ? fromDataToUi.Invoke(
                        new UiDataMapperElementData<TData>((TData)data.Data, data.MetaData)) 
                    : default(object));
            return this;
        }

        private UiDataMapperElementInstance<TValue> Bind(params MemberInfo[] memberInfos)
        {
            BindDynamic(memberInfos);
            return this;
        }
        
        public void SetValueConverter<TValueFrom, TValueTo>(
            Func<object, object> convertValueBeforeSetToData,
            Func<object, object> convertValueBeforeSetToUi)
        {
            _convertValueBeforeSetToData = new ValueConverter { Converter = convertValueBeforeSetToData };
            _convertValueBeforeSetToUi = new ValueConverter { Converter = convertValueBeforeSetToUi };
        }

    }
}
