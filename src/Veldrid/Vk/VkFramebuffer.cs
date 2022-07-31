using System.Collections.Generic;
using System.Diagnostics;
using TerraFX.Interop.Vulkan;
using static TerraFX.Interop.Vulkan.VkAttachmentLoadOp;
using static TerraFX.Interop.Vulkan.VkAttachmentStoreOp;
using static TerraFX.Interop.Vulkan.VkImageLayout;
using static TerraFX.Interop.Vulkan.Vulkan;
using static Veldrid.Vulkan.VulkanUtil;
using VulkanFramebuffer = TerraFX.Interop.Vulkan.VkFramebuffer;

namespace Veldrid.Vulkan
{
    internal sealed unsafe class VkFramebuffer : VkFramebufferBase
    {
        private readonly VkGraphicsDevice _gd;
        private readonly VulkanFramebuffer _deviceFramebuffer;
        private readonly VkRenderPass _renderPassNoClearLoad;
        private readonly VkRenderPass _renderPassNoClear;
        private readonly VkRenderPass _renderPassClear;
        private readonly List<VkImageView> _attachmentViews = new();
        private bool _destroyed;
        private string? _name;

        public override VulkanFramebuffer CurrentFramebuffer => _deviceFramebuffer;
        public override VkRenderPass RenderPassNoClear_Init => _renderPassNoClear;
        public override VkRenderPass RenderPassNoClear_Load => _renderPassNoClearLoad;
        public override VkRenderPass RenderPassClear => _renderPassClear;

        public override uint RenderableWidth => Width;
        public override uint RenderableHeight => Height;

        public override uint AttachmentCount { get; }

        public override bool IsDisposed => _destroyed;

