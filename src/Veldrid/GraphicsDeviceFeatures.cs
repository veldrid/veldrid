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
        /// Indicates whether indirect draw commands can be issued.
        /// </summary>
        public bool DrawIndirect { get; }
        /// <summary>
        /// Indicates whether indirect draw structures stored in an Indirect DeviceBuffer can contain
        /// a non-zero FirstInstance value.
        /// </summary>
        public bool DrawIndirectBaseInstance { get; }
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
        /// <summary>
        /// Indicates whether <see cref="BufferUsage.StructuredBufferReadOnly"/> and
        /// <see cref="BufferUsage.StructuredBufferReadWrite"/> can be used. If false, structured buffers cannot be created.
        /// </summary>
        public bool StructuredBuffer { get; }
        /// <summary>
        /// Indicates whether a <see cref="TextureView"/> can be created which does not view the full set of mip levels and array
        /// layers contained in its target Texture, or uses a different <see cref="PixelFormat"/> from the underlying Texture.
        /// </summary>
        public bool SubsetTextureView { get; }
        /// <summary>
        /// Indicates whether <see cref="CommandList"/> instances created with this device support the
        /// <see cref="CommandList.PushDebugGroup(string)"/>, <see cref="CommandList.PopDebugGroup"/>, and
        /// <see cref="CommandList.InsertDebugMarker(string)"/> methods. If not, these methods will have no effect.
        /// </summary>
        public bool CommandListDebugMarkers { get; }
        /// <summary>
        /// Indicates whether uniform and structured buffers can be bound with an offset and a size. If false, buffer resources
        /// must be bound with their full range.
        /// </summary>
        public bool BufferRangeBinding { get; }
        /// <summary>
        /// Indicates whether 64-bit floating point integers can be used in shaders.
        /// </summary>
        public bool ShaderFloat64 { get; }

        internal GraphicsDeviceFeatures(
            bool computeShader,
            bool geometryShader,
            bool tessellationShaders,
            bool multipleViewports,
            bool samplerLodBias,
            bool drawBaseVertex,
            bool drawBaseInstance,
            bool drawIndirect,
            bool drawIndirectBaseInstance,
            bool fillModeWireframe,
            bool samplerAnisotropy,
            bool depthClipDisable,
            bool texture1D,
            bool independentBlend,
            bool structuredBuffer,
            bool subsetTextureView,
            bool commandListDebugMarkers,
            bool bufferRangeBinding,
            bool shaderFloat64)
        {
            ComputeShader = computeShader;
            GeometryShader = geometryShader;
            TessellationShaders = tessellationShaders;
            MultipleViewports = multipleViewports;
            SamplerLodBias = samplerLodBias;
            DrawBaseVertex = drawBaseVertex;
            DrawBaseInstance = drawBaseInstance;
            DrawIndirect = drawIndirect;
            DrawIndirectBaseInstance = drawIndirectBaseInstance;
            FillModeWireframe = fillModeWireframe;
            SamplerAnisotropy = samplerAnisotropy;
            DepthClipDisable = depthClipDisable;
            Texture1D = texture1D;
            IndependentBlend = independentBlend;
            StructuredBuffer = structuredBuffer;
            SubsetTextureView = subsetTextureView;
            CommandListDebugMarkers = commandListDebugMarkers;
            BufferRangeBinding = bufferRangeBinding;
            ShaderFloat64 = shaderFloat64;
        }
    }
}
