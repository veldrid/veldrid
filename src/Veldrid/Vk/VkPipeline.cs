using System;
using System.Runtime.CompilerServices;
using TerraFX.Interop.Vulkan;
using static TerraFX.Interop.Vulkan.Vulkan;
using static Veldrid.Vulkan.VulkanUtil;
using VulkanPipeline = TerraFX.Interop.Vulkan.VkPipeline;

namespace Veldrid.Vulkan
{
    internal sealed unsafe class VkPipeline : Pipeline, IResourceRefCountTarget
    {
        private readonly VkGraphicsDevice _gd;
        private readonly VulkanPipeline _devicePipeline;
        private readonly VkPipelineLayout _pipelineLayout;
        private readonly VkRenderPass _renderPass;
        private bool _destroyed;
        private string? _name;

        public VulkanPipeline DevicePipeline => _devicePipeline;

        public VkPipelineLayout PipelineLayout => _pipelineLayout;

        public uint ResourceSetCount { get; }
        public int DynamicOffsetsCount { get; }
        public uint VertexLayoutCount { get; }
        public bool ScissorTestEnabled { get; }

        public override bool IsComputePipeline { get; }

        public ResourceRefCount RefCount { get; }

        public override bool IsDisposed => _destroyed;

        public VkPipeline(VkGraphicsDevice gd, in GraphicsPipelineDescription description)
            : base(description)
        {
            _gd = gd;
            IsComputePipeline = false;
            RefCount = new ResourceRefCount(this);

            VkGraphicsPipelineCreateInfo pipelineCI = new();
            pipelineCI.sType = VkStructureType.VK_STRUCTURE_TYPE_GRAPHICS_PIPELINE_CREATE_INFO;

            // Blend State
            int attachmentsCount = description.BlendState.AttachmentStates.Length;
            VkPipelineColorBlendAttachmentState* attachmentsPtr = stackalloc VkPipelineColorBlendAttachmentState[attachmentsCount];
            for (int i = 0; i < attachmentsCount; i++)
            {
                BlendAttachmentDescription vdDesc = description.BlendState.AttachmentStates[i];
                VkPipelineColorBlendAttachmentState attachmentState = new()
                {
                    srcColorBlendFactor = VkFormats.VdToVkBlendFactor(vdDesc.SourceColorFactor),
                    dstColorBlendFactor = VkFormats.VdToVkBlendFactor(vdDesc.DestinationColorFactor),
                    colorBlendOp = VkFormats.VdToVkBlendOp(vdDesc.ColorFunction),
                    srcAlphaBlendFactor = VkFormats.VdToVkBlendFactor(vdDesc.SourceAlphaFactor),
                    dstAlphaBlendFactor = VkFormats.VdToVkBlendFactor(vdDesc.DestinationAlphaFactor),
                    alphaBlendOp = VkFormats.VdToVkBlendOp(vdDesc.AlphaFunction),
                    blendEnable = vdDesc.BlendEnabled,
                    colorWriteMask = VkFormats.VdToVkColorWriteMask(vdDesc.ColorWriteMask.GetOrDefault()),
                };
                attachmentsPtr[i] = attachmentState;
            }

            VkPipelineColorBlendStateCreateInfo blendStateCI = new()
            {
                sType = VkStructureType.VK_STRUCTURE_TYPE_PIPELINE_COLOR_BLEND_STATE_CREATE_INFO,
                attachmentCount = (uint)attachmentsCount,
                pAttachments = attachmentsPtr
            };

            RgbaFloat blendFactor = description.BlendState.BlendFactor;
            blendStateCI.blendConstants[0] = blendFactor.R;
            blendStateCI.blendConstants[1] = blendFactor.G;
            blendStateCI.blendConstants[2] = blendFactor.B;
            blendStateCI.blendConstants[3] = blendFactor.A;

            pipelineCI.pColorBlendState = &blendStateCI;

            // Rasterizer State
            RasterizerStateDescription rsDesc = description.RasterizerState;
            VkPipelineRasterizationStateCreateInfo rsCI = new()
            {
                sType = VkStructureType.VK_STRUCTURE_TYPE_PIPELINE_RASTERIZATION_STATE_CREATE_INFO,
                cullMode = VkFormats.VdToVkCullMode(rsDesc.CullMode),
                polygonMode = VkFormats.VdToVkPolygonMode(rsDesc.FillMode),
                depthClampEnable = !rsDesc.DepthClipEnabled,
                frontFace = rsDesc.FrontFace == FrontFace.Clockwise
                    ? VkFrontFace.VK_FRONT_FACE_CLOCKWISE
                    : VkFrontFace.VK_FRONT_FACE_COUNTER_CLOCKWISE,
                lineWidth = 1f
            };

            pipelineCI.pRasterizationState = &rsCI;

            ScissorTestEnabled = rsDesc.ScissorTestEnabled;

            // Dynamic State
            VkDynamicState* dynamicStates = stackalloc VkDynamicState[2];
            dynamicStates[0] = VkDynamicState.VK_DYNAMIC_STATE_VIEWPORT;
            dynamicStates[1] = VkDynamicState.VK_DYNAMIC_STATE_SCISSOR;
            VkPipelineDynamicStateCreateInfo dynamicStateCI = new()
            {
                sType = VkStructureType.VK_STRUCTURE_TYPE_PIPELINE_DYNAMIC_STATE_CREATE_INFO,
                dynamicStateCount = 2,
                pDynamicStates = dynamicStates
            };

            pipelineCI.pDynamicState = &dynamicStateCI;

            // Depth Stencil State
            DepthStencilStateDescription vdDssDesc = description.DepthStencilState;
            VkPipelineDepthStencilStateCreateInfo dssCI = new()
            {
                sType = VkStructureType.VK_STRUCTURE_TYPE_PIPELINE_DEPTH_STENCIL_STATE_CREATE_INFO,
                depthWriteEnable = vdDssDesc.DepthWriteEnabled,
                depthTestEnable = vdDssDesc.DepthTestEnabled,
                depthCompareOp = VkFormats.VdToVkCompareOp(vdDssDesc.DepthComparison),
                stencilTestEnable = vdDssDesc.StencilTestEnabled,
                front = new VkStencilOpState()
                {
                    failOp = VkFormats.VdToVkStencilOp(vdDssDesc.StencilFront.Fail),
                    passOp = VkFormats.VdToVkStencilOp(vdDssDesc.StencilFront.Pass),
                    depthFailOp = VkFormats.VdToVkStencilOp(vdDssDesc.StencilFront.DepthFail),
                    compareOp = VkFormats.VdToVkCompareOp(vdDssDesc.StencilFront.Comparison),
                    compareMask = vdDssDesc.StencilReadMask,
                    writeMask = vdDssDesc.StencilWriteMask,
                    reference = vdDssDesc.StencilReference
                },
                back = new VkStencilOpState()
                {
                    failOp = VkFormats.VdToVkStencilOp(vdDssDesc.StencilBack.Fail),
                    passOp = VkFormats.VdToVkStencilOp(vdDssDesc.StencilBack.Pass),
                    depthFailOp = VkFormats.VdToVkStencilOp(vdDssDesc.StencilBack.DepthFail),
                    compareOp = VkFormats.VdToVkCompareOp(vdDssDesc.StencilBack.Comparison),
                    compareMask = vdDssDesc.StencilReadMask,
                    writeMask = vdDssDesc.StencilWriteMask,
                    reference = vdDssDesc.StencilReference
                }
            };

            pipelineCI.pDepthStencilState = &dssCI;

            // Multisample
            VkSampleCountFlags vkSampleCount = VkFormats.VdToVkSampleCount(description.Outputs.SampleCount);
            VkPipelineMultisampleStateCreateInfo multisampleCI = new()
            {
                sType = VkStructureType.VK_STRUCTURE_TYPE_PIPELINE_MULTISAMPLE_STATE_CREATE_INFO,
                rasterizationSamples = vkSampleCount,
                alphaToCoverageEnable = description.BlendState.AlphaToCoverageEnabled
            };

            pipelineCI.pMultisampleState = &multisampleCI;

            // Input Assembly
            VkPipelineInputAssemblyStateCreateInfo inputAssemblyCI = new()
            {
                sType = VkStructureType.VK_STRUCTURE_TYPE_PIPELINE_INPUT_ASSEMBLY_STATE_CREATE_INFO,
                topology = VkFormats.VdToVkPrimitiveTopology(description.PrimitiveTopology)
            };

            pipelineCI.pInputAssemblyState = &inputAssemblyCI;

            // Vertex Input State

            ReadOnlySpan<VertexLayoutDescription> inputDescriptions = description.ShaderSet.VertexLayouts;
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
                bindingDescs[binding] = new VkVertexInputBindingDescription()
                {
                    binding = (uint)binding,
                    inputRate = (inputDesc.InstanceStepRate != 0)
                                    ? VkVertexInputRate.VK_VERTEX_INPUT_RATE_INSTANCE
                                    : VkVertexInputRate.VK_VERTEX_INPUT_RATE_VERTEX,
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
                        offset = inputElement.Offset != 0 ? inputElement.Offset : currentOffset
                    };

                    targetIndex += 1;
                    currentOffset += FormatSizeHelpers.GetSizeInBytes(inputElement.Format);
                }

