using System;
using System.Collections.Generic;
using System.Reflection;
using Rundo.Core.Utils;
using Rundo.RuntimeEditor.Factory;
using Rundo.RuntimeEditor.Tools;
using UnityEngine;

namespace Rundo.RuntimeEditor.Behaviours.UI
{
    /// <summary>
    /// Default inspector for drawing primitive/object values (so everything excluded list)
    /// </summary>
    public class DefaultDataInspectorBehaviour : InspectorWindowElementBehaviour
    {
        [SerializeField] private Transform _content;
        
        private static readonly Dictionary<Type, object> RedrawInspectorTypeCache = new Dictionary<Type, object>();
        
        protected override Transform GetUiDataMapperDefaultContent => _content;

        private Type _currentType;
        private List<(string, GameObject)> _defaultElements = new List<(string, GameObject)>();
        
        protected override void MapUi() {}

        protected override void OnDataSetInternal()
        {
            base.OnDataSetInternal();
            Rebuild();
        }

        private void Rebuild()
        {
            if (HasData() == false)
            {
                ClearUiElements();
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);

            _currentType = UiDataMapper.DataHandler.GetDataType();

            DrawInspector();
        }

        private void ClearUiElements()
        {
            UiDataMapper.ClearElements();
        }

        public void DrawInspector()
        {
            ClearUiElements();
            
            var isReadOnly = false;

            if (UiDataMapper.DataHandler.ReadOnlyProvider != null)
                isReadOnly = UiDataMapper.DataHandler.ReadOnlyProvider.Invoke();

            var name = UiDataMapper.DataHandler.GetLastMemberName();
            var uiDataMapperElement = UiDataMapper.CreatePrimitive(_currentType, StringUtils.ToPascalCase(name), name);

            // ui element exist for the current type
            if (uiDataMapperElement != null)
            {
                if (isReadOnly)
                    uiDataMapperElement.SetReadOnly();
            }
            else
            {
                foreach (var memberInfo in RundoEngine.DataSerializer.GetSerializableMembers(_currentType))
                {
                    var data = UiDataMapper.DataHandler.Copy();
                    data.AddMember(memberInfo.Name);

                    var memberType = ReflectionUtils.GetMemberType(memberInfo);
                
                    // render list only when data set contains only single data
                    if (ReflectionUtils.IsList(memberType))
                        if (UiDataMapper.DataHandler.GetRootData().Count > 1)
                            continue;
                
                    // handle primitives
                    uiDataMapperElement = UiDataMapper.CreatePrimitive(memberType, StringUtils.ToPascalCase(memberInfo.Name), memberInfo.Name);
                    if (uiDataMapperElement != null)
                    {
                        if (isReadOnly)
                            uiDataMapperElement.SetReadOnly();
                        uiDataMapperElement.BindDynamic(memberInfo);
                        continue;
                    }
                    
                    _defaultElements.Add((memberInfo.Name, UiFactory.DrawInspector(data, _content)));
                }
            }
                    
            GetComponentInParent<CanvasRebuilderBehaviour>()?.Rebuild();
        }

        protected override void RedrawInternal()
        {
            base.RedrawInternal();
            
            // check if the type declares static method 'EDITOR_OverrideDefaultInspector'
            var onRedrawInspectorMethod = _currentType.GetMethod(nameof(IDefaultInspectorOverride.OnDefaultInspectorRedraw), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            if (onRedrawInspectorMethod != null)
            {
                if (RedrawInspectorTypeCache.TryGetValue(_currentType, out var redrawInstance) == false)
                {
                    redrawInstance = RundoEngine.DataFactory.Instantiate(_currentType);
                    RedrawInspectorTypeCache[_currentType] = redrawInstance;
                }
                
                onRedrawInspectorMethod.Invoke(RedrawInspectorTypeCache[_currentType], new object[] { this });
            }
        }

        public GameObject GetElementInstanceByName(string name)
        {
            foreach (var it in _defaultElements)
                if (it.Item1 == name)
                    return it.Item2;

            return UiDataMapper.GetElementInstanceByName(name)?.GameObject;
        }
    }
    
}

