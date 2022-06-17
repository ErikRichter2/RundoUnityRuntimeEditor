using Rundo.Core.Data;
using Rundo.RuntimeEditor.Behaviours.UI;
using Rundo.RuntimeEditor.Tools;
using Rundo.Ui;
using UnityEngine;

namespace Rundo.RuntimeEditor.Behaviours
{
    [DataComponent(typeof(Light))]
    [DataTypeId("247a8470-ff40-4f13-8c26-803fe73f6a8c")]
    public class DataLightBehaviour : DataComponentMonoBehaviour
    {
        public LightType type;

        public float range;
        public float spotAngle;
        public Vector2 areaSize;
        public LightShape shape;

        public Color color;

        public LightmapBakeType lightmapBakeType = LightmapBakeType.Baked;
        public float intensity;
        public float bounceIntensity;
        public LightShadows shadows;

        public static void OnDefaultInspectorRedraw(DefaultDataInspectorBehaviour defaultDataInspectorBehaviour)
        {
            var lightTypeValue = defaultDataInspectorBehaviour.UiDataMapper.DataHandler.GetValue(nameof(type));
            if (lightTypeValue.IsUndefined)
                return;
            var lightType = (LightType)lightTypeValue.Value;

            var rangeElement = defaultDataInspectorBehaviour.GetElementInstanceByName(nameof(range));
            var spotAngleElement = defaultDataInspectorBehaviour.GetElementInstanceByName(nameof(spotAngle));
            var areaSizeElement = defaultDataInspectorBehaviour.GetElementInstanceByName(nameof(areaSize));
            var shapeElement = defaultDataInspectorBehaviour.GetElementInstanceByName(nameof(shape));
            
            rangeElement.SetActive(lightType == LightType.Point || lightType == LightType.Spot || lightType == LightType.Area);
            spotAngleElement.SetActive(lightType == LightType.Spot);
            areaSizeElement.SetActive(lightType == LightType.Area);
            shapeElement.SetActive(lightType == LightType.Area);
            
            defaultDataInspectorBehaviour.GetComponentInParent<CanvasRebuilderBehaviour>()?.Rebuild();
        }
    }
    
}