        public VkFramebuffer(VkGraphicsDevice gd, in FramebufferDescription description, bool isPresented)
            : base(description.DepthTarget, description.ColorTargets)
        {
            _gd = gd;

            StackList<VkAttachmentDescription> attachments = new();

            uint colorAttachmentCount = (uint)ColorTargets.Length;
            StackList<VkAttachmentReference> colorAttachmentRefs = new();
            for (int i = 0; i < colorAttachmentCount; i++)
            {
                VkTexture vkColorTex = Util.AssertSubtype<Texture, VkTexture>(ColorTargets[i].Target);
                VkAttachmentDescription colorAttachmentDesc = new()
                {
                    format = vkColorTex.VkFormat,
                    samples = vkColorTex.VkSampleCount,
                    loadOp = VK_ATTACHMENT_LOAD_OP_LOAD,
                    storeOp = VK_ATTACHMENT_STORE_OP_STORE,
                    stencilLoadOp = VK_ATTACHMENT_LOAD_OP_DONT_CARE,
                    stencilStoreOp = VK_ATTACHMENT_STORE_OP_DONT_CARE,
                    initialLayout = isPresented
                        ? VK_IMAGE_LAYOUT_PRESENT_SRC_KHR
                        : ((vkColorTex.Usage & TextureUsage.Sampled) != 0)
                            ? VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL
                            : VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL,
                    finalLayout = VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL
                };
                attachments.Add(colorAttachmentDesc);

                VkAttachmentReference colorAttachmentRef = new()
                {
                    attachment = (uint)i,
                    layout = VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL
                };
                colorAttachmentRefs.Add(colorAttachmentRef);
            }

            VkAttachmentDescription depthAttachmentDesc = new();
            VkAttachmentReference depthAttachmentRef = new();
            if (DepthTarget != null)
            {
                VkTexture vkDepthTex = Util.AssertSubtype<Texture, VkTexture>(DepthTarget.Value.Target);
                bool hasStencil = FormatHelpers.IsStencilFormat(vkDepthTex.Format);
                depthAttachmentDesc.format = vkDepthTex.VkFormat;
                depthAttachmentDesc.samples = vkDepthTex.VkSampleCount;
                depthAttachmentDesc.loadOp = VK_ATTACHMENT_LOAD_OP_LOAD;
                depthAttachmentDesc.storeOp = VK_ATTACHMENT_STORE_OP_STORE;
                depthAttachmentDesc.stencilLoadOp = VK_ATTACHMENT_LOAD_OP_DONT_CARE;
                depthAttachmentDesc.stencilStoreOp = hasStencil
                    ? VK_ATTACHMENT_STORE_OP_STORE
                    : VK_ATTACHMENT_STORE_OP_DONT_CARE;
                depthAttachmentDesc.initialLayout = ((vkDepthTex.Usage & TextureUsage.Sampled) != 0)
                    ? VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL
                    : VK_IMAGE_LAYOUT_DEPTH_STENCIL_ATTACHMENT_OPTIMAL;
                depthAttachmentDesc.finalLayout = VK_IMAGE_LAYOUT_DEPTH_STENCIL_ATTACHMENT_OPTIMAL;

                depthAttachmentRef.attachment = (uint)description.ColorTargets.Length;
                depthAttachmentRef.layout = VK_IMAGE_LAYOUT_DEPTH_STENCIL_ATTACHMENT_OPTIMAL;
            }

            VkSubpassDescription subpass = new() { pipelineBindPoint = VkPipelineBindPoint.VK_PIPELINE_BIND_POINT_GRAPHICS };
            if (ColorTargets.Length > 0)
            {
                subpass.colorAttachmentCount = colorAttachmentCount;
                subpass.pColorAttachments = (VkAttachmentReference*)colorAttachmentRefs.Data;
            }

            if (DepthTarget != null)
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

            VkRenderPass renderPassNoClear;
            VkResult creationResult = vkCreateRenderPass(_gd.Device, &renderPassCI, null, &renderPassNoClear);
            CheckResult(creationResult);
            _renderPassNoClear = renderPassNoClear;

            for (int i = 0; i < colorAttachmentCount; i++)
            {
                attachments[i].loadOp = VK_ATTACHMENT_LOAD_OP_LOAD;
                attachments[i].initialLayout = VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL;
            }
            if (DepthTarget != null)
            {
                attachments[attachments.Count - 1].loadOp = VK_ATTACHMENT_LOAD_OP_LOAD;
                attachments[attachments.Count - 1].initialLayout = VK_IMAGE_LAYOUT_DEPTH_STENCIL_ATTACHMENT_OPTIMAL;
                bool hasStencil = FormatHelpers.IsStencilFormat(DepthTarget.Value.Target.Format);
                if (hasStencil)
                {
                    attachments[attachments.Count - 1].stencilLoadOp = VK_ATTACHMENT_LOAD_OP_LOAD;
                }

            }

            VkRenderPass renderPassNoClearLoad;
            creationResult = vkCreateRenderPass(_gd.Device, &renderPassCI, null, &renderPassNoClearLoad);
            CheckResult(creationResult);
            _renderPassNoClearLoad = renderPassNoClearLoad;

            // Load version

            if (DepthTarget != null)
            {
                attachments[attachments.Count - 1].loadOp = VK_ATTACHMENT_LOAD_OP_CLEAR;
                attachments[attachments.Count - 1].initialLayout = VK_IMAGE_LAYOUT_UNDEFINED;
                bool hasStencil = FormatHelpers.IsStencilFormat(DepthTarget.Value.Target.Format);
                if (hasStencil)
                {
                    attachments[attachments.Count - 1].stencilLoadOp = VK_ATTACHMENT_LOAD_OP_CLEAR;
                }
            }

            for (int i = 0; i < colorAttachmentCount; i++)
            {
                attachments[i].loadOp = VK_ATTACHMENT_LOAD_OP_CLEAR;
                attachments[i].initialLayout = VK_IMAGE_LAYOUT_UNDEFINED;
            }

            VkRenderPass renderPassClear;
            creationResult = vkCreateRenderPass(_gd.Device, &renderPassCI, null, &renderPassClear);
            CheckResult(creationResult);
            _renderPassClear = renderPassClear;

            uint fbAttachmentsCount = (uint)description.ColorTargets.Length;
            if (description.DepthTarget != null)
            {
                fbAttachmentsCount += 1;
            }

            VkImageView* fbAttachments = stackalloc VkImageView[(int)fbAttachmentsCount];
            for (int i = 0; i < colorAttachmentCount; i++)
            {
                VkTexture vkColorTarget = Util.AssertSubtype<Texture, VkTexture>(description.ColorTargets[i].Target);
                VkImageViewCreateInfo imageViewCI = new()
                {
                    sType = VkStructureType.VK_STRUCTURE_TYPE_IMAGE_VIEW_CREATE_INFO,
                    image = vkColorTarget.OptimalDeviceImage,
                    format = vkColorTarget.VkFormat,
                    viewType = VkImageViewType.VK_IMAGE_VIEW_TYPE_2D,
                    subresourceRange = new VkImageSubresourceRange()
                    {
                        aspectMask = VkImageAspectFlags.VK_IMAGE_ASPECT_COLOR_BIT,
                        baseMipLevel = description.ColorTargets[i].MipLevel,
                        levelCount = 1,
                        baseArrayLayer = description.ColorTargets[i].ArrayLayer,
                        layerCount = 1
                    }
                };
                VkImageView* dest = (fbAttachments + i);
                VkResult result = vkCreateImageView(_gd.Device, &imageViewCI, null, dest);
                CheckResult(result);
                _attachmentViews.Add(*dest);
            }

            // Depth
            if (description.DepthTarget != null)
            {
                VkTexture vkDepthTarget = Util.AssertSubtype<Texture, VkTexture>(description.DepthTarget.Value.Target);
                bool hasStencil = FormatHelpers.IsStencilFormat(vkDepthTarget.Format);
                VkImageViewCreateInfo depthViewCI = new()
                {
                    sType = VkStructureType.VK_STRUCTURE_TYPE_IMAGE_VIEW_CREATE_INFO,
                    image = vkDepthTarget.OptimalDeviceImage,
                    format = vkDepthTarget.VkFormat,
                    viewType = description.DepthTarget.Value.Target.ArrayLayers == 1
                        ? VkImageViewType.VK_IMAGE_VIEW_TYPE_2D
                        : VkImageViewType.VK_IMAGE_VIEW_TYPE_2D_ARRAY,
                    subresourceRange = new VkImageSubresourceRange()
                    {
                        aspectMask = hasStencil
                            ? VkImageAspectFlags.VK_IMAGE_ASPECT_DEPTH_BIT | VkImageAspectFlags.VK_IMAGE_ASPECT_STENCIL_BIT
                            : VkImageAspectFlags.VK_IMAGE_ASPECT_DEPTH_BIT,
                        baseMipLevel = description.DepthTarget.Value.MipLevel,
                        levelCount = 1,
                        baseArrayLayer = description.DepthTarget.Value.ArrayLayer,
                        layerCount = 1
                    }
                };

                VkImageView* dest = (fbAttachments + (fbAttachmentsCount - 1));
                VkResult result = vkCreateImageView(_gd.Device, &depthViewCI, null, dest);
                CheckResult(result);
                _attachmentViews.Add(*dest);
            }

            Texture dimTex;
            uint mipLevel;
            if (ColorTargets.Length > 0)
            {
                dimTex = ColorTargets[0].Target;
                mipLevel = ColorTargets[0].MipLevel;
            }
            else
            {
                Debug.Assert(DepthTarget != null);
                dimTex = DepthTarget.Value.Target;
                mipLevel = DepthTarget.Value.MipLevel;
            }

            Util.GetMipDimensions(
                dimTex,
                mipLevel,
                out uint mipWidth,
                out uint mipHeight);

            VkFramebufferCreateInfo fbCI = new()
            {
                sType = VkStructureType.VK_STRUCTURE_TYPE_FRAMEBUFFER_CREATE_INFO,
                width = mipWidth,
                height = mipHeight,
                attachmentCount = fbAttachmentsCount,
                pAttachments = fbAttachments,
                layers = 1,
                renderPass = _renderPassNoClear
            };

            VulkanFramebuffer deviceFramebuffer;
            creationResult = vkCreateFramebuffer(_gd.Device, &fbCI, null, &deviceFramebuffer);
            CheckResult(creationResult);
            _deviceFramebuffer = deviceFramebuffer;

            if (DepthTarget != null)
            {
                AttachmentCount += 1;
            }
            AttachmentCount += (uint)ColorTargets.Length;
        }

