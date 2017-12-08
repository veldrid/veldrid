using Veldrid;

namespace Veldrid.NeoDemo
{
    internal static class DemoOutputsDescriptions
    {
        public static OutputDescription ShadowMapPass { get; } = new OutputDescription(
            new OutputAttachmentDescription(PixelFormat.D24_UNorm_S8_UInt));
    }
}