                targetLocation += inputDesc.Elements.Length;
            }

            VkPipelineVertexInputStateCreateInfo vertexInputCI = new()
            {
                sType = VkStructureType.VK_STRUCTURE_TYPE_PIPELINE_VERTEX_INPUT_STATE_CREATE_INFO,
                vertexBindingDescriptionCount = bindingCount,
                pVertexBindingDescriptions = bindingDescs,
                vertexAttributeDescriptionCount = attributeCount,
                pVertexAttributeDescriptions = attributeDescs
            };

            pipelineCI.pVertexInputState = &vertexInputCI;

            // Shader Stage

            VkSpecializationInfo specializationInfo;
            SpecializationConstant[]? specDescs = description.ShaderSet.Specializations;
            if (specDescs != null)
            {
                uint specDataSize = 0;
                foreach (SpecializationConstant spec in specDescs)
                {
                    specDataSize += VkFormats.GetSpecializationConstantSize(spec.Type);
                }
                byte* fullSpecData = stackalloc byte[(int)specDataSize];
                int specializationCount = specDescs.Length;
                VkSpecializationMapEntry* mapEntries = stackalloc VkSpecializationMapEntry[specializationCount];
                uint specOffset = 0;
                for (int i = 0; i < specializationCount; i++)
                {
                    ulong data = specDescs[i].Data;
                    byte* srcData = (byte*)&data;
                    uint dataSize = VkFormats.GetSpecializationConstantSize(specDescs[i].Type);
                    Unsafe.CopyBlock(fullSpecData + specOffset, srcData, dataSize);
                    mapEntries[i].constantID = specDescs[i].ID;
                    mapEntries[i].offset = specOffset;
                    mapEntries[i].size = (UIntPtr)dataSize;
                    specOffset += dataSize;
                }
                specializationInfo.dataSize = (UIntPtr)specDataSize;
                specializationInfo.pData = fullSpecData;
                specializationInfo.mapEntryCount = (uint)specializationCount;
                specializationInfo.pMapEntries = mapEntries;
            }

