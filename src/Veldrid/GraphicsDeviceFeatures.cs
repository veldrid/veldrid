namespace Veldrid
{
    /// <summary>
    /// Enumerates the optional features supported by a given <see cref="GraphicsDevice"/>.
    /// </summary>
    public class GraphicsDeviceFeatures
    {
        /// <summary>
        /// Indicates whether Compute Shaders can be used.
        /// </summary>
        public bool ComputeShader { get; }
        /// <summary>
        /// Indicates whether Geometry Shaders can be used.
        /// </summary>
        public bool GeometryShader { get; }
        /// <summary>
        /// Indicates whether Tessellation Shaders can be used.
        /// </summary>
        public bool TessellationShaders { get; }
        /// <summary>
        /// Indicates whether multiple independent viewports can be set simultaneously.
        /// If this is not supported, then only the first Viewport index will be used for all render outputs.
        /// </summary>
        public bool MultipleViewports { get; }
        /// <summary>
        /// Indicates whether <see cref="SamplerDescription.LodBias"/> can be non-zero.
        /// If false, it is an error to attempt to use a non-zero bias value.
        /// </summary>
        public bool SamplerLodBias { get; }
        /// <summary>
        /// Indicates whether a non-zero "vertexStart" value can be used in
        /// <see cref="CommandList.Draw(uint, uint, uint, uint)"/> and
        /// <see cref="CommandList.DrawIndexed(uint, uint, uint, int, uint)"/>.
        /// </summary>
        public bool DrawBaseVertex { get; }
        /// <summary>
        /// Indicates whether a non-zero "instanceStart" value can be used in
        /// <see cref="CommandList.Draw(uint, uint, uint, uint)"/> and
        /// <see cref="CommandList.DrawIndexed(uint, uint, uint, int, uint)"/>.
        /// </summary>
        public bool DrawBaseInstance { get; }
        /// <summary>
        /// Indicates whether <see cref="PolygonFillMode.Wireframe"/> is supported.
        /// </summary>
        public bool FillModeWireframe { get; }
        /// <summary>
        /// Indicates whether <see cref="SamplerFilter.Anisotropic"/> is supported.
        /// </summary>
        public bool SamplerAnisotropy { get; }
        /// <summary>
        /// Indicates whether <see cref="RasterizerStateDescription.DepthClipEnabled"/> can be set to false.
        /// </summary>
        public bool DepthClipDisable { get; }
        /// <summary>
        /// Indicates whether a <see cref="Texture"/> can be created with <see cref="TextureType.Texture1D"/>.
        /// </summary>
        public bool Texture1D { get; }
        /// <summary>
        /// Indicates whether a <see cref="BlendStateDescription"/> can be used which has multiple different
        /// <see cref="BlendAttachmentDescription"/> values for each attachment. If false, all attachments must have the same
        /// blend state.
        /// </summary>
        public bool IndependentBlend { get; }

        internal GraphicsDeviceFeatures(
            bool computeShader,
            bool geometryShader,
            bool tessellationShaders,
            bool multipleViewports,
            bool samplerLodBias,
            bool drawBaseVertex,
            bool drawBaseInstance,
            bool fillModeWireframe,
            bool samplerAnisotropy,
            bool depthClipDisable,
            bool texture1D,
            bool independentBlend)
        {
            ComputeShader = computeShader;
            GeometryShader = geometryShader;
            TessellationShaders = tessellationShaders;
            MultipleViewports = multipleViewports;
            SamplerLodBias = samplerLodBias;
            DrawBaseVertex = drawBaseVertex;
            DrawBaseInstance = drawBaseInstance;
            FillModeWireframe = fillModeWireframe;
            SamplerAnisotropy = samplerAnisotropy;
            DepthClipDisable = depthClipDisable;
            Texture1D = texture1D;
            IndependentBlend = independentBlend;
        }
    }
}
