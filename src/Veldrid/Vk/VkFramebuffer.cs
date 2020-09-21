using System.Collections.Generic;
using Vulkan;
using static Vulkan.VulkanNative;
using static Veldrid.Vk.VulkanUtil;
using System;
using System.Diagnostics;

namespace Veldrid.Vk
{
    internal unsafe class VkFramebuffer : VkFramebufferBase
    {
        private readonly VkGraphicsDevice _gd;
        private readonly Vulkan.VkFramebuffer _deviceFramebuffer;
        private readonly VkRenderPass _renderPassNoClearLoad;
        private readonly VkRenderPass _renderPassNoClear;
        private readonly VkRenderPass _renderPassClear;
        private readonly List<VkImageView> _attachmentViews = new List<VkImageView>();
        private bool _destroyed;
        private string _name;

        private readonly Dictionary<RenderPassDescription, VkRenderPass> _renderPasses
            = new Dictionary<RenderPassDescription, VkRenderPass>();

        public override Vulkan.VkFramebuffer CurrentFramebuffer => _deviceFramebuffer;
        public override VkRenderPass RenderPassNoClear_Init => _renderPassNoClear;
        public override VkRenderPass RenderPassNoClear_Load => _renderPassNoClearLoad;
        public override VkRenderPass RenderPassClear => _renderPassClear;

        public override uint RenderableWidth => Width;
        public override uint RenderableHeight => Height;

        public override uint AttachmentCount { get; }
        public bool IsPresented { get; }

        public override bool IsDisposed => _destroyed;

