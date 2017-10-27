using Veldrid;

namespace Veldrid.NeoDemo
{
    internal static class DemoOutputsDescriptions
    {
        public static OutputDescription ShadowMapPass { get; } = new OutputDescription(
            new OutputAttachmentDescription(PixelFormat.R16_UNorm));
    }
}