using Vulkan;
using static Vulkan.VulkanNative;

namespace Veldrid.Vk
{
    internal unsafe class VkFence : Fence
    {
        private readonly VkGraphicsDevice _gd;
        private Vulkan.VkFence _fence;
        private string _name;
        private bool _disposed;

        public Vulkan.VkFence DeviceFence => _fence;

        public VkFence(VkGraphicsDevice gd, bool signaled)
            : base(gd)
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

        public override void Dispose()
        {
            if (!_disposed)
            {
                vkDestroyFence(_gd.Device, _fence, null);
                _disposed = true;
            }
        }
    }
}
