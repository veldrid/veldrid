using System.Numerics;
using System.Runtime.InteropServices;
using Veldrid.Graphics;

namespace Veldrid.RenderDemo
{
    [StructLayout(LayoutKind.Sequential)]
    public struct DirectionalLightBuffer
    {
        public RgbaFloat Color;
        public Vector3 Direction;
        private float __buffer;

        public DirectionalLightBuffer(RgbaFloat color, Vector3 direction)
        {
            Color = color;
            Direction = direction;
            __buffer = 0;
        }
    }
}