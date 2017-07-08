using System;
using System.Diagnostics;

namespace Veldrid.Graphics
{
    /// <summary>
    /// A generic description of vertex inputs to the device's input assembler stage.
    /// This object describes the inputs from a single vertex buffer.
    /// Shaders may use inputs from multiple vertex buffers, in which case multiple
    /// <see cref="VertexInputDescription"/> objects must be used.
    /// </summary>
    public class VertexInputDescription
    {
        /// <summary>
        /// The total size of an individual vertex, in bytes.
        /// </summary>
        public int VertexSizeInBytes { get; set; }

        /// <summary>
        /// The collection of individual vertex elements comprising a single vertex.
        /// </summary>
        public VertexInputElement[] Elements { get; set; }

        /// <summary>
        /// Constructs a new MaterialVertexInput.
        /// </summary>
        public VertexInputDescription(int vertexSizeInBytes, params VertexInputElement[] elements)
        {
            VertexSizeInBytes = vertexSizeInBytes;
            Elements = elements;
            DebugValidateVertexSize(elements, VertexSizeInBytes);
        }

        /// <summary>
        /// Constructs a new MaterialVertexInput.
        /// </summary>
        public VertexInputDescription() : this(0, Array.Empty<VertexInputElement>()) { }


        [Conditional("DEBUG")]
        private void DebugValidateVertexSize(VertexInputElement[] elements, int vertexSizeInBytes)
        {
            int computedSize = 0;
            foreach (VertexInputElement element in elements)
            {
                computedSize += element.SizeInBytes;
            }
            if (computedSize != vertexSizeInBytes)
            {
                throw new VeldridException($"Provided VertexSizeInBytes ({vertexSizeInBytes}) does not match sum of component sizes ({computedSize}).");
            }
        }
    }

    /// <summary>
    /// Describes an individual component of a vertex.
    /// </summary>
    public struct VertexInputElement
    {
        /// <summary>
        /// The name of the element.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The type of the element.
        /// </summary>
        public VertexSemanticType SemanticType { get; set; }

        /// <summary>
        /// The format of the element.
        /// </summary>
        public VertexElementFormat ElementFormat { get; set; }

        /// <summary>
        /// The size of the element, in bytes.
        /// </summary>
        public byte SizeInBytes { get; set; }

        public VertexElementInputClass StorageClassifier { get; set; }

        /// <summary>
        /// The number of instances to use a vertex. Only valid for PerInstance vertex inputs.
        /// </summary>
        public int InstanceStepRate { get; set; }

        /// <summary>
        /// Constructs a new MaterialVertexInputElement.
        /// </summary>
        public VertexInputElement(string name, VertexSemanticType semanticType, VertexElementFormat format)
        {
            Name = name;
            SemanticType = semanticType;
            ElementFormat = format;
            SizeInBytes = GetSizeInBytes(format);
            StorageClassifier = VertexElementInputClass.PerVertex;
            InstanceStepRate = 0;
        }

        /// <summary>
        /// Constructs a new MaterialVertexInputElement
        /// </summary>
        public VertexInputElement(
            string name,
            VertexSemanticType semanticType,
            VertexElementFormat format,
            VertexElementInputClass inputClass,
            int stepRate)
        {
            Name = name;
            SemanticType = semanticType;
            ElementFormat = format;
            SizeInBytes = GetSizeInBytes(format);
            StorageClassifier = inputClass;
            InstanceStepRate = stepRate;
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

    /// <summary>
    /// The structural format of a vertex element.
    /// </summary>
    public enum VertexElementFormat : byte
    {
        Float1,
        Float2,
        Float3,
        Float4,
        Byte1,
        Byte4
    }

    /// <summary>
    /// The storage type of a vertex element.
    /// </summary>
    public enum VertexElementInputClass : byte
    {
        /// <summary>Describes input which differs per-vertex.</summary>
        PerVertex,
        /// <summary>Describes input which differs per-instance.
        /// The instance step rate can be changed with <see cref="VertexInputElement.InstanceStepRate"/></summary>
        PerInstance
    }
}