            Shader[] shaders = description.ShaderSet.Shaders;
            StackList<VkPipelineShaderStageCreateInfo> stages = new();
            foreach (Shader shader in shaders)
            {
                VkShader vkShader = Util.AssertSubtype<Shader, VkShader>(shader);
                VkPipelineShaderStageCreateInfo stageCI = new()
                {
                    sType = VkStructureType.VK_STRUCTURE_TYPE_PIPELINE_SHADER_STAGE_CREATE_INFO,
                    module = vkShader.ShaderModule,
                    stage = VkFormats.VdToVkShaderStages(shader.Stage),
                    pName = shader.EntryPoint == "main" ? CommonStrings.main : new FixedUtf8String(shader.EntryPoint),
                    pSpecializationInfo = &specializationInfo
                };
                stages.Add(stageCI);
            }

            pipelineCI.stageCount = stages.Count;
            pipelineCI.pStages = (VkPipelineShaderStageCreateInfo*)stages.Data;

            // ViewportState
            VkPipelineViewportStateCreateInfo viewportStateCI = new()
            {
                sType = VkStructureType.VK_STRUCTURE_TYPE_PIPELINE_VIEWPORT_STATE_CREATE_INFO,
                viewportCount = 1,
                scissorCount = 1
            };

            pipelineCI.pViewportState = &viewportStateCI;

