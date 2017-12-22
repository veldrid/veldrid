using static Vulkan.VulkanNative;
using Vulkan;

namespace Veldrid.Vk
{
    internal unsafe class VkSemaphore : Semaphore
    {
        private readonly VkGraphicsDevice _gd;
        private Vulkan.VkSemaphore _semaphore;
        private string _name;
        private bool _destroyed;

        public Vulkan.VkSemaphore DeviceSemaphore => _semaphore;

        public VkSemaphore(VkGraphicsDevice gd)
        {
            _gd = gd;
            VkSemaphoreCreateInfo semaphoreCI = VkSemaphoreCreateInfo.New();
            VkResult result = vkCreateSemaphore(gd.Device, ref semaphoreCI, null, out _semaphore);
            VulkanUtil.CheckResult(result);
        }

        public override string Name
        {
            get => _name;
            set
            {
                _name = value; _gd.SetResourceName(this, value);
            }
        }

        public override void Dispose()
        {
            if (!_destroyed)
            {
                vkDestroySemaphore(_gd.Device, _semaphore, null);
                _destroyed = true;
            }
        }
    }
}
