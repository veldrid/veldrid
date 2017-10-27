namespace Veldrid.Vk
{
    internal class VkVertexBuffer : VkBuffer, VertexBuffer
    {
        public VkVertexBuffer(VkGraphicsDevice gd, ref BufferDescription description)
            : base(gd, description.SizeInBytes, description.Dynamic, Vulkan.VkBufferUsageFlags.VertexBuffer)
        {
        }
    }
}