            // Pipeline Layout
            ResourceLayout[] resourceLayouts = description.ResourceLayouts;
            VkDescriptorSetLayout* dsls = stackalloc VkDescriptorSetLayout[resourceLayouts.Length];
            for (int i = 0; i < resourceLayouts.Length; i++)
            {
                dsls[i] = Util.AssertSubtype<ResourceLayout, VkResourceLayout>(resourceLayouts[i]).DescriptorSetLayout;
            }
            VkPipelineLayoutCreateInfo pipelineLayoutCI = new()
            {
                sType = VkStructureType.VK_STRUCTURE_TYPE_PIPELINE_LAYOUT_CREATE_INFO,
                setLayoutCount = (uint)resourceLayouts.Length,
                pSetLayouts = dsls
            };

            VkPipelineLayout pipelineLayout;
            vkCreatePipelineLayout(_gd.Device, &pipelineLayoutCI, null, &pipelineLayout);
            _pipelineLayout = pipelineLayout;

            pipelineCI.layout = _pipelineLayout;

            // Create fake RenderPass for compatibility.

            OutputDescription outputDesc = description.Outputs;
            StackList<VkAttachmentDescription, Size512Bytes> attachments = new();

            // TODO: A huge portion of this next part is duplicated in VkFramebuffer.cs.

            StackList<VkAttachmentDescription> colorAttachmentDescs = new();
            StackList<VkAttachmentReference> colorAttachmentRefs = new();

            ReadOnlySpan<OutputAttachmentDescription> outputColorAttachmentDescs = outputDesc.ColorAttachments;
            for (int i = 0; i < outputColorAttachmentDescs.Length; i++)
            {
                ref VkAttachmentDescription desc = ref colorAttachmentDescs[i];
                desc.format = VkFormats.VdToVkPixelFormat(outputColorAttachmentDescs[i].Format);
                desc.samples = vkSampleCount;
                desc.loadOp = VkAttachmentLoadOp.VK_ATTACHMENT_LOAD_OP_DONT_CARE;
                desc.storeOp = VkAttachmentStoreOp.VK_ATTACHMENT_STORE_OP_STORE;
                desc.stencilLoadOp = VkAttachmentLoadOp.VK_ATTACHMENT_LOAD_OP_DONT_CARE;
                desc.stencilStoreOp = VkAttachmentStoreOp.VK_ATTACHMENT_STORE_OP_DONT_CARE;
                desc.initialLayout = VkImageLayout.VK_IMAGE_LAYOUT_UNDEFINED;
                desc.finalLayout = VkImageLayout.VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL;
                attachments.Add(desc);

                colorAttachmentRefs[i].attachment = (uint)i;
                colorAttachmentRefs[i].layout = VkImageLayout.VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL;
            }

