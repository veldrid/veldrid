namespace Veldrid.Graphics
{
    /// <summary>
    /// Describes which primitive faces to cull in the rasterizer.
    /// </summary>
    public enum FaceCullingMode : byte
    {
        /// <summary>
        /// Cull the back faces of primitives.
        /// </summary>
        Back,
        /// <summary>
        /// Cull the front faces of primitives.
        /// </summary>
        Front,
        /// <summary>
        /// Do not cull any primitive faces.
        /// </summary>
        None
    }
}