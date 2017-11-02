using Vulkan;
using static Vulkan.VulkanNative;
using static Veldrid.Vk.VulkanUtil;
using System;
using System.Diagnostics;

namespace Veldrid.Vk
{
    internal unsafe class VkPipeline : Pipeline
    {
        private readonly VkGraphicsDevice _gd;
        private readonly Vulkan.VkPipeline _devicePipeline;
        private readonly VkPipelineLayout _pipelineLayout;
        private readonly VkRenderPass _renderPass;
        private bool _disposed;

        public Vulkan.VkPipeline DevicePipeline => _devicePipeline;

        public VkPipelineLayout PipelineLayout => _pipelineLayout;

        public uint ResourceSetCount { get; }

        public VkPipeline(VkGraphicsDevice gd, ref PipelineDescription description)
        {
            _gd = gd;

            VkGraphicsPipelineCreateInfo pipelineCI = VkGraphicsPipelineCreateInfo.New();

            // Blend State
            VkPipelineColorBlendStateCreateInfo blendStateCI = VkPipelineColorBlendStateCreateInfo.New();
            int attachmentsCount = description.BlendState.AttachmentStates.Length;
            VkPipelineColorBlendAttachmentState* attachmentsPtr
                = stackalloc VkPipelineColorBlendAttachmentState[attachmentsCount];
            for (int i = 0; i < attachmentsCount; i++)
            {
                BlendAttachmentDescription vdDesc = description.BlendState.AttachmentStates[i];
                VkPipelineColorBlendAttachmentState attachmentState = new VkPipelineColorBlendAttachmentState();
                attachmentState.srcColorBlendFactor = VkFormats.VdToVkBlendFactor(vdDesc.SourceColorFactor);
                attachmentState.dstColorBlendFactor = VkFormats.VdToVkBlendFactor(vdDesc.DestinationColorFactor);
                attachmentState.colorBlendOp = VkFormats.VdToVkBlendOp(vdDesc.ColorFunction);
                attachmentState.srcAlphaBlendFactor = VkFormats.VdToVkBlendFactor(vdDesc.SourceAlphaFactor);
                attachmentState.dstAlphaBlendFactor = VkFormats.VdToVkBlendFactor(vdDesc.DestinationAlphaFactor);
                attachmentState.alphaBlendOp = VkFormats.VdToVkBlendOp(vdDesc.AlphaFunction);
                attachmentState.blendEnable = vdDesc.BlendEnabled;
                attachmentState.colorWriteMask = VkColorComponentFlags.R | VkColorComponentFlags.G | VkColorComponentFlags.B | VkColorComponentFlags.A;
                attachmentsPtr[i] = attachmentState;
            }

            blendStateCI.attachmentCount = (uint)attachmentsCount;
            blendStateCI.pAttachments = attachmentsPtr;
            RgbaFloat blendFactor = description.BlendState.BlendFactor;
            blendStateCI.blendConstants_0 = blendFactor.R;
            blendStateCI.blendConstants_1 = blendFactor.G;
            blendStateCI.blendConstants_2 = blendFactor.B;
            blendStateCI.blendConstants_3 = blendFactor.A;

            pipelineCI.pColorBlendState = &blendStateCI;

            // Rasterizer State
            RasterizerStateDescription rsDesc = description.RasterizerState;
            VkPipelineRasterizationStateCreateInfo rsCI = VkPipelineRasterizationStateCreateInfo.New();
            rsCI.cullMode = VkFormats.VdToVkCullMode(rsDesc.CullMode);
            rsCI.polygonMode = VkFormats.VdToVkPolygonMode(rsDesc.FillMode);
            rsCI.depthClampEnable = !rsDesc.DepthClipEnabled;
            rsCI.frontFace = VkFrontFace.Clockwise;
            rsCI.lineWidth = 1f;

            pipelineCI.pRasterizationState = &rsCI;

            // Dynamic State
            VkPipelineDynamicStateCreateInfo dynamicStateCI = VkPipelineDynamicStateCreateInfo.New();
            VkDynamicState* dynamicStates = stackalloc VkDynamicState[2];
            dynamicStates[0] = VkDynamicState.Viewport;
            dynamicStates[1] = VkDynamicState.Scissor;
            dynamicStateCI.dynamicStateCount = 2;
            dynamicStateCI.pDynamicStates = dynamicStates;

            pipelineCI.pDynamicState = &dynamicStateCI;

            // Depth Stencil State
            DepthStencilStateDescription vdDssDesc = description.DepthStencilState;
            VkPipelineDepthStencilStateCreateInfo dssCI = VkPipelineDepthStencilStateCreateInfo.New();
            dssCI.depthWriteEnable = vdDssDesc.DepthWriteEnabled;
            dssCI.depthTestEnable = vdDssDesc.DepthTestEnabled;
            dssCI.depthCompareOp = VkFormats.VdToVkCompareOp(vdDssDesc.ComparisonKind);

            pipelineCI.pDepthStencilState = &dssCI;

            // Multisample
            VkPipelineMultisampleStateCreateInfo multisampleCI = VkPipelineMultisampleStateCreateInfo.New();
            multisampleCI.rasterizationSamples = VkSampleCountFlags.Count1;

            pipelineCI.pMultisampleState = &multisampleCI;

            // Input Assembly
            VkPipelineInputAssemblyStateCreateInfo inputAssemblyCI = VkPipelineInputAssemblyStateCreateInfo.New();
            inputAssemblyCI.topology = VkFormats.VdToVkPrimitiveTopology(description.PrimitiveTopology);

            pipelineCI.pInputAssemblyState = &inputAssemblyCI;

            // Vertex Input State
            VkPipelineVertexInputStateCreateInfo vertexInputCI = VkPipelineVertexInputStateCreateInfo.New();

            VertexLayoutDescription[] inputDescriptions = description.ShaderSet.VertexLayouts;
            uint bindingCount = (uint)inputDescriptions.Length;
            uint attributeCount = 0;
            for (int i = 0; i < inputDescriptions.Length; i++)
            {
                attributeCount += (uint)inputDescriptions[i].Elements.Length;
            }
            VkVertexInputBindingDescription* bindingDescs = stackalloc VkVertexInputBindingDescription[(int)bindingCount];
            VkVertexInputAttributeDescription* attributeDescs = stackalloc VkVertexInputAttributeDescription[(int)attributeCount];

            int targetIndex = 0;
            int targetLocation = 0;
            for (int binding = 0; binding < inputDescriptions.Length; binding++)
            {
                VertexLayoutDescription inputDesc = inputDescriptions[binding];
                bindingDescs[targetIndex] = new VkVertexInputBindingDescription()
                {
                    binding = (uint)binding,
                    inputRate = (inputDesc.Elements[0].InstanceStepRate != 0) ? VkVertexInputRate.Instance : VkVertexInputRate.Vertex,
                    stride = inputDesc.Stride
                };

                uint currentOffset = 0;
                for (int location = 0; location < inputDesc.Elements.Length; location++)
                {
                    VertexElementDescription inputElement = inputDesc.Elements[location];

                    attributeDescs[targetIndex] = new VkVertexInputAttributeDescription()
                    {
                        format = VkFormats.VdToVkVertexElementFormat(inputElement.Format),
                        binding = (uint)binding,
                        location = (uint)(targetLocation + location),
                        offset = currentOffset
                    };

                    targetIndex += 1;
                    currentOffset += FormatHelpers.GetSizeInBytes(inputElement.Format);
                }

                targetLocation += inputDesc.Elements.Length;
            }

            vertexInputCI.vertexBindingDescriptionCount = bindingCount;
            vertexInputCI.pVertexBindingDescriptions = bindingDescs;
            vertexInputCI.vertexAttributeDescriptionCount = attributeCount;
            vertexInputCI.pVertexAttributeDescriptions = attributeDescs;

            pipelineCI.pVertexInputState = &vertexInputCI;

            // Shader Stage
            ShaderStageDescription[] stageDescs = description.ShaderSet.ShaderStages;
            StackList<VkPipelineShaderStageCreateInfo> stages = new StackList<VkPipelineShaderStageCreateInfo>();
            foreach (ShaderStageDescription stageDesc in stageDescs)
            {
                VkShader vkShader = Util.AssertSubtype<Shader, VkShader>(stageDesc.Shader);
                VkPipelineShaderStageCreateInfo stageCI = VkPipelineShaderStageCreateInfo.New();
                stageCI.module = vkShader.ShaderModule;
                stageCI.stage = VkFormats.VdToVkShaderStages(stageDesc.Stage);
                stageCI.pName = CommonStrings.main; // Meh
                stages.Add(stageCI);
            }

            pipelineCI.stageCount = stages.Count;
            pipelineCI.pStages = (VkPipelineShaderStageCreateInfo*)stages.Data;

            // ViewportState
            VkPipelineViewportStateCreateInfo viewportStateCI = VkPipelineViewportStateCreateInfo.New();
            viewportStateCI.viewportCount = 1;
            viewportStateCI.scissorCount = 1;

            pipelineCI.pViewportState = &viewportStateCI;

            // Pipeline Layout
            ResourceLayout[] resourceLayouts = description.ResourceLayouts;
            VkPipelineLayoutCreateInfo pipelineLayoutCI = VkPipelineLayoutCreateInfo.New();
            pipelineLayoutCI.setLayoutCount = (uint)resourceLayouts.Length;
            VkDescriptorSetLayout* dsls = stackalloc VkDescriptorSetLayout[resourceLayouts.Length];
            for (int i = 0; i < resourceLayouts.Length; i++)
            {
                dsls[i] = Util.AssertSubtype<ResourceLayout, VkResourceLayout>(resourceLayouts[i]).DescriptorSetLayout;
            }
            pipelineLayoutCI.pSetLayouts = dsls;

            vkCreatePipelineLayout(_gd.Device, ref pipelineLayoutCI, null, out _pipelineLayout);
            pipelineCI.layout = _pipelineLayout;

            // Create fake RenderPass for compatibility.
            VkRenderPassCreateInfo renderPassCI = VkRenderPassCreateInfo.New();
            OutputDescription outputDesc = description.Outputs;
            if (outputDesc.ColorAttachments.Length > 1)
            {
                throw new NotImplementedException("Laziness");
            }

            VkAttachmentDescription colorAttachmentDesc = new VkAttachmentDescription();
            colorAttachmentDesc.format = outputDesc.ColorAttachments.Length > 0
                ? VkFormats.VdToVkPixelFormat(outputDesc.ColorAttachments[0].Format) : 0;
            colorAttachmentDesc.samples = VkSampleCountFlags.Count1;
            colorAttachmentDesc.loadOp = VkAttachmentLoadOp.Clear;
            colorAttachmentDesc.storeOp = VkAttachmentStoreOp.Store;
            colorAttachmentDesc.stencilLoadOp = VkAttachmentLoadOp.DontCare;
            colorAttachmentDesc.stencilStoreOp = VkAttachmentStoreOp.DontCare;
            colorAttachmentDesc.initialLayout = VkImageLayout.Undefined;
            colorAttachmentDesc.finalLayout = VkImageLayout.PresentSrcKHR;

            VkAttachmentReference colorAttachmentRef = new VkAttachmentReference();
            colorAttachmentRef.attachment = 0;
            colorAttachmentRef.layout = VkImageLayout.ColorAttachmentOptimal;

            VkAttachmentDescription depthAttachmentDesc = new VkAttachmentDescription();
            VkAttachmentReference depthAttachmentRef = new VkAttachmentReference();
            if (outputDesc.DepthAttachment != null)
            {
                depthAttachmentDesc.format = VkFormats.VdToVkPixelFormat(outputDesc.DepthAttachment.Value.Format, toDepthFormat: true);
                depthAttachmentDesc.samples = VkSampleCountFlags.Count1;
                depthAttachmentDesc.loadOp = VkAttachmentLoadOp.Clear;
                depthAttachmentDesc.storeOp = VkAttachmentStoreOp.Store;
                depthAttachmentDesc.stencilLoadOp = VkAttachmentLoadOp.DontCare;
                depthAttachmentDesc.stencilStoreOp = VkAttachmentStoreOp.DontCare;
                depthAttachmentDesc.initialLayout = VkImageLayout.Undefined;
                depthAttachmentDesc.finalLayout = VkImageLayout.DepthStencilAttachmentOptimal;

                depthAttachmentRef.attachment = outputDesc.ColorAttachments.Length == 0 ? 0u : 1u;
                depthAttachmentRef.layout = VkImageLayout.DepthStencilAttachmentOptimal;
            }

            VkSubpassDescription subpass = new VkSubpassDescription();
            StackList<VkAttachmentDescription, Size512Bytes> attachments = new StackList<VkAttachmentDescription, Size512Bytes>();
            subpass.pipelineBindPoint = VkPipelineBindPoint.Graphics;
            if (outputDesc.ColorAttachments.Length > 0)
            {
                subpass.colorAttachmentCount = 1;
                subpass.pColorAttachments = &colorAttachmentRef;
                attachments.Add(colorAttachmentDesc);
            }

            if (outputDesc.DepthAttachment != null)
            {
                subpass.pDepthStencilAttachment = &depthAttachmentRef;
                attachments.Add(depthAttachmentDesc);
            }

            VkSubpassDependency subpassDependency = new VkSubpassDependency();
            subpassDependency.srcSubpass = SubpassExternal;
            subpassDependency.srcStageMask = VkPipelineStageFlags.ColorAttachmentOutput;
            subpassDependency.dstStageMask = VkPipelineStageFlags.ColorAttachmentOutput;
            subpassDependency.dstAccessMask = VkAccessFlags.ColorAttachmentRead | VkAccessFlags.ColorAttachmentWrite;
            if (outputDesc.DepthAttachment != null)
            {
                subpassDependency.dstAccessMask |= VkAccessFlags.DepthStencilAttachmentRead | VkAccessFlags.DepthStencilAttachmentWrite;
            }

            renderPassCI.attachmentCount = attachments.Count;
            renderPassCI.pAttachments = (VkAttachmentDescription*)attachments.Data;
            renderPassCI.subpassCount = 1;
            renderPassCI.pSubpasses = &subpass;
            renderPassCI.dependencyCount = 1;
            renderPassCI.pDependencies = &subpassDependency;

            VkResult creationResult = vkCreateRenderPass(_gd.Device, ref renderPassCI, null, out _renderPass);
            CheckResult(creationResult);

            pipelineCI.renderPass = _renderPass;

            VkResult result = vkCreateGraphicsPipelines(_gd.Device, VkPipelineCache.Null, 1, ref pipelineCI, null, out _devicePipeline);
            CheckResult(result);

            ResourceSetCount = (uint)description.ResourceLayouts.Length;
        }

        public override void Dispose()
        {
            Debug.Assert(!_disposed);
            _disposed = true;
            vkDestroyPipelineLayout(_gd.Device, _pipelineLayout, null);
            vkDestroyPipeline(_gd.Device, _devicePipeline, null);
            vkDestroyRenderPass(_gd.Device, _renderPass, null);
        }
    }
}
