﻿using System;
using OpenTK.Graphics.OpenGL;

namespace Veldrid.Graphics.OpenGL
{
    public static class OpenGLFormats
    {
        public static OpenTK.Graphics.OpenGL.PixelFormat MapPixelFormat(PixelFormat format)
        {
            switch (format)
            {
                case PixelFormat.R32_G32_B32_A32_Float:
                    return OpenTK.Graphics.OpenGL.PixelFormat.Rgba;
                case PixelFormat.Alpha_UInt8:
                    return OpenTK.Graphics.OpenGL.PixelFormat.Alpha;
                case PixelFormat.R8_G8_B8_A8:
                    return OpenTK.Graphics.OpenGL.PixelFormat.Rgba;
                default:
                    throw Illegal.Value<PixelFormat>();
            }
        }

        public static PixelType MapPixelType(PixelFormat format)
        {
            switch (format)
            {
                case PixelFormat.R32_G32_B32_A32_Float:
                    return PixelType.Float;
                case PixelFormat.Alpha_UInt8:
                    return PixelType.UnsignedByte;
                case PixelFormat.R8_G8_B8_A8:
                    return PixelType.UnsignedInt8888;
                default:
                    throw Illegal.Value<PixelFormat>();
            }
        }

        internal static PixelInternalFormat MapPixelInternalFormat(PixelFormat format)
        {
            switch (format)
            {
                case PixelFormat.R32_G32_B32_A32_Float:
                    return PixelInternalFormat.Rgba32f;
                case PixelFormat.Alpha_UInt8:
                    return PixelInternalFormat.Alpha;
                case PixelFormat.R8_G8_B8_A8:
                    return PixelInternalFormat.Rgba8;
                default:
                    throw Illegal.Value<PixelFormat>();
            }
        }

        internal static BlendingFactorSrc ConvertBlendSrc(Blend blend)
        {
            switch (blend)
            {
                case Blend.Zero:
                    return BlendingFactorSrc.Zero;
                case Blend.One:
                    return BlendingFactorSrc.One;
                case Blend.SourceAlpha:
                    return BlendingFactorSrc.SrcAlpha;
                case Blend.InverseSourceAlpha:
                    return BlendingFactorSrc.OneMinusSrcAlpha;
                case Blend.DestinationAlpha:
                    return BlendingFactorSrc.DstAlpha;
                case Blend.InverseDestinationAlpha:
                    return BlendingFactorSrc.OneMinusDstAlpha;
                case Blend.SourceColor:
                    return BlendingFactorSrc.SrcColor;
                case Blend.InverseSourceColor:
                    return BlendingFactorSrc.OneMinusSrcColor;
                case Blend.DestinationColor:
                    return BlendingFactorSrc.DstColor;
                case Blend.InverseDestinationColor:
                    return BlendingFactorSrc.OneMinusDstColor;
                case Blend.BlendFactor:
                    return BlendingFactorSrc.ConstantColor;
                case Blend.InverseBlendFactor:
                    return BlendingFactorSrc.OneMinusConstantColor;
                default:
                    throw Illegal.Value<Blend>();
            }
        }

        internal static BlendingFactorDest ConvertBlendDest(Blend blend) => (BlendingFactorDest)ConvertBlendSrc(blend);

        public static BlendEquationMode ConvertBlendEquation(BlendFunction function)
        {
            switch (function)
            {
                case BlendFunction.Add:
                    return BlendEquationMode.FuncAdd;
                case BlendFunction.Subtract:
                    return BlendEquationMode.FuncSubtract;
                case BlendFunction.ReverseSubtract:
                    return BlendEquationMode.FuncReverseSubtract;
                case BlendFunction.Minimum:
                    return BlendEquationMode.Min;
                case BlendFunction.Maximum:
                    return BlendEquationMode.Max;
                default:
                    throw Illegal.Value<BlendFunction>();
            }
        }

        internal static DrawElementsType MapIndexFormat(IndexFormat format)
        {
            switch (format)
            {
                case IndexFormat.UInt32:
                    return DrawElementsType.UnsignedInt;
                case IndexFormat.UInt16:
                    return DrawElementsType.UnsignedShort;
                case IndexFormat.UInt8:
                    return DrawElementsType.UnsignedByte;
                default:
                    throw Illegal.Value<DrawElementsType>();
            }
        }

        public static int GetIndexFormatSize(DrawElementsType type)
        {
            switch (type)
            {
                case DrawElementsType.UnsignedByte:
                    return 1;
                case DrawElementsType.UnsignedShort:
                    return 2;
                case DrawElementsType.UnsignedInt:
                    return 4;
                default:
                    throw Illegal.Value<DrawElementsType>();
            }
        }
    }
}