        public VkFramebuffer(VkGraphicsDevice gd, ref FramebufferDescription description, bool isPresented)
            : base(description.DepthTarget, description.ColorTargets, description.ResolveTargetsOrEmpty())
        {
            _gd = gd;
            IsPresented = isPresented;

            VkRenderPassCreateInfo renderPassCI = VkRenderPassCreateInfo.New();

            StackList<VkAttachmentDescription> attachments = new StackList<VkAttachmentDescription>();

            uint colorAttachmentCount = (uint)ColorTargets.Count;
            StackList<VkAttachmentReference> colorAttachmentRefs = new StackList<VkAttachmentReference>();
            for (int i = 0; i < colorAttachmentCount; i++)
            {
                VkTexture vkColorTex = Util.AssertSubtype<Texture, VkTexture>(ColorTargets[i].Target);
                VkAttachmentDescription colorAttachmentDesc = new VkAttachmentDescription();
                colorAttachmentDesc.format = vkColorTex.VkFormat;
                colorAttachmentDesc.samples = vkColorTex.VkSampleCount;
                colorAttachmentDesc.loadOp = VkAttachmentLoadOp.Load;
                colorAttachmentDesc.storeOp = VkAttachmentStoreOp.Store;
                colorAttachmentDesc.stencilLoadOp = VkAttachmentLoadOp.DontCare;
                colorAttachmentDesc.stencilStoreOp = VkAttachmentStoreOp.DontCare;
                colorAttachmentDesc.initialLayout = isPresented
                    ? VkImageLayout.PresentSrcKHR
                    : ((vkColorTex.Usage & TextureUsage.Sampled) != 0)
                        ? VkImageLayout.ShaderReadOnlyOptimal
                        : VkImageLayout.ColorAttachmentOptimal;
                colorAttachmentDesc.finalLayout = VkImageLayout.ColorAttachmentOptimal;
                attachments.Add(colorAttachmentDesc);

                VkAttachmentReference colorAttachmentRef = new VkAttachmentReference();
                colorAttachmentRef.attachment = (uint)i;
                colorAttachmentRef.layout = VkImageLayout.ColorAttachmentOptimal;
                colorAttachmentRefs.Add(colorAttachmentRef);
            }

            VkAttachmentDescription depthAttachmentDesc = new VkAttachmentDescription();
            VkAttachmentReference depthAttachmentRef = new VkAttachmentReference();
            if (DepthTarget != null)
            {
                VkTexture vkDepthTex = Util.AssertSubtype<Texture, VkTexture>(DepthTarget.Value.Target);
                bool hasStencil = FormatHelpers.IsStencilFormat(vkDepthTex.Format);
                depthAttachmentDesc.format = vkDepthTex.VkFormat;
                depthAttachmentDesc.samples = vkDepthTex.VkSampleCount;
                depthAttachmentDesc.loadOp = VkAttachmentLoadOp.Load;
                depthAttachmentDesc.storeOp = VkAttachmentStoreOp.Store;
                depthAttachmentDesc.stencilLoadOp = VkAttachmentLoadOp.DontCare;
                depthAttachmentDesc.stencilStoreOp = hasStencil
                    ? VkAttachmentStoreOp.Store
                    : VkAttachmentStoreOp.DontCare;
                depthAttachmentDesc.initialLayout = ((vkDepthTex.Usage & TextureUsage.Sampled) != 0)
                    ? VkImageLayout.ShaderReadOnlyOptimal
                    : VkImageLayout.DepthStencilAttachmentOptimal;
                depthAttachmentDesc.finalLayout = VkImageLayout.DepthStencilAttachmentOptimal;

                depthAttachmentRef.attachment = (uint)description.ColorTargets.Length;
                depthAttachmentRef.layout = VkImageLayout.DepthStencilAttachmentOptimal;
            }

            VkSubpassDescription subpass = new VkSubpassDescription();
            subpass.pipelineBindPoint = VkPipelineBindPoint.Graphics;
            if (ColorTargets.Count > 0)
            {
                subpass.colorAttachmentCount = colorAttachmentCount;
                subpass.pColorAttachments = (VkAttachmentReference*)colorAttachmentRefs.Data;
            }

            if (DepthTarget != null)
            {
                subpass.pDepthStencilAttachment = &depthAttachmentRef;
                attachments.Add(depthAttachmentDesc);
            }

            VkSubpassDependency subpassDependency = new VkSubpassDependency();
            subpassDependency.srcSubpass = SubpassExternal;
            subpassDependency.srcStageMask = VkPipelineStageFlags.ColorAttachmentOutput;
            subpassDependency.dstStageMask = VkPipelineStageFlags.ColorAttachmentOutput;
            subpassDependency.dstAccessMask = VkAccessFlags.ColorAttachmentRead | VkAccessFlags.ColorAttachmentWrite;

            renderPassCI.attachmentCount = attachments.Count;
            renderPassCI.pAttachments = (VkAttachmentDescription*)attachments.Data;
            renderPassCI.subpassCount = 1;
            renderPassCI.pSubpasses = &subpass;
            renderPassCI.dependencyCount = 1;
            renderPassCI.pDependencies = &subpassDependency;

            VkResult creationResult = vkCreateRenderPass(_gd.Device, ref renderPassCI, null, out _renderPassNoClear);
            CheckResult(creationResult);

            for (int i = 0; i < colorAttachmentCount; i++)
            {
                attachments[i].loadOp = VkAttachmentLoadOp.Load;
                attachments[i].initialLayout = VkImageLayout.ColorAttachmentOptimal;
            }
            if (DepthTarget != null)
            {
                attachments[attachments.Count - 1].loadOp = VkAttachmentLoadOp.Load;
                attachments[attachments.Count - 1].initialLayout = VkImageLayout.DepthStencilAttachmentOptimal;
                bool hasStencil = FormatHelpers.IsStencilFormat(DepthTarget.Value.Target.Format);
                if (hasStencil)
                {
                    attachments[attachments.Count - 1].stencilLoadOp = VkAttachmentLoadOp.Load;
                }

            }
            creationResult = vkCreateRenderPass(_gd.Device, ref renderPassCI, null, out _renderPassNoClearLoad);
            CheckResult(creationResult);

            // Load version

            if (DepthTarget != null)
            {
                attachments[attachments.Count - 1].loadOp = VkAttachmentLoadOp.Clear;
                attachments[attachments.Count - 1].initialLayout = VkImageLayout.Undefined;
                bool hasStencil = FormatHelpers.IsStencilFormat(DepthTarget.Value.Target.Format);
                if (hasStencil)
                {
                    attachments[attachments.Count - 1].stencilLoadOp = VkAttachmentLoadOp.Clear;
                }
            }

            for (int i = 0; i < colorAttachmentCount; i++)
            {
                attachments[i].loadOp = VkAttachmentLoadOp.Clear;
                attachments[i].initialLayout = VkImageLayout.Undefined;
            }

            creationResult = vkCreateRenderPass(_gd.Device, ref renderPassCI, null, out _renderPassClear);
            CheckResult(creationResult);

            VkFramebufferCreateInfo fbCI = VkFramebufferCreateInfo.New();
            uint fbAttachmentsCount = (uint)description.ColorTargets.Length;
            if (description.DepthTarget != null)
            {
                fbAttachmentsCount += 1;
            }
            fbAttachmentsCount += (uint)description.ResolveTargetsOrEmpty().Length;

            VkImageView* fbAttachments = stackalloc VkImageView[(int)fbAttachmentsCount];
            uint currAttach = 0;
            for (int i = 0; i < colorAttachmentCount; i++)
            {
                VkTexture vkColorTarget = Util.AssertSubtype<Texture, VkTexture>(description.ColorTargets[i].Target);
                VkImageViewCreateInfo imageViewCI = VkImageViewCreateInfo.New();
                imageViewCI.image = vkColorTarget.OptimalDeviceImage;
                imageViewCI.format = vkColorTarget.VkFormat;
                imageViewCI.viewType = VkImageViewType.Image2D;
                imageViewCI.subresourceRange = new VkImageSubresourceRange(
                    VkImageAspectFlags.Color,
                    description.ColorTargets[i].MipLevel,
                    1,
                    description.ColorTargets[i].ArrayLayer,
                    1);
                VkImageView* dest = fbAttachments + currAttach;
                currAttach += 1;
                VkResult result = vkCreateImageView(_gd.Device, ref imageViewCI, null, dest);
                CheckResult(result);
                _attachmentViews.Add(*dest);
            }

            // Depth
            if (description.DepthTarget != null)
            {
                VkTexture vkDepthTarget = Util.AssertSubtype<Texture, VkTexture>(description.DepthTarget.Value.Target);
                bool hasStencil = FormatHelpers.IsStencilFormat(vkDepthTarget.Format);
                VkImageViewCreateInfo depthViewCI = VkImageViewCreateInfo.New();
                depthViewCI.image = vkDepthTarget.OptimalDeviceImage;
                depthViewCI.format = vkDepthTarget.VkFormat;
                depthViewCI.viewType = description.DepthTarget.Value.Target.ArrayLayers == 1
                    ? VkImageViewType.Image2D
                    : VkImageViewType.Image2DArray;
                depthViewCI.subresourceRange = new VkImageSubresourceRange(
                    hasStencil ? VkImageAspectFlags.Depth | VkImageAspectFlags.Stencil : VkImageAspectFlags.Depth,
                    description.DepthTarget.Value.MipLevel,
                    1,
                    description.DepthTarget.Value.ArrayLayer,
                    1);
                VkImageView* dest = fbAttachments + currAttach;
                currAttach += 1;
                VkResult result = vkCreateImageView(_gd.Device, ref depthViewCI, null, dest);
                CheckResult(result);
                _attachmentViews.Add(*dest);
            }

            // Resolve
            for (int i = 0; i < ResolveTargets.Count; i++)
            {
                if (ResolveTargets.Count > i
                    && ResolveTargets[i].Target is VkTexture vkResolveTarget)
                {
                    VkImageViewCreateInfo imageViewCI = VkImageViewCreateInfo.New();
                    imageViewCI.image = vkResolveTarget.OptimalDeviceImage;
                    imageViewCI.format = vkResolveTarget.VkFormat;
                    imageViewCI.viewType = VkImageViewType.Image2D;
                    imageViewCI.subresourceRange = new VkImageSubresourceRange(
                        VkImageAspectFlags.Color,
                        ResolveTargets[i].MipLevel,
                        1,
                        ResolveTargets[i].ArrayLayer,
                        1);
                    VkImageView* dest = fbAttachments + currAttach;
                    currAttach += 1;
                    VkResult result = vkCreateImageView(_gd.Device, ref imageViewCI, null, dest);
                    CheckResult(result);
                    _attachmentViews.Add(*dest);
                }
            }

            Texture dimTex;
            uint mipLevel;
            if (ColorTargets.Count > 0)
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
                out uint mipHeight,
                out _);

