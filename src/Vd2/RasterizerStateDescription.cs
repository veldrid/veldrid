namespace Vd2
{
    public struct RasterizerStateDescription
    {
        public FaceCullMode CullMode;
        public TriangleFillMode FillMode;
        public FrontFace FrontFace;
        public bool DepthClipEnabled;
        public bool ScissorTestEnabled;

        public RasterizerStateDescription(
            FaceCullMode cullMode,
            TriangleFillMode fillMode,
            FrontFace frontFace,
            bool depthClipEnabled,
            bool scissorTestEnabled)
        {
            CullMode = cullMode;
            FillMode = fillMode;
            FrontFace = frontFace;
            DepthClipEnabled = depthClipEnabled;
            ScissorTestEnabled = scissorTestEnabled;
        }

        public static readonly RasterizerStateDescription Default = new RasterizerStateDescription
        {
            CullMode = FaceCullMode.Back,
            FillMode = TriangleFillMode.Solid,
            FrontFace = FrontFace.Clockwise,
            DepthClipEnabled = true,
            ScissorTestEnabled = true,
        };
    }
}
