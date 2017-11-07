using System;

namespace Veldrid
{
    /// <summary>
    /// Describes a single element of a vertex.
    /// </summary>
    public struct VertexElementDescription : IEquatable<VertexElementDescription>
    {
        /// <summary>
        /// The name of the element.
        /// </summary>
        public string Name;
        /// <summary>
        /// The semantic type of the element.
        /// </summary>
        public VertexElementSemantic Semantic;
        /// <summary>
        /// The format of the element.
        /// </summary>
        public VertexElementFormat Format;
        /// <summary>
        /// A value controlling how often data for instances is advanced for this element. For per-vertex elements, this value
        /// should be 0.
        /// For example, an InstanceStepRate of 3 indicates that 3 instances will be drawn with the same value for this element. The
        /// next 3 instances will be drawn with the next value for this element, and so on.
        /// </summary>
        public uint InstanceStepRate;

        /// <summary>
        /// Constructs a new VertexElementDescription describing a per-vertex element.
        /// </summary>
        /// <param name="name">The name of the element.</param>
        /// <param name="semantic">The semantic type of the element.</param>
        /// <param name="format">The format of the element.</param>
        public VertexElementDescription(string name, VertexElementSemantic semantic, VertexElementFormat format)
            : this(name, format, semantic, 0)
        {
        }

        /// <summary>
        /// Constructs a new VertexElementDescription.
        /// </summary>
        /// <param name="name">The name of the element.</param>
        /// <param name="semantic">The semantic type of the element.</param>
        /// <param name="format">The format of the element.</param>
        /// <param name="instanceStepRate">A value controlling how often data for instances is advanced for this element. For
        /// per-vertex elements, this value should be 0.
        /// For example, an InstanceStepRate of 3 indicates that 3 instances will be drawn with the same value for this element.
        /// The next 3 instances will be drawn with the next value for this element, and so on.</param>
        public VertexElementDescription(
            string name,
            VertexElementFormat format,
            VertexElementSemantic semantic,
            uint instanceStepRate)
        {
            Name = name;
            Format = format;
            Semantic = semantic;
            InstanceStepRate = instanceStepRate;
        }

        /// <summary>
        /// Element-wise equality.
        /// </summary>
        /// <param name="other">The instance to compare to.</param>
        /// <returns>True if all elements are equal; false otherswise.</returns>
        public bool Equals(VertexElementDescription other)
        {
            return Name.Equals(other.Name)
                && Format == other.Format
                && Semantic == other.Semantic
                && InstanceStepRate == other.InstanceStepRate;
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
        public override int GetHashCode()
        {
            return HashHelper.Combine(
                Name.GetHashCode(),
                Format.GetHashCode(),
                Semantic.GetHashCode(),
                InstanceStepRate.GetHashCode());
        }
    }
}