            fbCI.width = mipWidth;
            fbCI.height = mipHeight;

            fbCI.attachmentCount = fbAttachmentsCount;
            fbCI.pAttachments = fbAttachments;
            fbCI.layers = 1;
            fbCI.renderPass = _renderPassNoClear;

            if (ResolveTargets.Count > 0)
            {
                RenderPassDescription rpDesc = new RenderPassDescription(
                    this,
                    LoadAction.Load,
                    default,
                    LoadAction.Load,
                    default,
                    StoreAction.Store,
                    StoreAction.Store);
                VkRenderPass rp = GetRenderPass(rpDesc);
                fbCI.renderPass = rp;
            }

            creationResult = vkCreateFramebuffer(_gd.Device, ref fbCI, null, out _deviceFramebuffer);
            CheckResult(creationResult);

            if (DepthTarget != null)
            {
                AttachmentCount += 1;
            }
            AttachmentCount += (uint)ColorTargets.Count;
            AttachmentCount += (uint)ResolveTargets.Count;
        }

        internal VkRenderPass GetRenderPass(in RenderPassDescription desc)
        {
            if (!_renderPasses.TryGetValue(desc, out VkRenderPass ret))
            {
                VkRenderPassCreateInfo renderPassCI = VkRenderPassCreateInfo.New();

                uint colorCount = (uint)ColorTargets.Count;
                uint depthCount = DepthTarget != null ? 1u : 0u;
                uint resolveCount = (uint)ResolveTargets.Count;

                uint totalAttachmentCount = colorCount + depthCount + resolveCount;
                VkAttachmentDescription* attachments = stackalloc VkAttachmentDescription[(int)totalAttachmentCount];
                VkAttachmentReference* colorRefs = stackalloc VkAttachmentReference[(int)colorCount];
                VkAttachmentReference* resolveRefs = stackalloc VkAttachmentReference[(int)resolveCount];

                for (int i = 0; i < colorCount; i++)
                {
                    desc.GetColorAttachment(
                        (uint)i,
                        out LoadAction vdLoad,
                        out StoreAction vdStore,
                        out _);

                    VkTexture vkColorTex = Util.AssertSubtype<Texture, VkTexture>(ColorTargets[i].Target);
                    attachments[i].format = vkColorTex.VkFormat;
                    attachments[i].samples = vkColorTex.VkSampleCount;
                    attachments[i].loadOp = VkFormats.VdToVkLoadAction(vdLoad);
                    attachments[i].storeOp = VkFormats.VdToVkStoreAction(vdStore);
                    attachments[i].initialLayout = VulkanUtil.GetFinalLayout(vkColorTex);
                    attachments[i].finalLayout = VulkanUtil.GetFinalLayout(vkColorTex);

                    colorRefs[i].attachment = (uint)i;
                    colorRefs[i].layout = VkImageLayout.ColorAttachmentOptimal;

                    uint resolveAttachmentOffset = colorCount + depthCount;
                    uint resolveIndex = (uint)i + resolveAttachmentOffset;
                    if (ResolveTargets.Count > i)
                    {
                        FramebufferAttachment resolveAttachment = ResolveTargets[(int)i];
                        if (resolveAttachment.Target is VkTexture vkResolveTex)
                        {
                            attachments[resolveIndex].format = vkResolveTex.VkFormat;
                            attachments[resolveIndex].samples = vkResolveTex.VkSampleCount;
                            attachments[resolveIndex].initialLayout = VulkanUtil.GetFinalLayout(vkResolveTex);
                            attachments[resolveIndex].finalLayout = VulkanUtil.GetFinalLayout(vkResolveTex);

                            resolveRefs[i].attachment = resolveIndex;
                            resolveRefs[i].layout = VulkanUtil.GetFinalLayout(vkResolveTex);
                        }
                        else
                        {
                            resolveRefs[i].attachment = RawConstants.VK_ATTACHMENT_UNUSED;
                        }
                    }
                }

                VkAttachmentReference depthAttachmentRef = new VkAttachmentReference();
                if (DepthTarget != null)
                {
                    VkTexture vkDepthTex = Util.AssertSubtype<Texture, VkTexture>(DepthTarget.Value.Target);
                    bool hasStencil = FormatHelpers.IsStencilFormat(vkDepthTex.Format);
                    attachments[colorCount].format = vkDepthTex.VkFormat;
                    attachments[colorCount].samples = vkDepthTex.VkSampleCount;
                    attachments[colorCount].loadOp = VkFormats.VdToVkLoadAction(desc.DepthLoadAction);
                    attachments[colorCount].storeOp = VkFormats.VdToVkStoreAction(desc.DepthStoreAction);
                    attachments[colorCount].stencilLoadOp = VkFormats.VdToVkLoadAction(desc.StencilLoadAction);
                    attachments[colorCount].stencilStoreOp = VkFormats.VdToVkStoreAction(desc.StencilStoreAction);
                    attachments[colorCount].initialLayout = VulkanUtil.GetFinalLayout(vkDepthTex);
                    attachments[colorCount].finalLayout = VulkanUtil.GetFinalLayout(vkDepthTex);

                    depthAttachmentRef.attachment = colorCount;
                    depthAttachmentRef.layout = VkImageLayout.DepthStencilAttachmentOptimal;
                }

                VkSubpassDescription subpass = new VkSubpassDescription();
                subpass.pipelineBindPoint = VkPipelineBindPoint.Graphics;
                if (ColorTargets.Count > 0)
                {
                    subpass.colorAttachmentCount = colorCount;
                    subpass.pColorAttachments = colorRefs;
                    subpass.pResolveAttachments = resolveRefs;
                }

                if (DepthTarget != null)
                {
                    subpass.pDepthStencilAttachment = &depthAttachmentRef;
                }

                renderPassCI.attachmentCount = totalAttachmentCount;
                renderPassCI.pAttachments = attachments;
                renderPassCI.subpassCount = 1;
                renderPassCI.pSubpasses = &subpass;

                VkResult creationResult = vkCreateRenderPass(_gd.Device, ref renderPassCI, null, out ret);
                CheckResult(creationResult);

                _renderPasses.Add(desc, ret);
            }

            return ret;
        }

