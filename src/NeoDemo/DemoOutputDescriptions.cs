using Vd2;

namespace Vd2.NeoDemo
{
    internal static class DemoOutputsDescriptions
    {
        public static OutputDescription ShadowMapPass { get; } = new OutputDescription(
            new OutputAttachmentDescription(PixelFormat.R16_UNorm));
    }
}