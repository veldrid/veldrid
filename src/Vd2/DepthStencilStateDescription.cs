namespace Vd2
{
    public struct DepthStencilStateDescription
    {
        public bool DepthTestEnabled;
        public bool DepthWriteEnabled;
        public DepthComparisonKind ComparisonKind;

        public static readonly DepthStencilStateDescription LessEqual = new DepthStencilStateDescription
        {
            DepthTestEnabled = true,
            DepthWriteEnabled = true,
            ComparisonKind = DepthComparisonKind.LessEqual
        };
    }
}