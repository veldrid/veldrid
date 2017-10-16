namespace Vd2
{
    public struct BlendStateDescription
    {
        public BlendAttachmentDescription[] AttachmentStates;

        public static readonly BlendStateDescription SingleOverrideBlend = new BlendStateDescription
        {
            AttachmentStates = new BlendAttachmentDescription[] { BlendAttachmentDescription.OverrideBlend }
        };
    }
}