        public override void TransitionToIntermediateLayout(VkCommandBuffer cb)
        {
            for (int i = 0; i < ColorTargets.Count; i++)
            {
                FramebufferAttachment ca = ColorTargets[i];
                VkTexture vkTex = Util.AssertSubtype<Texture, VkTexture>(ca.Target);
                vkTex.SetImageLayout(ca.MipLevel, ca.ArrayLayer, VkImageLayout.ColorAttachmentOptimal);
            }
            if (DepthTarget != null)
            {
                VkTexture vkTex = Util.AssertSubtype<Texture, VkTexture>(DepthTarget.Value.Target);
                vkTex.SetImageLayout(
                    DepthTarget.Value.MipLevel,
                    DepthTarget.Value.ArrayLayer,
                    VkImageLayout.DepthStencilAttachmentOptimal);
            }
        }

        public override void TransitionToFinalLayout(VkCommandBuffer cb)
        {
            for (int i = 0; i < ColorTargets.Count; i++)
            {
                FramebufferAttachment ca = ColorTargets[i];
                VkTexture vkTex = Util.AssertSubtype<Texture, VkTexture>(ca.Target);
                if ((vkTex.Usage & TextureUsage.Sampled) != 0)
                {
                    vkTex.TransitionImageLayout(
                        cb,
                        ca.MipLevel, 1,
                        ca.ArrayLayer, 1,
                        VkImageLayout.ShaderReadOnlyOptimal);
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
                        VkImageLayout.ShaderReadOnlyOptimal);
                }
            }
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

                foreach (KeyValuePair<RenderPassDescription, VkRenderPass> kvp in _renderPasses)
                {
                    vkDestroyRenderPass(_gd.Device, kvp.Value, null);
                }

                _destroyed = true;
            }
        }
    }
}
