namespace Vd2
{
    public struct BlendAttachmentDescription
    {
        public bool BlendEnabled;
        public BlendFactor SourceColorFactor;
        public BlendFactor DestinationColorFactor;
        public BlendFunction ColorFunction;
        public BlendFactor SourceAlphaFactor;
        public BlendFactor DestinationAlphaFactor;
        public BlendFunction AlphaFunction;

        public static readonly BlendAttachmentDescription AdditiveBlend = new BlendAttachmentDescription
        {
            SourceColorFactor = BlendFactor.SourceAlpha,
            DestinationColorFactor = BlendFactor.One,
            ColorFunction = BlendFunction.Add,
            SourceAlphaFactor = BlendFactor.SourceAlpha,
            DestinationAlphaFactor = BlendFactor.One,
            AlphaFunction = BlendFunction.Add,
            BlendEnabled = true,
        };

        public static readonly BlendAttachmentDescription AlphaBlend = new BlendAttachmentDescription
        {
            SourceColorFactor = BlendFactor.SourceAlpha,
            DestinationColorFactor = BlendFactor.InverseSourceAlpha,
            ColorFunction = BlendFunction.Add,
            SourceAlphaFactor = BlendFactor.SourceAlpha,
            DestinationAlphaFactor = BlendFactor.InverseSourceAlpha,
            AlphaFunction = BlendFunction.Add,
            BlendEnabled = true,
        };

    }
}
