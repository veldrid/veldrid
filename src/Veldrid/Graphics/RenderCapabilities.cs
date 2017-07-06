namespace Veldrid.Graphics
{
    /// <summary>
    /// Describes the rendering capabilities of a RenderContext.
    /// </summary>
    public class RenderCapabilities
    {
        /// <summary>
        /// Returns whether the RenderContext supports the use of Geometry shaders.
        /// </summary>
        public bool SupportsGeometryShaders { get; }
        /// <summary>
        /// Returns whether the RenderContext supports TriangleFillMode.Wireframe
        /// </summary>
        public bool SupportsWireframeFillMode { get; }

        public RenderCapabilities(
            bool supportsGeometryShaders,
            bool supportsWireframeFillMode)
        {
            SupportsGeometryShaders = supportsGeometryShaders;
            SupportsWireframeFillMode = supportsWireframeFillMode;
        }
    }
}
