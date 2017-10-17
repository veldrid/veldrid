namespace Vd2
{
    public struct RasterizerStateDescription
    {
        public FaceCullMode CullMode;
        public TriangleFillMode FillMode;
        public bool DepthClipEnabled;
        public bool ScissorTestEnabled;

        public RasterizerStateDescription(
            FaceCullMode cullMode,
            TriangleFillMode fillMode,
            bool depthClipEnabled,
            bool scissorTestEnabled)
        {
            CullMode = cullMode;
            FillMode = fillMode;
            DepthClipEnabled = depthClipEnabled;
            ScissorTestEnabled = scissorTestEnabled;
        }

        public static readonly RasterizerStateDescription Default = new RasterizerStateDescription
        {
            CullMode = FaceCullMode.Back,
            DepthClipEnabled = true,
            FillMode = TriangleFillMode.Solid,
            ScissorTestEnabled = true,
        };
    }
}
