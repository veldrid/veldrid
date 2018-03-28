namespace Veldrid
{
    public class GraphicsDeviceFeatures
    {
        public bool ComputeShader { get; }
        public bool GeometryShader { get; }
        public bool TessellationShaders { get; }
        public bool MultipleViewports { get; }
        public bool SamplerLodBias { get; }
        public bool DrawBaseVertex { get; }
        public bool DrawBaseInstance { get; }
        public bool FillModeWireframe { get; }
        public bool SamplerAnisotropy { get; }
        public bool DepthClipDisable { get; }

        public GraphicsDeviceFeatures(
            bool computeShader, 
            bool geometryShader, 
            bool tessellationShaders, 
            bool multipleViewports, 
            bool samplerLodBias, 
            bool drawBaseVertex, 
            bool drawBaseInstance, 
            bool fillModeWireframe, 
            bool samplerAnisotropy, 
            bool depthClipDisable)
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
        }
    }
}
