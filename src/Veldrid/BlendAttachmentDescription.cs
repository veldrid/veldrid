using System;

namespace Veldrid
{
    public struct BlendAttachmentDescription : IEquatable<BlendAttachmentDescription>
    {
        public bool BlendEnabled;
        public BlendFactor SourceColorFactor;
        public BlendFactor DestinationColorFactor;
        public BlendFunction ColorFunction;
        public BlendFactor SourceAlphaFactor;
        public BlendFactor DestinationAlphaFactor;
        public BlendFunction AlphaFunction;

        public static readonly BlendAttachmentDescription OverrideBlend = new BlendAttachmentDescription
        {
            SourceColorFactor = BlendFactor.One,
            DestinationColorFactor = BlendFactor.Zero,
            ColorFunction = BlendFunction.Add,
            SourceAlphaFactor = BlendFactor.One,
            DestinationAlphaFactor = BlendFactor.Zero,
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

        public bool Equals(BlendAttachmentDescription other)
        {
            return BlendEnabled.Equals(other.BlendEnabled) && SourceColorFactor == other.SourceColorFactor
                && DestinationColorFactor == other.DestinationColorFactor && ColorFunction == other.ColorFunction
                && SourceAlphaFactor == other.SourceAlphaFactor && DestinationAlphaFactor == other.DestinationAlphaFactor
                && AlphaFunction == other.AlphaFunction;
        }

        public override int GetHashCode()
        {
            return HashHelper.Combine(
                BlendEnabled.GetHashCode(),
                SourceColorFactor.GetHashCode(),
                DestinationColorFactor.GetHashCode(),
                ColorFunction.GetHashCode(),
                SourceAlphaFactor.GetHashCode(),
                DestinationAlphaFactor.GetHashCode(),
                AlphaFunction.GetHashCode());
        }
    }
}
