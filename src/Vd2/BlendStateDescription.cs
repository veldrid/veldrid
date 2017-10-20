using System;

namespace Vd2
{
    public struct BlendStateDescription
    {
        public RgbaFloat BlendFactor;
        public BlendAttachmentDescription[] AttachmentStates;

        public static readonly BlendStateDescription SingleOverrideBlend = new BlendStateDescription
        {
            AttachmentStates = new BlendAttachmentDescription[] { BlendAttachmentDescription.OverrideBlend }
        };

        public static readonly BlendStateDescription SingleAlphaBlend = new BlendStateDescription
        {
            AttachmentStates = new BlendAttachmentDescription[] { BlendAttachmentDescription.AlphaBlend }
        };

        public static readonly BlendStateDescription Empty = new BlendStateDescription
        {
            AttachmentStates = Array.Empty<BlendAttachmentDescription>()
        };
    }
}