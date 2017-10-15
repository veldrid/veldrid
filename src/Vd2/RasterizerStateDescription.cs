namespace Vd2
{
    public struct RasterizerStateDescription
    {
        public FaceCullMode CullMode;
        public TriangleFillMode FillMode;
        public bool DepthClipEnabled;
        public bool ScissorTestEnabled;

        public static readonly RasterizerStateDescription Default = new RasterizerStateDescription
        {
            CullMode = FaceCullMode.Back,
            DepthClipEnabled = true,
            FillMode = TriangleFillMode.Solid,
            ScissorTestEnabled = true,
        };
    }
}
