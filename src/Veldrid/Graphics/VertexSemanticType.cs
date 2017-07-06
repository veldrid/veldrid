namespace Veldrid.Graphics
{
    /// <summary>
    /// The semantic meaning of a vertex element.
    /// </summary>
    public enum VertexSemanticType : byte
    {
        /// <summary>
        /// Describes a position.
        /// </summary>
        Position,
        /// <summary>
        /// Describes a texture coordinate.
        /// </summary>
        TextureCoordinate,
        /// <summary>
        /// Describes a normal vector.
        /// </summary>
        Normal,
        /// <summary>
        /// Describes a color.
        /// </summary>
        Color
    }
}