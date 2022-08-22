using TerraFX.Interop.Vulkan;
using static TerraFX.Interop.Vulkan.Vulkan;
using static Veldrid.Vulkan.VulkanUtil;

namespace Veldrid.Vulkan
{
    internal sealed unsafe class VkResourceLayout : ResourceLayout
    {
        private readonly VkGraphicsDevice _gd;
        private readonly VkDescriptorSetLayout _dsl;
        private readonly VkDescriptorType[] _descriptorTypes;
        private bool _disposed;
        private string? _name;

        public VkDescriptorSetLayout DescriptorSetLayout => _dsl;
        public VkDescriptorType[] DescriptorTypes => _descriptorTypes;
        public DescriptorResourceCounts DescriptorResourceCounts { get; }
        public new int DynamicBufferCount { get; }

        public override bool IsDisposed => _disposed;

        public VkResourceLayout(VkGraphicsDevice gd, in ResourceLayoutDescription description)
            : base(description)
        {
            _gd = gd;
            ResourceLayoutElementDescription[] elements = description.Elements;
            _descriptorTypes = new VkDescriptorType[elements.Length];
            VkDescriptorSetLayoutBinding* bindings = stackalloc VkDescriptorSetLayoutBinding[elements.Length];

            uint uniformBufferCount = 0;
            uint uniformBufferDynamicCount = 0;
            uint sampledImageCount = 0;
            uint samplerCount = 0;
            uint storageBufferCount = 0;
            uint storageBufferDynamicCount = 0;
            uint storageImageCount = 0;

            for (uint i = 0; i < elements.Length; i++)
            {
                bindings[i].binding = i;
                bindings[i].descriptorCount = 1;
                VkDescriptorType descriptorType = VkFormats.VdToVkDescriptorType(elements[i].Kind, elements[i].Options);
                bindings[i].descriptorType = descriptorType;
                bindings[i].stageFlags = VkFormats.VdToVkShaderStages(elements[i].Stages);
                if ((elements[i].Options & ResourceLayoutElementOptions.DynamicBinding) != 0)
                {
                    DynamicBufferCount += 1;
                }

                _descriptorTypes[i] = descriptorType;

                switch (descriptorType)
                {
                    case VkDescriptorType.VK_DESCRIPTOR_TYPE_SAMPLER:
                        samplerCount += 1;
                        break;
                    case VkDescriptorType.VK_DESCRIPTOR_TYPE_SAMPLED_IMAGE:
                        sampledImageCount += 1;
                        break;
                    case VkDescriptorType.VK_DESCRIPTOR_TYPE_STORAGE_IMAGE:
                        storageImageCount += 1;
                        break;
                    case VkDescriptorType.VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER:
                        uniformBufferCount += 1;
                        break;
                    case VkDescriptorType.VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER_DYNAMIC:
                        uniformBufferDynamicCount += 1;
                        break;
                    case VkDescriptorType.VK_DESCRIPTOR_TYPE_STORAGE_BUFFER:
                        storageBufferCount += 1;
                        break;
                    case VkDescriptorType.VK_DESCRIPTOR_TYPE_STORAGE_BUFFER_DYNAMIC:
                        storageBufferDynamicCount += 1;
                        break;
                }
            }

            DescriptorResourceCounts = new DescriptorResourceCounts(
                uniformBufferCount,
                uniformBufferDynamicCount,
                sampledImageCount,
                samplerCount,
                storageBufferCount,
                storageBufferDynamicCount,
                storageImageCount);

            VkDescriptorSetLayoutCreateInfo dslCI = new()
            {
                sType = VkStructureType.VK_STRUCTURE_TYPE_DESCRIPTOR_SET_LAYOUT_CREATE_INFO,
                bindingCount = (uint)elements.Length,
                pBindings = bindings
            };

            VkDescriptorSetLayout dsl;
            VkResult result = vkCreateDescriptorSetLayout(_gd.Device, &dslCI, null, &dsl);
            CheckResult(result);
            _dsl = dsl;
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
            if (!_disposed)
            {
                _disposed = true;
                vkDestroyDescriptorSetLayout(_gd.Device, _dsl, null);
            }
        }
    }
}
