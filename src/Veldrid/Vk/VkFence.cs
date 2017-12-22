using Vulkan;
using static Vulkan.VulkanNative;

namespace Veldrid.Vk
{
    internal unsafe class VkFence : Fence, VkDeferredDisposal
    {
        private readonly VkGraphicsDevice _gd;
        private Vulkan.VkFence _fence;
        private string _name;

        public Vulkan.VkFence DeviceFence => _fence;

        public VkFence(VkGraphicsDevice gd, bool signaled)
        {
            _gd = gd;
            VkFenceCreateInfo fenceCI = VkFenceCreateInfo.New();
            fenceCI.flags = signaled ? VkFenceCreateFlags.Signaled : VkFenceCreateFlags.None;
            VkResult result = vkCreateFence(_gd.Device, ref fenceCI, null, out _fence);
            VulkanUtil.CheckResult(result);
        }

        public override void Reset()
        {
            _gd.ResetFence(this);
        }

        public override bool Signaled => vkGetFenceStatus(_gd.Device, _fence) == VkResult.Success;

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
            vkDestroyFence(_gd.Device, _fence, null);
        }

    }
}
