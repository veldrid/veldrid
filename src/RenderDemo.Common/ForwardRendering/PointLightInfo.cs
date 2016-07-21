using System.Numerics;
using System.Runtime.InteropServices;

namespace Veldrid.RenderDemo.ForwardRendering
{
    [StructLayout(LayoutKind.Sequential)]
    public struct PointLightInfo
    {
        public Vector3 Position;
        public float Range;
        public Vector3 Color;
        private float __padding;

        public PointLightInfo(Vector3 position, Vector3 color, float range)
        {
            Position = position;
            Color = color;
            Range = range;
            __padding = 0f;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PointLightsBuffer
    {
        public const int MaxLights = 4;

        public int NumActivePointLights;
        private Vector3 __padding;

        public PointLightInfo LightInfo0;
        public PointLightInfo LightInfo1;
        public PointLightInfo LightInfo2;
        public PointLightInfo LightInfo3;
    }
}
