using System;
using System.Numerics;
using OpenTK.Graphics;
using System.Runtime.InteropServices;

namespace Veldrid.Graphics
{
    public struct RgbaFloat
    {
        private readonly Vector4 _channels;

        public float R => _channels.X;
        public float G => _channels.Y;
        public float B => _channels.Z;
        public float A => _channels.W;

        public RgbaFloat(float r, float g, float b, float a)
        {
            _channels = new Vector4(r, g, b, a);
        }

        public static readonly RgbaFloat Red = new RgbaFloat(1, 0, 0, 1);
        public static readonly RgbaFloat Green = new RgbaFloat(0, 1, 0, 1);
        public static readonly RgbaFloat Blue = new RgbaFloat(0, 0, 1, 1);
        public static readonly RgbaFloat Yellow = new RgbaFloat(1, 1, 0, 1);
        public static readonly RgbaFloat Grey = new RgbaFloat(.25f, .25f, .25f, 1);
        public static readonly RgbaFloat Cyan = new RgbaFloat(0, 1, 1, 1);
        public static readonly RgbaFloat White = new RgbaFloat(1, 1, 1, 1);
        public static readonly RgbaFloat CornflowerBlue = new RgbaFloat(0.3921f, 0.5843f, 0.9294f, 1);

        internal static unsafe Color4 ToOpenTKColor(RgbaFloat clearColor)
        {
            return *(Color4*)&clearColor;
        }
    }
}