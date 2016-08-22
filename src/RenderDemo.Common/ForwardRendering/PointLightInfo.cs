using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Veldrid.RenderDemo.ForwardRendering
{
    [StructLayout(LayoutKind.Sequential)]
    public struct PointLightInfo : IEquatable<PointLightInfo>
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

        public bool Equals(PointLightInfo other)
        {
            return Position.Equals(other.Position) && Range.Equals(other.Range) && Color.Equals(other.Color);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PointLightsBuffer : IEquatable<PointLightsBuffer>
    {
        public const int MaxLights = 4;

        public int NumActivePointLights;
        private Vector3 __padding;

        public PointLightInfo LightInfo0;
        public PointLightInfo LightInfo1;
        public PointLightInfo LightInfo2;
        public PointLightInfo LightInfo3;

        public bool Equals(PointLightsBuffer other)
        {
            return NumActivePointLights.Equals(other.NumActivePointLights)
                && LightInfo0.Equals(other.LightInfo0)
                && LightInfo1.Equals(other.LightInfo1)
                && LightInfo2.Equals(other.LightInfo2)
                && LightInfo3.Equals(other.LightInfo3);
        }
    }
}
