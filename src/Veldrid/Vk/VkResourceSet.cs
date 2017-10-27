using Vulkan;
using static Vulkan.VulkanNative;
using static Veldrid.Vk.VulkanUtil;

namespace Veldrid.Vk
{
    internal unsafe class VkResourceSet : ResourceSet
    {
        private readonly VkGraphicsDevice _gd;
        private readonly VkDescriptorSet _descriptorSet;

        public VkDescriptorSet DescriptorSet => _descriptorSet;

        public VkResourceSet(VkGraphicsDevice gd, ref ResourceSetDescription description)
        {
            _gd = gd;
            VkResourceLayout vkLayout = Util.AssertSubtype<ResourceLayout, VkResourceLayout>(description.Layout);

            VkDescriptorSetAllocateInfo dsAI = VkDescriptorSetAllocateInfo.New();
            dsAI.descriptorSetCount = 1;
            VkDescriptorSetLayout dsl = vkLayout.DescriptorSetLayout;
            dsAI.pSetLayouts = &dsl;
            dsAI.descriptorPool = _gd.SharedDescriptorPool;
            VkResult result = vkAllocateDescriptorSets(_gd.Device, ref dsAI, out _descriptorSet);
            CheckResult(result);

            BindableResource[] boundResources = description.BoundResources;
            uint descriptorWriteCount = (uint)boundResources.Length;
            VkWriteDescriptorSet* descriptorWrites = stackalloc VkWriteDescriptorSet[(int)descriptorWriteCount];
            VkDescriptorBufferInfo* bufferInfos = stackalloc VkDescriptorBufferInfo[(int)descriptorWriteCount];
            VkDescriptorImageInfo* imageInfos = stackalloc VkDescriptorImageInfo[(int)descriptorWriteCount];

            for (int i = 0; i < descriptorWriteCount; i++)
            {
                VkDescriptorType type = vkLayout.DescriptorTypes[i];

                descriptorWrites[i].sType = VkStructureType.WriteDescriptorSet;
                descriptorWrites[i].descriptorCount = 1;
                descriptorWrites[i].descriptorType = type;
                descriptorWrites[i].dstBinding = (uint)i;
                descriptorWrites[i].dstSet = _descriptorSet;

                if (type == VkDescriptorType.UniformBuffer)
                {
                    VkUniformBuffer uniformBuffer = Util.AssertSubtype<BindableResource, VkUniformBuffer>(boundResources[i]);
                    bufferInfos[i].buffer = uniformBuffer.DeviceBuffer;
                    bufferInfos[i].range = uniformBuffer.SizeInBytes;
                    descriptorWrites[i].pBufferInfo = &bufferInfos[i];
                }
                else if (type == VkDescriptorType.SampledImage)
                {
                    VkTextureView textureView = Util.AssertSubtype<BindableResource, VkTextureView>(boundResources[i]);
                    imageInfos[i].imageView = textureView.ImageView;
                    imageInfos[i].imageLayout = VkImageLayout.ShaderReadOnlyOptimal;
                    descriptorWrites[i].pImageInfo = &imageInfos[i];
                }
                else if (type == VkDescriptorType.Sampler)
                {
                    VkSampler sampler = Util.AssertSubtype<BindableResource, VkSampler>(boundResources[i]);
                    imageInfos[i].sampler = sampler.DeviceSampler;
                    descriptorWrites[i].pImageInfo = &imageInfos[i];
                }
            }

            vkUpdateDescriptorSets(_gd.Device, descriptorWriteCount, descriptorWrites, 0, null);
        }

        public override void Dispose()
        {
            VkDescriptorSet ds = _descriptorSet;
            vkFreeDescriptorSets(_gd.Device, _gd.SharedDescriptorPool, 1, ref ds);
        }
    }
}