using System.Collections;
using System.Collections.Generic;
using Rundo.Core.Data;
using Rundo.RuntimeEditor.Behaviours;
using Rundo.Ui;
using UnityEngine;

[DataComponent]
[DataTypeId("5ab0ed5a-3ecc-4a1f-9984-2b808cea0993")]
public class MeshColorSetterBehaviour : DataComponentMonoBehaviour
{
    private Color _color;
    private bool _invalidate;
    
    public Color Color 
    {
        get => _color;
        set
        {
            _color = value;
            _invalidate = true;
        }
    }
    
    private void Update()
    {
        if (_invalidate)
        {
            _invalidate = false;
            Refresh();
        }
    }
    
    private void Refresh()
    {
        GetComponentInChildren<MeshRenderer>().material.color = _color;
    }
}