            VkAttachmentDescription depthAttachmentDesc = new();
            VkAttachmentReference depthAttachmentRef = new();
            if (outputDesc.DepthAttachment != null)
            {
                PixelFormat depthFormat = outputDesc.DepthAttachment.GetValueOrDefault().Format;
                bool hasStencil = FormatHelpers.IsStencilFormat(depthFormat);
                depthAttachmentDesc.format = VkFormats.VdToVkPixelFormat(depthFormat, toDepthFormat: true);
                depthAttachmentDesc.samples = vkSampleCount;
                depthAttachmentDesc.loadOp = VkAttachmentLoadOp.VK_ATTACHMENT_LOAD_OP_DONT_CARE;
                depthAttachmentDesc.storeOp = VkAttachmentStoreOp.VK_ATTACHMENT_STORE_OP_STORE;
                depthAttachmentDesc.stencilLoadOp = VkAttachmentLoadOp.VK_ATTACHMENT_LOAD_OP_DONT_CARE;
                depthAttachmentDesc.stencilStoreOp = hasStencil
                    ? VkAttachmentStoreOp.VK_ATTACHMENT_STORE_OP_STORE
                    : VkAttachmentStoreOp.VK_ATTACHMENT_STORE_OP_DONT_CARE;
                depthAttachmentDesc.initialLayout = VkImageLayout.VK_IMAGE_LAYOUT_UNDEFINED;
                depthAttachmentDesc.finalLayout = VkImageLayout.VK_IMAGE_LAYOUT_DEPTH_STENCIL_ATTACHMENT_OPTIMAL;

                depthAttachmentRef.attachment = (uint)outputColorAttachmentDescs.Length;
                depthAttachmentRef.layout = VkImageLayout.VK_IMAGE_LAYOUT_DEPTH_STENCIL_ATTACHMENT_OPTIMAL;
            }

            VkSubpassDescription subpass = new()
            {
                pipelineBindPoint = VkPipelineBindPoint.VK_PIPELINE_BIND_POINT_GRAPHICS,
                colorAttachmentCount = (uint)outputColorAttachmentDescs.Length,
                pColorAttachments = (VkAttachmentReference*)colorAttachmentRefs.Data
            };

            for (int i = 0; i < colorAttachmentDescs.Count; i++)
            {
                attachments.Add(colorAttachmentDescs[i]);
            }

            if (outputDesc.DepthAttachment != null)
            {
                subpass.pDepthStencilAttachment = &depthAttachmentRef;
                attachments.Add(depthAttachmentDesc);
            }

            VkSubpassDependency subpassDependency = new()
            {
                srcSubpass = VK_SUBPASS_EXTERNAL,
                srcStageMask = VkPipelineStageFlags.VK_PIPELINE_STAGE_COLOR_ATTACHMENT_OUTPUT_BIT,
                dstStageMask = VkPipelineStageFlags.VK_PIPELINE_STAGE_COLOR_ATTACHMENT_OUTPUT_BIT,
                dstAccessMask = VkAccessFlags.VK_ACCESS_COLOR_ATTACHMENT_READ_BIT | VkAccessFlags.VK_ACCESS_COLOR_ATTACHMENT_WRITE_BIT
            };

            VkRenderPassCreateInfo renderPassCI = new()
            {
                sType = VkStructureType.VK_STRUCTURE_TYPE_RENDER_PASS_CREATE_INFO,
                attachmentCount = attachments.Count,
                pAttachments = (VkAttachmentDescription*)attachments.Data,
                subpassCount = 1,
                pSubpasses = &subpass,
                dependencyCount = 1,
                pDependencies = &subpassDependency
            };

            VkRenderPass renderPass;
            VkResult creationResult = vkCreateRenderPass(_gd.Device, &renderPassCI, null, &renderPass);
            CheckResult(creationResult);
            _renderPass = renderPass;

            pipelineCI.renderPass = _renderPass;

            VulkanPipeline devicePipeline;
            VkResult result = vkCreateGraphicsPipelines(_gd.Device, default, 1, &pipelineCI, null, &devicePipeline);
            CheckResult(result);
            _devicePipeline = devicePipeline;

