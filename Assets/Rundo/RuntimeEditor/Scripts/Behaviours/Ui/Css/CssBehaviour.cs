using System;
using System.Collections.Generic;
using Rundo.Core.Utils;
using UnityEngine;

namespace Rundo.RuntimeEditor.Behaviours
{
    public class CssBehaviour : MonoBehaviour
    {
        private CssData _data;
        private CssData _defaultData;
        private bool _updateChildren;
        private bool _requireUpdate;
        private bool _wasDefaultInit;
        
        public bool IsSuppressChildrenCss { get; private set; }

        private void Start()
        {
            _requireUpdate = true;
        }

        public CssBehaviour SuppressChildrenCss()
        {
            _requireUpdate = true;
            IsSuppressChildrenCss = true;
            return this;
        }
        
        public bool? GetBool(CssPropertyEnum property)
        {
            if (TryGetBool(property, out var value))
                return value;

            return null;
        }

        public bool TryGetBool(CssPropertyEnum property, out bool value)
        {
            value = false;
            if (TryGet(property, out var obj))
            {
                value = (bool)obj;
                return true;
            }

            return false;
        }

        public int? GetInt(CssPropertyEnum property)
        {
            if (TryGetInt(property, out var value))
                return value;

            return null;
        }

        public bool TryGetInt(CssPropertyEnum property, out int value)
        {
            value = 0;
            if (TryGet(property, out var obj))
            {
                value = (int)obj;
                return true;
            }

            return false;
        }

        public float? GetFloat(CssPropertyEnum property)
        {
            if (TryGetFloat(property, out var value))
                return value;

            return null;
        }

        public bool TryGetFloat(CssPropertyEnum property, out float value)
        {
            value = 0f;
            if (TryGet(property, out var obj))
            {
                value = (float)obj;
                return true;
            }

            return false;
        }

        public string GetString(CssPropertyEnum property)
        {
            if (TryGetValue(property, out var value))
                return (string)value;

            return null;
        }
        
        public bool TryGetString(CssPropertyEnum property, out string value)
        {
            value = "";
            if (TryGet(property, out var obj))
            {
                value = (string)obj;
                return true;
            }

            return false;
        }

        public T? Get<T>(CssPropertyEnum property) where T: struct
        {
            if (TryGet<T>(property, out var value))
                return value;

            return null;
        }

        public bool TryGet<T>(CssPropertyEnum property, out T value) where T: struct
        {
            value = default;
            if (TryGet(property, out var obj))
            {
                value = (T)obj;
                return true;
            }

            return false;
        }

        public object Get(CssPropertyEnum property)
        {
            if (TryGet(property, out var value))
                return value;

            return null;
        }

        public bool TryGet(CssPropertyEnum property, out object value)
        {
            value = default;
            if (TryGetValue(property, out object obj))
            {
                value = obj;
                return true;
            }

            return false;
        }

        private bool TryGetValue(CssPropertyEnum property, out object value)
        {
            if (_requireUpdate)
                UpdateCss();
            
            if (_data != null && _data.TryGetValue(property, out value))
                return true;

            if (transform.parent != null)
            {
                var parentCss = transform.parent.GetComponentInParent<CssBehaviour>();
                if (parentCss.IsSuppressChildrenCss == false &&
                    parentCss.TryGetValue(property, out value))
                    return true;
            }

            if (_defaultData.TryGetValue(property, out value))
                return true;
            
            return false;
        }

        public CssBehaviour SetDefaultValue(CssPropertyEnum property, object value)
        {
            _defaultData ??= new CssData();
            _defaultData.SetValue(property, value);
            return this;
        }
        
        public CssBehaviour SetValue(CssPropertyEnum property, object value)
        {
            _data ??= new CssData();
            _data.SetValue(property, value);
            _requireUpdate = true;
            _updateChildren = true;
            return this;
        }

        private void UpdateCss()
        {
            if (_requireUpdate)
            {
                _requireUpdate = false;
                
                foreach (var it in GetComponents<ICssElement>())
                {
                    if (_wasDefaultInit == false)
                    {
                        _wasDefaultInit = false;
                        it.InitDefaultCssValues(this);
                    }
                        
                    it.UpdateCss(this);
                }
            }
        }

        private void Update()
        {
            if (_requireUpdate)
                UpdateCss();
            
            if (_updateChildren)
            {
                _updateChildren = false;
                
                var queue = new Queue<GameObject>();
                QueueUtils.EnqueueGameObjectChildren(queue, gameObject);

                while (queue.Count > 0)
                {
                    var go = queue.Dequeue();
                    var cssBehaviour = go.GetComponent<CssBehaviour>();
                    if (cssBehaviour != null)
                    {
                        cssBehaviour._requireUpdate = true;
                        QueueUtils.EnqueueGameObjectChildren(queue, go);
                    }
                }
            }
        }
    }
}


