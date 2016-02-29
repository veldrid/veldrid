using System;
using System.Collections.Generic;

namespace Veldrid.Graphics
{
    public class MaterialVertexInput
    {
        public int VertexSizeInBytes { get; }
        public MaterialVertexInputElement[] Elements { get; }
        public MaterialVertexInput(int vertexSizeInBytes, MaterialVertexInputElement[] elements)
        {
            VertexSizeInBytes = vertexSizeInBytes;
            Elements = elements;
        }
    }

    public struct MaterialVertexInputElement
    {
        public string Name { get; }
        public byte SizeInBytes { get; }
        public VertexElementFormat ElementFormat { get; }

        public MaterialVertexInputElement(string name, VertexElementFormat format)
        {
            Name = name;
            ElementFormat = format;
            SizeInBytes = GetSizeInBytes(format);
        }

        private static byte GetSizeInBytes(VertexElementFormat format)
        {
            switch (format)
            {
                case VertexElementFormat.Float1:
                    return sizeof(float) * 1;
                case VertexElementFormat.Float2:
                    return sizeof(float) * 2;
                case VertexElementFormat.Float3:
                    return sizeof(float) * 3;
                case VertexElementFormat.Float4:
                    return sizeof(float) * 4;
                default:
                    throw new InvalidOperationException("Invalid format: " + format);
            }
        }
    }

    public enum VertexElementFormat : byte
    {
        Float1,
        Float2,
        Float3,
        Float4
    }
}
