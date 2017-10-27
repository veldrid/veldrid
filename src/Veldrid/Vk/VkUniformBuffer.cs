namespace Veldrid.Vk
{
    internal class VkUniformBuffer : VkBuffer, UniformBuffer
    {
        public VkUniformBuffer(VkGraphicsDevice gd, ref BufferDescription description)
            : base(gd, description.SizeInBytes, description.Dynamic, Vulkan.VkBufferUsageFlags.UniformBuffer)
        {
        }
    }
}