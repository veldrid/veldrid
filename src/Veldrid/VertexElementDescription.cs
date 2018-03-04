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
        /// The offset in bytes, from the start of the vertex that this element appears.
        /// If 0, this vertex element is assumed to be tightly-packed with respect to the other vertex elements that appear in
        /// the <see cref="VertexLayoutDescription"/> with it. It's offset will effectively be the sum of the sizes of all
        /// of the elements that occur before it in the layout description.
        /// </summary>
        public uint Offset;

        /// <summary>
        /// Constructs a new VertexElementDescription describing a per-vertex element.
        /// </summary>
        /// <param name="name">The name of the element.</param>
        /// <param name="semantic">The semantic type of the element.</param>
        /// <param name="format">The format of the element.</param>
        /// <param name="offset">The offset in bytes, from the start of the vertex that this element appears.</param>
        public VertexElementDescription(string name, VertexElementSemantic semantic, VertexElementFormat format, uint offset)
        {
            Name = name;
            Format = format;
            Semantic = semantic;
            Offset = offset;
        }

        public VertexElementDescription(string name, VertexElementSemantic semantic, VertexElementFormat format)
            : this(name, semantic, format, 0)
        {
        }

        /// <summary>
        /// Constructs a new VertexElementDescription.
        /// </summary>
        /// <param name="name">The name of the element.</param>
        /// <param name="semantic">The semantic type of the element.</param>
        /// <param name="format">The format of the element.</param>
        public VertexElementDescription(
            string name,
            VertexElementFormat format,
            VertexElementSemantic semantic)
            : this(name, semantic, format)
        {
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
                && Semantic == other.Semantic;
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
                Semantic.GetHashCode());
        }
    }
}