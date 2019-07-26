using System.Collections.Generic;
using Vulkan;
using static Vulkan.VulkanNative;
using static Veldrid.Vk.VulkanUtil;

namespace Veldrid.Vk
{
    internal unsafe class VkResourceSet : ResourceSet
    {
        private readonly VkGraphicsDevice _gd;
        private readonly DescriptorResourceCounts _descriptorCounts;
        private readonly DescriptorAllocationToken _descriptorAllocationToken;
        private readonly List<ResourceRefCount> _refCounts = new List<ResourceRefCount>();
        private bool _destroyed;
        private string _name;

        public VkDescriptorSet DescriptorSet => _descriptorAllocationToken.Set;

        private readonly List<VkTexture> _sampledTextures = new List<VkTexture>();
        public IReadOnlyList<VkTexture> SampledTextures => _sampledTextures;
        private readonly List<VkTexture> _storageImages = new List<VkTexture>();
        public IReadOnlyList<VkTexture> StorageTextures => _storageImages;

        public ResourceRefCount RefCount { get; }
        public IReadOnlyList<ResourceRefCount> RefCounts => _refCounts;

        public VkResourceSet(VkGraphicsDevice gd, ref ResourceSetDescription description)
            : base(ref description)
        {
            _gd = gd;
            RefCount = new ResourceRefCount(DisposeCore);
            VkResourceLayout vkLayout = Util.AssertSubtype<ResourceLayout, VkResourceLayout>(description.Layout);

            VkDescriptorSetLayout dsl = vkLayout.DescriptorSetLayout;
            _descriptorCounts = vkLayout.DescriptorResourceCounts;
            _descriptorAllocationToken = _gd.DescriptorPoolManager.Allocate(_descriptorCounts, dsl);

            BindableResource[] boundResources = description.BoundResources;
            uint maxDescriptorWrites = (uint)boundResources.Length;
            VkWriteDescriptorSet* descriptorWrites = stackalloc VkWriteDescriptorSet[(int)maxDescriptorWrites];
            VkDescriptorBufferInfo* bufferInfos = stackalloc VkDescriptorBufferInfo[(int)maxDescriptorWrites];
            VkDescriptorImageInfo* imageInfos = stackalloc VkDescriptorImageInfo[(int)maxDescriptorWrites];

            uint writeIndex = 0;
            for (int i = 0; i < maxDescriptorWrites; i++)
            {
                if (vkLayout.DescriptorTypes[i] == null) { continue; } // Unused resource slot.

                VkDescriptorType type = vkLayout.DescriptorTypes[i].Value;

                descriptorWrites[writeIndex].sType = VkStructureType.WriteDescriptorSet;
                descriptorWrites[writeIndex].descriptorCount = 1;
                descriptorWrites[writeIndex].descriptorType = type;
                descriptorWrites[writeIndex].dstBinding = (uint)i;
                descriptorWrites[writeIndex].dstSet = _descriptorAllocationToken.Set;

                if (type == VkDescriptorType.UniformBuffer || type == VkDescriptorType.UniformBufferDynamic
                    || type == VkDescriptorType.StorageBuffer || type == VkDescriptorType.StorageBufferDynamic)
                {
                    DeviceBufferRange range = Util.GetBufferRange(boundResources[i], 0);
                    VkBuffer rangedVkBuffer = Util.AssertSubtype<DeviceBuffer, VkBuffer>(range.Buffer);
                    bufferInfos[writeIndex].buffer = rangedVkBuffer.DeviceBuffer;
                    bufferInfos[writeIndex].offset = range.Offset;
                    bufferInfos[writeIndex].range = range.SizeInBytes;
                    descriptorWrites[writeIndex].pBufferInfo = &bufferInfos[writeIndex];
                    _refCounts.Add(rangedVkBuffer.RefCount);
                }
                else if (type == VkDescriptorType.SampledImage)
                {
                    TextureView texView = Util.GetTextureView(_gd, boundResources[i]);
                    VkTextureView vkTexView = Util.AssertSubtype<TextureView, VkTextureView>(texView);
                    imageInfos[writeIndex].imageView = vkTexView.ImageView;
                    imageInfos[writeIndex].imageLayout = VkImageLayout.ShaderReadOnlyOptimal;
                    descriptorWrites[writeIndex].pImageInfo = &imageInfos[writeIndex];
                    _sampledTextures.Add(Util.AssertSubtype<Texture, VkTexture>(texView.Target));
                    _refCounts.Add(vkTexView.RefCount);
                }
                else if (type == VkDescriptorType.StorageImage)
                {
                    TextureView texView = Util.GetTextureView(_gd, boundResources[i]);
                    VkTextureView vkTexView = Util.AssertSubtype<TextureView, VkTextureView>(texView);
                    imageInfos[writeIndex].imageView = vkTexView.ImageView;
                    imageInfos[writeIndex].imageLayout = VkImageLayout.General;
                    descriptorWrites[writeIndex].pImageInfo = &imageInfos[writeIndex];
                    _storageImages.Add(Util.AssertSubtype<Texture, VkTexture>(texView.Target));
                    _refCounts.Add(vkTexView.RefCount);
                }
                else if (type == VkDescriptorType.Sampler)
                {
                    VkSampler sampler = Util.AssertSubtype<BindableResource, VkSampler>(boundResources[i]);
                    imageInfos[writeIndex].sampler = sampler.DeviceSampler;
                    descriptorWrites[writeIndex].pImageInfo = &imageInfos[writeIndex];
                    _refCounts.Add(sampler.RefCount);
                }

                writeIndex += 1;
            }

            vkUpdateDescriptorSets(_gd.Device, writeIndex, descriptorWrites, 0, null);
        }

        public override string Name
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

        private void DisposeCore()
        {
            if (!_destroyed)
            {
                _destroyed = true;
                _gd.DescriptorPoolManager.Free(_descriptorAllocationToken, _descriptorCounts);
            }
        }
    }
}
