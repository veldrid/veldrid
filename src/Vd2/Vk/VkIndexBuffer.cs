namespace Vd2.Vk
{
    internal class VkIndexBuffer : VkBuffer, IndexBuffer
    {
        public IndexFormat Format { get; }

        public VkIndexBuffer(VkGraphicsDevice gd, ref IndexBufferDescription description)
            : base(gd, description.SizeInBytes, Vulkan.VkBufferUsageFlags.IndexBuffer)
        {
            Format = description.Format;
        }
    }
}
