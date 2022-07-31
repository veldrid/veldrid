using TerraFX.Interop.Vulkan;
using static TerraFX.Interop.Vulkan.Vulkan;
using VulkanSampler = TerraFX.Interop.Vulkan.VkSampler;

namespace Veldrid.Vulkan
{
    internal sealed unsafe class VkSampler : Sampler, IResourceRefCountTarget
    {
        private readonly VkGraphicsDevice _gd;
        private readonly VulkanSampler _sampler;
        private bool _disposed;
        private string? _name;

        public VulkanSampler DeviceSampler => _sampler;

        public ResourceRefCount RefCount { get; }

        public override bool IsDisposed => _disposed;

        public VkSampler(VkGraphicsDevice gd, in SamplerDescription description)
        {
            _gd = gd;
            VkFormats.GetFilterParams(
                description.Filter, out VkFilter minFilter, out VkFilter magFilter, out VkSamplerMipmapMode mipmapMode);

            VkSamplerCreateInfo samplerCI = new()
            {
                sType = VkStructureType.VK_STRUCTURE_TYPE_SAMPLER_CREATE_INFO,
                addressModeU = VkFormats.VdToVkSamplerAddressMode(description.AddressModeU),
                addressModeV = VkFormats.VdToVkSamplerAddressMode(description.AddressModeV),
                addressModeW = VkFormats.VdToVkSamplerAddressMode(description.AddressModeW),
                minFilter = minFilter,
                magFilter = magFilter,
                mipmapMode = mipmapMode,
                compareEnable = description.ComparisonKind != null,
                compareOp = description.ComparisonKind != null
                    ? VkFormats.VdToVkCompareOp(description.ComparisonKind.Value)
                    : VkCompareOp.VK_COMPARE_OP_NEVER,
                anisotropyEnable = description.Filter == SamplerFilter.Anisotropic,
                maxAnisotropy = description.MaximumAnisotropy,
                minLod = description.MinimumLod,
                maxLod = description.MaximumLod,
                mipLodBias = description.LodBias,
                borderColor = VkFormats.VdToVkSamplerBorderColor(description.BorderColor)
            };

            VulkanSampler sampler;
            vkCreateSampler(_gd.Device, &samplerCI, null, &sampler);
            _sampler = sampler;
            RefCount = new ResourceRefCount(this);
        }

        public override string? Name
        {
            get => _name;
            set
            {
                _name = value;
                _gd.SetResourceName(this, value);
            }
        }

        public override void Dispose()
        {
            RefCount.Decrement();
        }

        void IResourceRefCountTarget.RefZeroed()
        {
            if (!_disposed)
            {
                vkDestroySampler(_gd.Device, _sampler, null);
                _disposed = true;
            }
        }
    }
}
