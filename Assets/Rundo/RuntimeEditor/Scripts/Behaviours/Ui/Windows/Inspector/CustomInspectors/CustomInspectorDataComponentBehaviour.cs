using System;
using Rundo.Core.Commands;
using Rundo.Core.Data;
using Rundo.Core.Utils;
using Rundo.RuntimeEditor.Attributes;
using Rundo.RuntimeEditor.Data;
using Rundo.RuntimeEditor.Factory;
using Rundo.RuntimeEditor.Tools;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Rundo.RuntimeEditor.Behaviours.UI
{
    [CustomInspector(typeof(DataComponent))]
    public class CustomInspectorDataComponentBehaviour : InspectorWindowElementBehaviour
    {
        [SerializeField] private TMP_Text _headerName;
        [SerializeField] private Button _headerOptions;
        [SerializeField] private Transform _content;
        [SerializeField] private ToggleBehaviour _override;

        protected override void MapUi()
        {
            UiDataMapper.Map(_override)
                .BindCustom<DataComponent>(
                    (data, value) =>
                    {
                        if (IsOverridable() == false)
                            return;

                        if (value.Value != data.DataComponentPrefab.OverridePrefabComponent)
                        {
                            var command = new SetValueToMemberCommand(data.DataComponentPrefab,
                                nameof(DataComponentPrefab.OverridePrefabComponent), value.Value);
                            command.AddDispatcherData(data);
                            data.DataGameObject.DataScene.CommandProcessor.Process(command);
                            if (value.Value == false)
                                data.DataGameObject.DataScene.CommandProcessor.Process(
                                    new ModifyDataCommand(data.GetData(), data.DataComponentPrefab.GetPrefabData()));
                        }
                    },
                    data =>
                    {
                        if (IsOverridable())
                            return data.Data.DataComponentPrefab.OverridePrefabComponent;
                        return false;
                    });
            
            _headerOptions.onClick.AddListener(() =>
            {
                var prefab = Resources.Load<ContextMenuBehaviour>("Rundo/Ui/ContextMenuPrefab");
                var instance = Instantiate(prefab, GetComponentInParent<Canvas>().transform);

                var canPasteClipboardAsNew = Clipboard.IsType<DataComponent>();
                var canPasteClipboardAsValues = false;

                var clipboardComponent = Clipboard.Copy<DataComponent>();
                if (clipboardComponent != null &&
                    clipboardComponent.GetComponentType() == ((DataComponent)UiDataMapper.DataHandler.GetRootData()[0]).GetComponentType())
                    canPasteClipboardAsValues = true;

                instance
                    .AddItemData(new ContextMenuItemData<object>
                    {
                        Name = "Remove",
                        Callback = obj =>
                        {
                            foreach (var it in UiDataMapper.DataHandler.GetRootDataTyped<DataComponent>())
                                it.GetParentInHierarchy<DataGameObject>().RemoveComponent(it);
                        }
                    })
                    .AddItemData(new ContextMenuItemData<object>
                    {
                        Name = "Copy",
                        Disabled = UiDataMapper.DataHandler.GetRootData().Count != 1,
                        Callback = obj =>
                        {
                            Clipboard.Set(UiDataMapper.DataHandler.GetRootData()[0]);
                        }
                    })
                    .AddItemData(new ContextMenuItemData<object>
                    {
                        Name = "Paste as New",
                        Disabled = canPasteClipboardAsNew == false,
                        Callback = obj =>
                        {
                            foreach (var it in UiDataMapper.DataHandler.GetRootDataTyped<DataComponent>())
                                it.GetParentInHierarchy<DataGameObject>().AddComponent(Clipboard.Clone<DataComponent>());
                        }
                    })
                    .AddItemData(new ContextMenuItemData<object>
                    {
                        Name = "Paste Values",
                        Disabled = canPasteClipboardAsValues == false,
                        Callback = obj =>
                        {
                            var copy = Clipboard.Clone<DataComponent>();
                            foreach (var it in UiDataMapper.DataHandler.GetRootDataTyped<DataComponent>())
                                it.CopyFrom(copy);
                        }
                    });
            });
        }

        private bool IsOverridable()
        {
            foreach (var it in UiDataMapper.DataHandler.GetRootDataTyped<DataComponent>())
                if (it.IsOverridable() == false)
                    return false;

            return true;
        }

        protected override void RedrawInternal()
        {
            base.RedrawInternal();

            _override.gameObject.SetActive(IsOverridable());
            _headerName.text = StringUtils.ToPascalCase(GetComponentType()?.Name);
        }

        private Type GetComponentType()
        {
            if (HasData())
                return ((DataComponent)UiDataMapper.DataHandler.GetRootData()[0]).GetComponentType();
            return null;
        }

        protected override void OnDataSetInternal()
        {
            base.OnDataSetInternal();
            
            foreach (Transform it in _content)
                Destroy(it.gameObject);

            if (HasData() == false)
                return;
            
            var dataHandler = UiDataMapper.DataHandler.Copy().AddMember(nameof(DataComponent.ComponentData));
            InspectorFactory.DrawInspector(dataHandler, _content, false);

            GetComponentInParent<CanvasRebuilderBehaviour>()?.Rebuild();
        }
    }
}

