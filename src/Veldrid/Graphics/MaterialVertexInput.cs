using System;

namespace Veldrid.Graphics
{
    /// <summary>Describes the vertex input of a material.</summary>
    public class MaterialVertexInput
    {
        /// <summary>The total size of an individual vertex, in bytes.</summary>
        public int VertexSizeInBytes { get; set; }

        /// <summary>The collection of individual vertex elements comprising a single vertex.</summary>
        public MaterialVertexInputElement[] Elements { get; set; }

        /// <summary>Constructs a new MaterialVertexInput.</summary>
        public MaterialVertexInput(int vertexSizeInBytes, params MaterialVertexInputElement[] elements)
        {
            VertexSizeInBytes = vertexSizeInBytes;
            Elements = elements;
        }

        /// <summary>Constructs a new MaterialVertexInput.</summary>
        public MaterialVertexInput() : this(0, Array.Empty<MaterialVertexInputElement>()) { }
    }

    /// <summary>Describes an individual component of a vertex.</summary>
    public struct MaterialVertexInputElement
    {
        /// <summary>The name of the element.</summary>
        public string Name { get; set; }

        /// <summary>The type of the element.</summary>
        public VertexSemanticType SemanticType { get; set; }

        /// <summary>The format of the element.</summary>
        public VertexElementFormat ElementFormat { get; set; }

        /// <summary>The size of the element, in bytes.</summary>
        public byte SizeInBytes { get; set; }

        /// <summary>Constructs a new MaterialVertexInputElement</summary>
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

    /// <summary>The structural format of a vertex element.</summary>
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
