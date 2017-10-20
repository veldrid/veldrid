namespace Vd2.Vk
{
    internal class VkUniformBuffer : VkBuffer, UniformBuffer
    {
        public VkUniformBuffer(VkGraphicsDevice gd, ref BufferDescription description)
            : base(gd, description.SizeInBytes, Vulkan.VkBufferUsageFlags.UniformBuffer)
        {
        }
    }
}