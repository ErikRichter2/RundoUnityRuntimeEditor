using System;
using System.Collections.Generic;
using Rundo.Ui;
using Rundo.Core.Utils;
using Rundo.RuntimeEditor.Tools;
using UnityEngine;

namespace Rundo.RuntimeEditor.Behaviours
{
    public class DefaultDataInspectorBehaviour : InspectorBaseBehaviour
    {
        [SerializeField] private Transform _content;
        
        private static readonly Dictionary<Type, object> _redrawInspectorTypeCache = new Dictionary<Type, object>();
        
        protected override Transform GetUiDataMapperDefaultContent => _content;

        private Type _currentType;
        
        private readonly List<GameObject> _instantiatedUiElements = new List<GameObject>();
        private readonly List<IInspectorBehaviour> _inspectorBehaviours = new List<IInspectorBehaviour>();

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

            CreateUiElements();
        }

        public void ClearUiElements()
        {
            UiDataMapper.ClearElements();

            foreach (var it in _instantiatedUiElements)
                Destroy(it);
            
            _inspectorBehaviours.Clear();
        }

        private void CreateUiElements()
        {
            var isReadOnly = false;

            if (UiDataMapper.DataHandler.ReadOnlyProvider != null)
                isReadOnly = UiDataMapper.DataHandler.ReadOnlyProvider.Invoke();
            
            var createDefault = true;
            
            // check if the type declares 'OnRedrawInspector'
            var onRedrawInspectorMethod = _currentType.GetMethod(nameof(IDefaultInspectorOverride.EDITOR_OverrideDefaultInspector));
            if (onRedrawInspectorMethod != null)
            {
                object redrawInstance = null;
                if (_redrawInspectorTypeCache.TryGetValue(_currentType, out redrawInstance) == false)
                {
                    redrawInstance = Activator.CreateInstance(_currentType);
                    _redrawInspectorTypeCache[_currentType] = redrawInstance;
                }
                createDefault = (bool)onRedrawInspectorMethod.Invoke(_redrawInspectorTypeCache[_currentType], new object[] { this });
            }

            if (createDefault == false)
                return;
            
            ClearUiElements();
            
            var uiDataMapperElement = UiDataMapper.CreatePrimitive(_currentType, UiDataMapper.DataHandler.GetLastMemberName());

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
                    uiDataMapperElement = UiDataMapper.CreatePrimitive(memberType, StringUtils.ToPascalCase(memberInfo.Name));
                    if (uiDataMapperElement != null)
                    {
                        if (isReadOnly)
                            uiDataMapperElement.SetReadOnly();
                        uiDataMapperElement.BindDynamic(memberInfo);
                        continue;
                    }
                    
                    UiElementsFactory.DrawInspector(data, _content);
                }
            }
                    
            GetComponentInParent<CanvasRebuilderBehaviour>()?.Rebuild();
        }
    }
    
}

