using static Vulkan.VulkanNative;
using Vulkan;

namespace Veldrid.Vk
{
    internal unsafe class VkSemaphore : Semaphore, VkDeferredDisposal
    {
        private readonly VkGraphicsDevice _gd;
        private Vulkan.VkSemaphore _semaphore;
        private string _name;

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

        public ReferenceTracker ReferenceTracker { get; } = new ReferenceTracker();

        public override void Dispose()
        {
            _gd.DeferredDisposal(this);
        }

        public void DestroyResources()
        {
            vkDestroySemaphore(_gd.Device, _semaphore, null);
        }
    }
}
