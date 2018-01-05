using Veldrid;

namespace Veldrid.NeoDemo
{
    internal static class DemoOutputsDescriptions
    {
        public static OutputDescription ShadowMapPass { get; } = new OutputDescription(
            new OutputAttachmentDescription(PixelFormat.D32_Float_S8_UInt));
    }
}