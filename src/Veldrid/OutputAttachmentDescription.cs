using System;

namespace Veldrid
{
    public struct OutputAttachmentDescription : IEquatable<OutputAttachmentDescription>
    {
        public PixelFormat Format;

        public OutputAttachmentDescription(PixelFormat format)
        {
            Format = format;
        }

        public bool Equals(OutputAttachmentDescription other)
        {
            return Format == other.Format;
        }

        public override int GetHashCode()
        {
            return Format.GetHashCode();
        }
    }
}