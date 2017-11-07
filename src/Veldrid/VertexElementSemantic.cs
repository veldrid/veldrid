namespace Veldrid
{
    /// <summary>
    /// The type of a vertex element, describing how the element is interpreted.
    /// </summary>
    public enum VertexElementSemantic : byte
    {
        /// <summary>
        /// A position.
        /// </summary>
        Position,
        /// <summary>
        /// A normal direction.
        /// </summary>
        Normal,
        /// <summary>
        /// A texture coordinate.
        /// </summary>
        TextureCoordinate,
        /// <summary>
        /// A color.
        /// </summary>
        Color,
    }
}