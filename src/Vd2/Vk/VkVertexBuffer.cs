namespace Vd2.Vk
{
    internal class VkVertexBuffer : VkBuffer, VertexBuffer
    {
        public VkVertexBuffer(VkGraphicsDevice gd, ref BufferDescription description)
            : base(gd, description.SizeInBytes, Vulkan.VkBufferUsageFlags.VertexBuffer)
        {
        }
    }
}
