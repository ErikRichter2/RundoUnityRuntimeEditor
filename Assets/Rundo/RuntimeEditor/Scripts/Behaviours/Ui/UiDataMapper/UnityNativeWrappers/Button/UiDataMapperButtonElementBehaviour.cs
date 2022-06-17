using System;
using Rundo.RuntimeEditor.Data.UiDataMapper;
using UnityEngine;
using UnityEngine.UI;

namespace Rundo.RuntimeEditor.Behaviours
{
    public class UiDataMapperButtonElementBehaviour : MonoBehaviour, IUiDataMapperElementBehaviour, ICustomUiDataMapper
    {
        public Type GetDataMapperType()
        {
            return typeof(UiDataMapperButtonInstance);
        }

        public void OnClick(Action onClick)
        {
            GetComponent<Button>().onClick.AddListener(() =>
            {
                onClick.Invoke();
            });
        }

        public void SetUndefValue()
        {
        }
    }
}
