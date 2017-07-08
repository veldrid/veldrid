namespace Veldrid.Graphics
{
    /// <summary>
    /// Describes how triangles are rasterized.
    /// </summary>
    public enum TriangleFillMode : byte
    {
        /// <summary>
        /// Fill triangles fully.
        /// </summary>
        Solid,
        /// <summary>
        /// Outline triangles "wireframe-style".
        /// </summary>
        Wireframe
    }
}