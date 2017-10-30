using Vulkan;
using static Vulkan.VulkanNative;
using static Veldrid.Vk.VulkanUtil;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Veldrid.Vk
{
    internal unsafe class VkSwapchainFramebuffer : VkFramebufferBase
    {
        private readonly VkGraphicsDevice _gd;
        private readonly VkSurfaceKHR _surface;

        private VkSwapchainKHR _swapchain;

        private VkFramebuffer[] _scFramebuffers;
        private uint _currentImageIndex;
        private VkImage[] _scImages;
        private VkFormat _scImageFormat;
        private VkExtent2D _scExtent;
        private VkTexture2D[][] _scColorTextures;

        private VkTexture2D _depthTexture;

        public override Vulkan.VkFramebuffer CurrentFramebuffer => _scFramebuffers[(int)_currentImageIndex].CurrentFramebuffer;

        public override VkRenderPass RenderPass => _scFramebuffers[0].RenderPass;

        public override IReadOnlyList<Texture2D> ColorTextures => _scColorTextures[(int)_currentImageIndex];

        public override Texture2D DepthTexture => _depthTexture;

        public override uint Width => _scExtent.width;
        public override uint Height => _scExtent.height;

        public VkSwapchainKHR Swapchain => _swapchain;

        public uint ImageIndex => _currentImageIndex;

        public override OutputDescription OutputDescription { get; }

        public VkSwapchainFramebuffer(VkGraphicsDevice gd, VkSurfaceKHR surface, uint width, uint height)
            : base()
        {
            _gd = gd;
            _surface = surface;
            CreateSwapchain(width, height);
            OutputDescription = OutputDescription.CreateFromFramebuffer(this);
        }

        public void AcquireNextImage(VkDevice device, VkSemaphore semaphore, VkFence fence)
        {
            VkResult result = vkAcquireNextImageKHR(
                device,
                _swapchain,
                ulong.MaxValue,
                semaphore,
                fence,
                ref _currentImageIndex);
            if (result == VkResult.ErrorOutOfDateKHR || result == VkResult.SuboptimalKHR)
            {
                // RecreateSwapChain();
            }
            else if (result != VkResult.Success)
            {
                throw new VeldridException("Could not acquire next image from the Vulkan swapchain.");
            }
        }

        public void Resize(uint width, uint height)
        {
            CreateSwapchain(width, height);
        }

        private void CreateSwapchain(uint width, uint height)
        {
            _currentImageIndex = 0;
            uint surfaceFormatCount = 0;
            vkGetPhysicalDeviceSurfaceFormatsKHR(_gd.PhysicalDevice, _surface, ref surfaceFormatCount, null);
            VkSurfaceFormatKHR[] formats = new VkSurfaceFormatKHR[surfaceFormatCount];
            vkGetPhysicalDeviceSurfaceFormatsKHR(_gd.PhysicalDevice, _surface, ref surfaceFormatCount, out formats[0]);

            VkSurfaceFormatKHR surfaceFormat = new VkSurfaceFormatKHR();
            if (formats.Length == 1 && formats[0].format == VkFormat.Undefined)
            {
                surfaceFormat = new VkSurfaceFormatKHR { colorSpace = VkColorSpaceKHR.SrgbNonlinearKHR, format = VkFormat.B8g8r8a8Unorm };
            }
            else
            {
                foreach (VkSurfaceFormatKHR format in formats)
                {
                    if (format.colorSpace == VkColorSpaceKHR.SrgbNonlinearKHR && format.format == VkFormat.B8g8r8a8Unorm)
                    {
                        surfaceFormat = format;
                        break;
                    }
                }
                if (surfaceFormat.format == VkFormat.Undefined)
                {
                    surfaceFormat = formats[0];
                }
            }

            uint presentModeCount = 0;
            vkGetPhysicalDeviceSurfacePresentModesKHR(_gd.PhysicalDevice, _surface, ref presentModeCount, null);
            VkPresentModeKHR[] presentModes = new VkPresentModeKHR[presentModeCount];
            vkGetPhysicalDeviceSurfacePresentModesKHR(_gd.PhysicalDevice, _surface, ref presentModeCount, out presentModes[0]);

            VkPresentModeKHR presentMode = VkPresentModeKHR.FifoKHR;
            if (presentModes.Contains(VkPresentModeKHR.MailboxKHR))
            {
                presentMode = VkPresentModeKHR.MailboxKHR;
            }
            else if (presentModes.Contains(VkPresentModeKHR.ImmediateKHR))
            {
                presentMode = VkPresentModeKHR.ImmediateKHR;
            }

            vkGetPhysicalDeviceSurfaceCapabilitiesKHR(_gd.PhysicalDevice, _surface, out VkSurfaceCapabilitiesKHR surfaceCapabilities);
            uint imageCount = surfaceCapabilities.minImageCount + 1;

            VkSwapchainCreateInfoKHR swapchainCI = VkSwapchainCreateInfoKHR.New();
            swapchainCI.surface = _surface;
            swapchainCI.presentMode = presentMode;
            swapchainCI.imageFormat = surfaceFormat.format;
            swapchainCI.imageColorSpace = surfaceFormat.colorSpace;
            swapchainCI.imageExtent = new VkExtent2D { width = (uint)width, height = (uint)height };
            swapchainCI.minImageCount = imageCount;
            swapchainCI.imageArrayLayers = 1;
            swapchainCI.imageUsage = VkImageUsageFlags.ColorAttachment;

            FixedArray2<uint> queueFamilyIndices = new FixedArray2<uint>(_gd.GraphicsQueueIndex, _gd.PresentQueueIndex);

            if (_gd.GraphicsQueueIndex != _gd.PresentQueueIndex)
            {
                swapchainCI.imageSharingMode = VkSharingMode.Concurrent;
                swapchainCI.queueFamilyIndexCount = 2;
                swapchainCI.pQueueFamilyIndices = &queueFamilyIndices.First;
            }
            else
            {
                swapchainCI.imageSharingMode = VkSharingMode.Exclusive;
                swapchainCI.queueFamilyIndexCount = 0;
            }

            swapchainCI.preTransform = surfaceCapabilities.currentTransform;
            swapchainCI.compositeAlpha = VkCompositeAlphaFlagsKHR.OpaqueKHR;
            swapchainCI.clipped = true;

            VkSwapchainKHR oldSwapchain = _swapchain;
            swapchainCI.oldSwapchain = oldSwapchain;

            VkResult result = vkCreateSwapchainKHR(_gd.Device, ref swapchainCI, null, out _swapchain);
            CheckResult(result);
            if (oldSwapchain != VkSwapchainKHR.Null)
            {
                vkDestroySwapchainKHR(_gd.Device, oldSwapchain, null);
            }

            // Get the images
            uint scImageCount = 0;
            result = vkGetSwapchainImagesKHR(_gd.Device, _swapchain, ref scImageCount, null);
            CheckResult(result);
            if (_scImages == null)
            {
                _scImages = new VkImage[(int)scImageCount];
            }
            result = vkGetSwapchainImagesKHR(_gd.Device, _swapchain, ref scImageCount, out _scImages[0]);
            CheckResult(result);

            _scImageFormat = surfaceFormat.format;
            _scExtent = swapchainCI.imageExtent;

            CreateDepthTexture();
            CreateFramebuffers();
        }

        private void CreateDepthTexture()
        {
            _depthTexture?.Dispose();
            _depthTexture = (VkTexture2D)_gd.ResourceFactory.CreateTexture2D(new TextureDescription(
                _scExtent.width,
                _scExtent.height,
                1,
                1,
                 PixelFormat.R16_UNorm,
                TextureUsage.DepthStencil));
        }

        private void CreateFramebuffers()
        {
            if (_scFramebuffers != null)
            {
                for (int i = 0; i < _scFramebuffers.Length; i++)
                {
                    _scFramebuffers[i]?.Dispose();
                    _scFramebuffers[i] = null;
                }
                Array.Clear(_scFramebuffers, 0, _scFramebuffers.Length);
            }

            EnsureSize(ref _scFramebuffers, _scImages.Length);
            EnsureSize(ref _scColorTextures, _scImages.Length);
            for (uint i = 0; i < _scImages.Length; i++)
            {
                VkTexture2D colorTex = new VkTexture2D(_gd, _scExtent.width, _scExtent.height, 1, 1, _scImageFormat, TextureUsage.RenderTarget, _scImages[i]);
                FramebufferDescription desc = new FramebufferDescription(_depthTexture, colorTex);
                VkFramebuffer fb = (VkFramebuffer)_gd.ResourceFactory.CreateFramebuffer(ref desc);
                _scFramebuffers[i] = fb;
                _scColorTextures[i] = new VkTexture2D[] { colorTex };
            }
        }

        private void EnsureSize<T>(ref T[] array, int size)
        {
            if (array == null)
            {
                array = new T[size];
            }
            else
            {
                Util.EnsureArraySize(ref array, (uint)size);
            }
        }

        public override void Dispose()
        {
            for (int i = 0; i < _scFramebuffers.Length; i++)
            {
                _scFramebuffers[i].Dispose();
            }
        }
    }
}
