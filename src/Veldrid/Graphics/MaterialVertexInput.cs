using System;

namespace Veldrid.Graphics
{
    public class MaterialVertexInput
    {
        public int VertexSizeInBytes { get; }
        public MaterialVertexInputElement[] Elements { get; }

        public MaterialVertexInput(int vertexSizeInBytes, params MaterialVertexInputElement[] elements)
        {
            VertexSizeInBytes = vertexSizeInBytes;
            Elements = elements;
        }

        public MaterialVertexInput() : this(0, Array.Empty<MaterialVertexInputElement>()) { }
    }

    public struct MaterialVertexInputElement
    {
        public string Name { get; set; }
        public VertexSemanticType SemanticType { get; set; }
        public VertexElementFormat ElementFormat { get; set; }
        public byte SizeInBytes { get; set; }

        public MaterialVertexInputElement(string name, VertexSemanticType semanticType, VertexElementFormat format)
        {
            Name = name;
            SemanticType = semanticType;
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
                case VertexElementFormat.Byte1:
                    return sizeof(byte) * 1;
                case VertexElementFormat.Byte4:
                    return sizeof(byte) * 4;
                default:
                    throw Illegal.Value<VertexElementFormat>();
            }
        }
    }

    public enum VertexElementFormat : byte
    {
        Float1,
        Float2,
        Float3,
        Float4,
        Byte1,
        Byte4
    }
}
