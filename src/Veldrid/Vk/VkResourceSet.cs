using System.Collections.Generic;
using TerraFX.Interop.Vulkan;
using static TerraFX.Interop.Vulkan.Vulkan;

namespace Veldrid.Vulkan
{
    internal sealed unsafe class VkResourceSet : ResourceSet, IResourceRefCountTarget
    {
        private readonly VkGraphicsDevice _gd;
        private readonly DescriptorResourceCounts _descriptorCounts;
        private readonly DescriptorAllocationToken _descriptorAllocationToken;
        private readonly List<ResourceRefCount> _refCounts = new();
        private bool _destroyed;
        private string? _name;

        public VkDescriptorSet DescriptorSet => _descriptorAllocationToken.Set;

        private readonly List<VkTexture> _sampledTextures = new();
        public List<VkTexture> SampledTextures => _sampledTextures;
        private readonly List<VkTexture> _storageImages = new();
        public List<VkTexture> StorageTextures => _storageImages;

        public ResourceRefCount RefCount { get; }
        public List<ResourceRefCount> RefCounts => _refCounts;

        public override bool IsDisposed => _destroyed;

        public VkResourceSet(VkGraphicsDevice gd, in ResourceSetDescription description)
            : base(description)
        {
            _gd = gd;
            RefCount = new ResourceRefCount(this);
            VkResourceLayout vkLayout = Util.AssertSubtype<ResourceLayout, VkResourceLayout>(description.Layout);

            VkDescriptorSetLayout dsl = vkLayout.DescriptorSetLayout;
            _descriptorCounts = vkLayout.DescriptorResourceCounts;
            _descriptorAllocationToken = _gd.DescriptorPoolManager.Allocate(_descriptorCounts, dsl);

            BindableResource[] boundResources = description.BoundResources;
            uint descriptorWriteCount = (uint)boundResources.Length;
            VkWriteDescriptorSet* descriptorWrites = stackalloc VkWriteDescriptorSet[(int)descriptorWriteCount];
            VkDescriptorBufferInfo* bufferInfos = stackalloc VkDescriptorBufferInfo[(int)descriptorWriteCount];
            VkDescriptorImageInfo* imageInfos = stackalloc VkDescriptorImageInfo[(int)descriptorWriteCount];

            for (int i = 0; i < descriptorWriteCount; i++)
            {
                VkDescriptorType type = vkLayout.DescriptorTypes[i];

                descriptorWrites[i] = new VkWriteDescriptorSet()
                {
                    sType = VkStructureType.VK_STRUCTURE_TYPE_WRITE_DESCRIPTOR_SET,
                    descriptorCount = 1,
                    descriptorType = type,
                    dstBinding = (uint)i,
                    dstSet = _descriptorAllocationToken.Set
                };

                if (type == VkDescriptorType.VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER ||
                    type == VkDescriptorType.VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER_DYNAMIC ||
                    type == VkDescriptorType.VK_DESCRIPTOR_TYPE_STORAGE_BUFFER ||
                    type == VkDescriptorType.VK_DESCRIPTOR_TYPE_STORAGE_BUFFER_DYNAMIC)
                {
                    DeviceBufferRange range = Util.GetBufferRange(boundResources[i], 0);
                    VkBuffer rangedVkBuffer = Util.AssertSubtype<DeviceBuffer, VkBuffer>(range.Buffer);
                    bufferInfos[i] = new VkDescriptorBufferInfo()
                    {
                        buffer = rangedVkBuffer.DeviceBuffer,
                        offset = range.Offset,
                        range = range.SizeInBytes
                    };
                    descriptorWrites[i].pBufferInfo = &bufferInfos[i];
                    _refCounts.Add(rangedVkBuffer.RefCount);
                }
                else if (type == VkDescriptorType.VK_DESCRIPTOR_TYPE_SAMPLED_IMAGE)
                {
                    TextureView texView = Util.GetTextureView(_gd, boundResources[i]);
                    VkTextureView vkTexView = Util.AssertSubtype<TextureView, VkTextureView>(texView);
                    imageInfos[i] = new VkDescriptorImageInfo()
                    {
                        imageView = vkTexView.ImageView,
                        imageLayout = VkImageLayout.VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL
                    };
                    descriptorWrites[i].pImageInfo = &imageInfos[i];
                    _sampledTextures.Add(Util.AssertSubtype<Texture, VkTexture>(texView.Target));
                    _refCounts.Add(vkTexView.RefCount);
                }
                else if (type == VkDescriptorType.VK_DESCRIPTOR_TYPE_STORAGE_IMAGE)
                {
                    TextureView texView = Util.GetTextureView(_gd, boundResources[i]);
                    VkTextureView vkTexView = Util.AssertSubtype<TextureView, VkTextureView>(texView);
                    imageInfos[i] = new VkDescriptorImageInfo()
                    {
                        imageView = vkTexView.ImageView,
                        imageLayout = VkImageLayout.VK_IMAGE_LAYOUT_GENERAL
                    };
                    descriptorWrites[i].pImageInfo = &imageInfos[i];
                    _storageImages.Add(Util.AssertSubtype<Texture, VkTexture>(texView.Target));
                    _refCounts.Add(vkTexView.RefCount);
                }
                else if (type == VkDescriptorType.VK_DESCRIPTOR_TYPE_SAMPLER)
                {
                    VkSampler sampler = Util.AssertSubtype<BindableResource, VkSampler>(boundResources[i]);
                    imageInfos[i] = new VkDescriptorImageInfo() { sampler = sampler.DeviceSampler };
                    descriptorWrites[i].pImageInfo = &imageInfos[i];
                    _refCounts.Add(sampler.RefCount);
                }
            }

            vkUpdateDescriptorSets(_gd.Device, descriptorWriteCount, descriptorWrites, 0, null);
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
            if (!_destroyed)
            {
                _destroyed = true;
                _gd.DescriptorPoolManager.Free(_descriptorAllocationToken, _descriptorCounts);
            }
        }
    }
}
