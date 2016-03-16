using System;

namespace Veldrid.Graphics
{
    public static class VertexFormatHelpers
    {
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
