using System.Numerics;

#pragma warning disable IDE0051 // Remove unused private members

namespace Veldrid.NeoDemo
{
    public struct PointLightInfo
    {
        public Vector3 Position;
        private float _padding0;
        public Vector3 Color;
        public float _padding1;
        public float Range;
        private float _padding2;
        private float _padding3;
        private float _padding4;
    }

    public struct PointLightsInfo
    {
        public PointLightInfo[] PointLights;
        public int NumActiveLights;
        private float _padding0;
        private float _padding1;
        private float _padding2;

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
            public PointLightInfo PointLights0;
            public PointLightInfo PointLights1;
            public PointLightInfo PointLights2;
            public PointLightInfo PointLights3;
            public int NumActiveLights;

            private float _padding0;
            private float _padding1;
            private float _padding2;
        }
    }
}
