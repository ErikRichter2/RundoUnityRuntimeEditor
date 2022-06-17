using Rundo.Core.Data;
using Rundo.Ui;
using UnityEngine;

namespace Rundo.RuntimeEditor.Behaviours
{
    [DataComponent(typeof(Light))]
    [DataTypeId("247a8470-ff40-4f13-8c26-803fe73f6a8c")]
    public class DataLightBehaviour : DataComponentMonoBehaviour
    {
        public LightType type;
        public Color color;
        public LightmapBakeType lightmapBakeType;
        public float intensity;
        public float bounceIntensity;
        public LightShadows shadows;
    }
    
}