            ResourceSetCount = (uint)description.ResourceLayouts.Length;
            DynamicOffsetsCount = 0;
            foreach (VkResourceLayout layout in description.ResourceLayouts)
            {
                DynamicOffsetsCount += layout.DynamicBufferCount;
            }
            VertexLayoutCount = (uint)inputDescriptions.Length;
        }

        public VkPipeline(VkGraphicsDevice gd, in ComputePipelineDescription description)
            : base(description)
        {
            _gd = gd;
            IsComputePipeline = true;
            RefCount = new ResourceRefCount(this);

            // Pipeline Layout
            ResourceLayout[] resourceLayouts = description.ResourceLayouts;
            VkDescriptorSetLayout* dsls = stackalloc VkDescriptorSetLayout[resourceLayouts.Length];
            for (int i = 0; i < resourceLayouts.Length; i++)
            {
                dsls[i] = Util.AssertSubtype<ResourceLayout, VkResourceLayout>(resourceLayouts[i]).DescriptorSetLayout;
            }

            VkPipelineLayoutCreateInfo pipelineLayoutCI = new()
            {
                sType = VkStructureType.VK_STRUCTURE_TYPE_PIPELINE_LAYOUT_CREATE_INFO,
                setLayoutCount = (uint)resourceLayouts.Length,
                pSetLayouts = dsls
            };

            VkPipelineLayout pipelineLayout;
            vkCreatePipelineLayout(_gd.Device, &pipelineLayoutCI, null, &pipelineLayout);
            _pipelineLayout = pipelineLayout;

            // Shader Stage

            VkSpecializationInfo specializationInfo;
            SpecializationConstant[]? specDescs = description.Specializations;
            if (specDescs != null)
            {
                uint specDataSize = 0;
                foreach (SpecializationConstant spec in specDescs)
                {
                    specDataSize += VkFormats.GetSpecializationConstantSize(spec.Type);
                }
                byte* fullSpecData = stackalloc byte[(int)specDataSize];
                int specializationCount = specDescs.Length;
                VkSpecializationMapEntry* mapEntries = stackalloc VkSpecializationMapEntry[specializationCount];
                uint specOffset = 0;
                for (int i = 0; i < specializationCount; i++)
                {
                    ulong data = specDescs[i].Data;
                    byte* srcData = (byte*)&data;
                    uint dataSize = VkFormats.GetSpecializationConstantSize(specDescs[i].Type);
                    Unsafe.CopyBlock(fullSpecData + specOffset, srcData, dataSize);
                    mapEntries[i].constantID = specDescs[i].ID;
                    mapEntries[i].offset = specOffset;
                    mapEntries[i].size = (UIntPtr)dataSize;
                    specOffset += dataSize;
                }
                specializationInfo.dataSize = (UIntPtr)specDataSize;
                specializationInfo.pData = fullSpecData;
                specializationInfo.mapEntryCount = (uint)specializationCount;
                specializationInfo.pMapEntries = mapEntries;
            }

            Shader shader = description.ComputeShader;
            VkShader vkShader = Util.AssertSubtype<Shader, VkShader>(shader);
            VkPipelineShaderStageCreateInfo stageCI = new()
            {
                sType = VkStructureType.VK_STRUCTURE_TYPE_PIPELINE_SHADER_STAGE_CREATE_INFO,
                module = vkShader.ShaderModule,
                stage = VkFormats.VdToVkShaderStages(shader.Stage),
                pName = shader.EntryPoint == "main" ? CommonStrings.main : new FixedUtf8String(shader.EntryPoint),
                pSpecializationInfo = &specializationInfo
            };

            VkComputePipelineCreateInfo pipelineCI = new()
            {
                sType = VkStructureType.VK_STRUCTURE_TYPE_COMPUTE_PIPELINE_CREATE_INFO,
                stage = stageCI,
                layout = _pipelineLayout
            };

            VulkanPipeline devicePipeline;
            VkResult result = vkCreateComputePipelines(
                 _gd.Device,
                 default,
                 1,
                 &pipelineCI,
                 null,
                 &devicePipeline);
            CheckResult(result);
            _devicePipeline = devicePipeline;

            ResourceSetCount = (uint)description.ResourceLayouts.Length;
            DynamicOffsetsCount = 0;
            foreach (VkResourceLayout layout in description.ResourceLayouts)
            {
                DynamicOffsetsCount += layout.DynamicBufferCount;
            }
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
                vkDestroyPipelineLayout(_gd.Device, _pipelineLayout, null);
                vkDestroyPipeline(_gd.Device, _devicePipeline, null);
                if (!IsComputePipeline)
                {
                    vkDestroyRenderPass(_gd.Device, _renderPass, null);
                }
            }
        }
    }
}
