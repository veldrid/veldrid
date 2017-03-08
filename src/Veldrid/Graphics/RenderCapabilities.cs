namespace Veldrid.Graphics
{
    public class RenderCapabilities
    {
        public bool SupportsGeometryShaders { get; }
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
