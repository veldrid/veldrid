using System.Numerics;
using Veldrid.Graphics;

namespace Veldrid.RenderDemo
{
    public struct DirectionalLightBuffer
    {
        public RgbaFloat Color;
        public Vector3 Direction;
#pragma warning disable 0414 // This is used as struct padding.
        private float __buffer;
#pragma warning restore 0414

        public DirectionalLightBuffer(RgbaFloat color, Vector3 direction)
        {
            Color = color;
            Direction = direction;
            __buffer = 0;
        }
    }
}