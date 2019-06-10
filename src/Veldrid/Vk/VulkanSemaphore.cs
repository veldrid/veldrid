using Vulkan;

using static Vulkan.VulkanNative;
using static Veldrid.Vk.VulkanUtil;

namespace Veldrid.Vk
{
    internal unsafe class VulkanSemaphore : Semaphore
    {
        private readonly VkGraphicsDevice _gd;
        private readonly ResourceRefCount _refCount;
        private readonly VkSemaphore _semaphore;
        public VkSemaphore NativeSemaphore => _semaphore;

        public VulkanSemaphore(VkGraphicsDevice gd)
        {
            _gd = gd;
            _refCount = new ResourceRefCount(DisposeCore);
            VkSemaphoreCreateInfo semaphoreCI = VkSemaphoreCreateInfo.New();
            VkResult result = vkCreateSemaphore(_gd.Device, &semaphoreCI, null, out _semaphore);
            CheckResult(result);
        }

        public override void Dispose() => _refCount.Decrement();

        private void DisposeCore()
        {
            vkDestroySemaphore(_gd.Device, NativeSemaphore, null);
        }
    }
}
