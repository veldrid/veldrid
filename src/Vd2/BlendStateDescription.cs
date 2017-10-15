namespace Vd2
{
    public struct BlendStateDescription
    {
        public BlendAttachmentDescription[] AttachmentStates;

        public static readonly BlendStateDescription SingleAdditiveBlend = new BlendStateDescription
        {
            AttachmentStates = new BlendAttachmentDescription[] { BlendAttachmentDescription.AdditiveBlend }
        };
    }
}