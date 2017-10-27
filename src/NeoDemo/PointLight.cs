using ShaderGen;
using System.Numerics;

namespace Veldrid.NeoDemo
{
    public struct PointLightInfo
    {
        public Vector3 Position;
        public float Range;
        public Vector3 Color;
        public float _padding;
    }

    public struct PointLightsInfo
    {
        public int NumActiveLights;
        [ArraySize(4)] public PointLightInfo[] PointLights;

        public Blittable GetBlittable()
        {
            return new Blittable
            {
                NumActiveLights = NumActiveLights,
                PointLights0 = PointLights[0],
                PointLights1 = PointLights[1],
                PointLights2 = PointLights[2],
                PointLights3 = PointLights[3],
            };
        }

        public struct Blittable
        {
            public int NumActiveLights;
            public Vector3 _padding;
            public PointLightInfo PointLights0;
            public PointLightInfo PointLights1;
            public PointLightInfo PointLights2;
            public PointLightInfo PointLights3;
        }
    }
}