        public override void TransitionToIntermediateLayout(VkCommandBuffer cb)
        {
            foreach (ref readonly FramebufferAttachment ca in ColorTargets)
            {
                VkTexture vkTex = Util.AssertSubtype<Texture, VkTexture>(ca.Target);
                vkTex.SetImageLayout(ca.MipLevel, ca.ArrayLayer, VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL);
            }
            if (DepthTarget != null)
            {
                VkTexture vkTex = Util.AssertSubtype<Texture, VkTexture>(DepthTarget.Value.Target);
                vkTex.SetImageLayout(
                    DepthTarget.Value.MipLevel,
                    DepthTarget.Value.ArrayLayer,
                    VK_IMAGE_LAYOUT_DEPTH_STENCIL_ATTACHMENT_OPTIMAL);
            }
        }

        public override void TransitionToFinalLayout(VkCommandBuffer cb)
        {
            foreach (ref readonly FramebufferAttachment ca in ColorTargets)
            {
                VkTexture vkTex = Util.AssertSubtype<Texture, VkTexture>(ca.Target);
                if ((vkTex.Usage & TextureUsage.Sampled) != 0)
                {
                    vkTex.TransitionImageLayout(
                        cb,
                        ca.MipLevel, 1,
                        ca.ArrayLayer, 1,
                        VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL);
                }
            }
            if (DepthTarget != null)
            {
                VkTexture vkTex = Util.AssertSubtype<Texture, VkTexture>(DepthTarget.Value.Target);
                if ((vkTex.Usage & TextureUsage.Sampled) != 0)
                {
                    vkTex.TransitionImageLayout(
                        cb,
                        DepthTarget.Value.MipLevel, 1,
                        DepthTarget.Value.ArrayLayer, 1,
                        VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL);
                }
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

        protected override void DisposeCore()
        {
            if (!_destroyed)
            {
                vkDestroyFramebuffer(_gd.Device, _deviceFramebuffer, null);
                vkDestroyRenderPass(_gd.Device, _renderPassNoClear, null);
                vkDestroyRenderPass(_gd.Device, _renderPassNoClearLoad, null);
                vkDestroyRenderPass(_gd.Device, _renderPassClear, null);

                foreach (VkImageView view in _attachmentViews)
                {
                    vkDestroyImageView(_gd.Device, view, null);
                }

                _destroyed = true;
            }
        }
    }
}
