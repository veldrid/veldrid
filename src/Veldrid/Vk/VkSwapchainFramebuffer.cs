using System;
using System.Diagnostics;
using TerraFX.Interop.Vulkan;
using static TerraFX.Interop.Vulkan.Vulkan;
using static Veldrid.Vulkan.VulkanUtil;

namespace Veldrid.Vulkan
{
    internal sealed unsafe class VkSwapchainFramebuffer : VkFramebufferBase
    {
        private readonly VkGraphicsDevice _gd;
        private readonly VkSwapchain _swapchain;
        private readonly VkSurfaceKHR _surface;
        private readonly PixelFormat? _depthFormat;
        private uint _currentImageIndex;

        private VkFramebuffer[] _scFramebuffers = Array.Empty<VkFramebuffer>();
        private VkImage[] _scImages = Array.Empty<VkImage>();
        private VkFormat _scImageFormat;
        private VkExtent2D _scExtent;

        private string? _name;

        public override TerraFX.Interop.Vulkan.VkFramebuffer CurrentFramebuffer => _scFramebuffers[_currentImageIndex].CurrentFramebuffer;

        public override VkRenderPass RenderPassNoClear_Init => _scFramebuffers[0].RenderPassNoClear_Init;
        public override VkRenderPass RenderPassNoClear_Load => _scFramebuffers[0].RenderPassNoClear_Load;
        public override VkRenderPass RenderPassClear => _scFramebuffers[0].RenderPassClear;

        public override VkExtent2D RenderableExtent => _scExtent;

        public uint ImageIndex => _currentImageIndex;

        public VkSwapchain Swapchain => _swapchain;

        public VkSwapchainFramebuffer(
            VkGraphicsDevice gd,
            VkSwapchain swapchain,
            VkSurfaceKHR surface,
            in SwapchainDescription description)
            : base()
        {
            _gd = gd;
            _swapchain = swapchain;
            _surface = surface;
            _depthFormat = description.DepthFormat;

            AttachmentCount = _depthFormat.HasValue ? 2u : 1u; // 1 Color + 1 Depth
        }

        internal void SetImageIndex(uint index)
        {
            _currentImageIndex = index;
            _colorTargets = _scFramebuffers[_currentImageIndex].ColorTargetArray;
        }

        internal void SetNewSwapchain(
            VkSwapchainKHR deviceSwapchain,
            uint width,
            uint height,
            VkSurfaceFormatKHR surfaceFormat,
            VkExtent2D swapchainExtent)
        {
            Width = width;
            Height = height;

            // Get the images
            uint scImageCount = 0;
            VkResult result = vkGetSwapchainImagesKHR(_gd.Device, deviceSwapchain, &scImageCount, null);
            CheckResult(result);
            if (_scImages.Length < scImageCount)
            {
                _scImages = new VkImage[(int)scImageCount];
            }
            fixed (VkImage* scImagesPtr = _scImages)
            {
                result = vkGetSwapchainImagesKHR(_gd.Device, deviceSwapchain, &scImageCount, scImagesPtr);
                CheckResult(result);
            }

            _scImageFormat = surfaceFormat.format;
            _scExtent = swapchainExtent;

            CreateFramebuffers();

            OutputDescription = OutputDescription.CreateFromFramebuffer(this);
        }

        private void DestroySwapchainFramebuffers()
        {
            _depthTarget?.Target.Dispose();
            _depthTarget = default;

            foreach (ref VkFramebuffer fb in _scFramebuffers.AsSpan())
            {
                if (fb != null)
                {
                    foreach (FramebufferAttachment attachment in fb.ColorTargets)
                    {
                        attachment.Target.Dispose();
                    }
                    fb.Dispose();
                    fb = null!;
                }
            }
        }

        private void CreateDepthTexture()
        {
            if (_depthFormat.HasValue)
            {
                Debug.Assert(!_depthTarget.HasValue);

                VkTexture depthTexture = (VkTexture)_gd.ResourceFactory.CreateTexture(TextureDescription.Texture2D(
                    Math.Max(1, _scExtent.width),
                    Math.Max(1, _scExtent.height),
                    1,
                    1,
                    _depthFormat.Value,
                    TextureUsage.DepthStencil));
                _depthTarget = new FramebufferAttachment(depthTexture, 0);
            }
        }

        private void CreateFramebuffers()
        {
            DestroySwapchainFramebuffers();

            CreateDepthTexture();

            Util.EnsureArrayMinimumSize(ref _scFramebuffers, (uint)_scImages.Length);

            for (uint i = 0; i < _scImages.Length; i++)
            {
                VkTexture colorTex = new(
                    _gd,
                    Math.Max(1, _scExtent.width),
                    Math.Max(1, _scExtent.height),
                    1,
                    1,
                    _scImageFormat,
                    TextureUsage.RenderTarget,
                    TextureSampleCount.Count1,
                    _scImages[i],
                    true,
                    true);
                FramebufferDescription desc = new(_depthTarget?.Target, colorTex);
                VkFramebuffer fb = new(_gd, desc, true);
                _scFramebuffers[i] = fb;
            }

            SetImageIndex(0);
        }

        public override void TransitionToIntermediateLayout(VkCommandBuffer cb)
        {
            foreach (ref readonly FramebufferAttachment ca in ColorTargets)
            {
                VkTexture vkTex = Util.AssertSubtype<Texture, VkTexture>(ca.Target);
                vkTex.SetImageLayout(0, ca.ArrayLayer, VkImageLayout.VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL);
            }
        }

        public override void TransitionToFinalLayout(VkCommandBuffer cb, bool attachment)
        {
            VkImageLayout layout = attachment
                ? VkImageLayout.VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL
                : VkImageLayout.VK_IMAGE_LAYOUT_PRESENT_SRC_KHR;

            foreach (ref readonly FramebufferAttachment ca in ColorTargets)
            {
                VkTexture vkTex = Util.AssertSubtype<Texture, VkTexture>(ca.Target);
                vkTex.TransitionImageLayout(cb, 0, 1, ca.ArrayLayer, 1, layout);
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
            DestroySwapchainFramebuffers();
        }
    }
}
