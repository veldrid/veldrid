using System;

namespace Veldrid.Graphics
{
    internal static class FormatHelpers
    {
        public static int GetIndexFormatElementByteSize(IndexFormat format)
        {
            switch (format)
            {
                case IndexFormat.UInt32:
                    return 4;
                case IndexFormat.UInt16:
                    return 2;
                case IndexFormat.UInt8:
                    return 1;
                default:
                    throw Illegal.Value<IndexFormat>();
            }
        }

        public static int GetPixelSize(PixelFormat veldridFormat)
        {
            switch (veldridFormat)
            {
                case PixelFormat.R32_G32_B32_A32_Float:
                    return 16;
                case PixelFormat.R16_UInt:
                    return 2;
                case PixelFormat.R8_G8_B8_A8_UInt:
                    return 4;
                case PixelFormat.R8_UInt:
                    return 1;
                default:
                    throw Illegal.Value<PixelFormat>();
            }
        }

        public static bool GetShaderConstantTypeByteSize(ShaderConstantType type, out int bytes)
        {
            switch (type)
            {
                case ShaderConstantType.Boolean:
                    bytes = 1;
                    return true;
                case ShaderConstantType.Int1:
                    bytes = 4;
                    return true;
                case ShaderConstantType.Int2:
                    bytes = 8;
                    return true;
                case ShaderConstantType.Int3:
                    bytes = 12;
                    return true;
                case ShaderConstantType.Int4:
                    bytes = 16;
                    return true;
                case ShaderConstantType.Float1:
                    bytes = 4;
                    return true;
                case ShaderConstantType.Float2:
                    bytes = 8;
                    return true;
                case ShaderConstantType.Float3:
                    bytes = 12;
                    return true;
                case ShaderConstantType.Float4:
                    bytes = 16;
                    return true;
                case ShaderConstantType.Matrix4x4:
                    bytes = 64;
                    return true;
                case ShaderConstantType.Custom:
                default:
                    bytes = -1;
                    return false;
            }
        }

        /// <summary>
        /// Gets the number of individual vertex components from a given format.
        /// </summary>
        /// <param name="format">The vertex format.</param>
        /// <returns>The number of vertex components.</returns>
        public static byte GetElementCount(VertexElementFormat format)
        {
            switch (format)
            {
                case VertexElementFormat.Byte1:
                case VertexElementFormat.Float1:
                    return 1;
                case VertexElementFormat.Float2:
                    return 2;
                case VertexElementFormat.Float3:
                    return 3;
                case VertexElementFormat.Float4:
                case VertexElementFormat.Byte4:
                    return 4;
                default:
                    throw Illegal.Value<VertexElementFormat>();
            }
        }
    }
}
