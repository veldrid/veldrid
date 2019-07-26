using System;
using System.Linq;
using Vulkan;
using static Veldrid.Vk.VulkanUtil;
using static Vulkan.VulkanNative;

namespace Veldrid.Vk
{
    internal unsafe class VkSwapchain : Swapchain
    {
        private readonly VkGraphicsDevice _gd;
        private readonly VkSurfaceKHR _surface;
        private VkSwapchainKHR _deviceSwapchain;
        private readonly VkSwapchainFramebuffer _framebuffer;
        private Fence _imageAvailableFence;
        private readonly uint _presentQueueIndex;
        private readonly VkQueue _presentQueue;
        private bool _syncToVBlank;
        private readonly SwapchainSource _swapchainSource;
        private readonly bool _colorSrgb;
        private uint _currentImageIndex;
        private string _name;
        private Framebuffer[] _framebuffers;

        public override string Name { get => _name; set { _name = value; _gd.SetResourceName(this, value); } }
        public override Framebuffer Framebuffer => _framebuffer;
        public override bool SyncToVerticalBlank
        {
            get => _syncToVBlank;
            set
            {
                if (_syncToVBlank != value)
                {
                    _syncToVBlank = value;
                    Resize(Width, Height);
                }
            }
        }

        public VkSwapchainKHR DeviceSwapchain => _deviceSwapchain;
        public uint ImageIndex => _currentImageIndex;
        public Fence ImageAvailableFence => _imageAvailableFence;
        public VkSurfaceKHR Surface => _surface;
        public VkQueue PresentQueue => _presentQueue;
        public uint PresentQueueIndex => _presentQueueIndex;
        public ResourceRefCount RefCount { get; }

        public override Framebuffer[] Framebuffers => _framebuffers;

        public override uint LastAcquiredImage => _currentImageIndex;

        public VkSwapchain(VkGraphicsDevice gd, ref SwapchainDescription description) : this(gd, ref description, VkSurfaceKHR.Null) { }

        public VkSwapchain(VkGraphicsDevice gd, ref SwapchainDescription description, VkSurfaceKHR existingSurface)
        {
            _gd = gd;
            _syncToVBlank = description.SyncToVerticalBlank;
            _swapchainSource = description.Source;
            _colorSrgb = description.ColorSrgb;

            if (existingSurface == VkSurfaceKHR.Null)
            {
                _surface = VkSurfaceUtil.CreateSurface(gd, gd.Instance, _swapchainSource);
            }
            else
            {
                _surface = existingSurface;
            }

            if (!GetPresentQueueIndex(out _presentQueueIndex))
            {
                throw new VeldridException($"The system does not support presenting the given Vulkan surface.");
            }
            vkGetDeviceQueue(_gd.Device, _presentQueueIndex, 0, out _presentQueue);

            _framebuffer = new VkSwapchainFramebuffer(gd, this, _surface, description.Width, description.Height, description.DepthFormat);

            CreateSwapchain(description.Width, description.Height);

            _imageAvailableFence = _gd.ResourceFactory.CreateFence(false);

            RefCount = new ResourceRefCount(DisposeCore);

            if (!_gd.Features.CommandBuffers)
            {
                AcquireNextImage(null, _imageAvailableFence);
                _imageAvailableFence.Wait();
                _imageAvailableFence.Reset();
            }
        }

        public override void Resize(uint width, uint height)
        {
            CreateSwapchain(width, height);
            if (!_gd.Features.CommandBuffers)
            {
                AcquireNextImage(null, _imageAvailableFence);
                _imageAvailableFence.Wait();
                _imageAvailableFence.Reset();
            }
        }

        public void RecreateSwapchain()
        {
            CreateSwapchain(Width, Height);
        }

        public AcquireResult AcquireNextImage(Semaphore semaphore, Fence fence)
        {
            VulkanSemaphore vkSemaphore = semaphore != null
                ? Util.AssertSubtype<Semaphore, VulkanSemaphore>(semaphore)
                : null;
            VkFence vkFence = fence != null ? Util.AssertSubtype<Fence, VkFence>(fence) : null;


            AcquireResult acquireResult = AcquireResult.Success;
            _currentImageIndex = 0;
            VkResult result = vkAcquireNextImageKHR(
                _gd.Device,
                DeviceSwapchain,
                ulong.MaxValue,
                vkSemaphore?.NativeSemaphore ?? VkSemaphore.Null,
                vkFence?.DeviceFence ?? Vulkan.VkFence.Null,
                ref _currentImageIndex);
            if (result == VkResult.ErrorOutOfDateKHR)
            {
                acquireResult = AcquireResult.OutOfDate;
            }
            else
            {
                CheckResult(result);
            }

            return acquireResult;
        }

        private void RecreateAndReacquire(uint width, uint height)
        {
            if (CreateSwapchain(width, height))
            {
                AcquireNextImage(null, _imageAvailableFence);
                _imageAvailableFence.Wait();
                _imageAvailableFence.Reset();
            }
        }

        private bool CreateSwapchain(uint width, uint height)
        {
            // Obtain the surface capabilities first -- this will indicate whether the surface has been lost.
            VkResult result = vkGetPhysicalDeviceSurfaceCapabilitiesKHR(_gd.PhysicalDevice, _surface, out VkSurfaceCapabilitiesKHR surfaceCapabilities);
            if (result == VkResult.ErrorSurfaceLostKHR)
            {
                throw new VeldridException($"The Swapchain's underlying surface has been lost.");
            }

            if (surfaceCapabilities.minImageExtent.width == 0 && surfaceCapabilities.minImageExtent.height == 0
                && surfaceCapabilities.maxImageExtent.width == 0 && surfaceCapabilities.maxImageExtent.height == 0)
            {
                return false;
            }

            if (_deviceSwapchain != VkSwapchainKHR.Null)
            {
                _gd.WaitForIdle();
            }

            uint surfaceFormatCount = 0;
            result = vkGetPhysicalDeviceSurfaceFormatsKHR(_gd.PhysicalDevice, _surface, ref surfaceFormatCount, null);
            CheckResult(result);
            VkSurfaceFormatKHR[] formats = new VkSurfaceFormatKHR[surfaceFormatCount];
            result = vkGetPhysicalDeviceSurfaceFormatsKHR(_gd.PhysicalDevice, _surface, ref surfaceFormatCount, out formats[0]);
            CheckResult(result);

            VkFormat[] desiredFormats = _colorSrgb
                ? VkFormats.SrgbSwapchainFormats
                : VkFormats.StandardSwapchainFormats;

            VkSurfaceFormatKHR surfaceFormat = new VkSurfaceFormatKHR();
            if (formats.Length == 1 && formats[0].format == VkFormat.Undefined)
            {
                surfaceFormat = new VkSurfaceFormatKHR { colorSpace = VkColorSpaceKHR.SrgbNonlinearKHR, format = desiredFormats[0] };
            }
            else
            {
                foreach (VkSurfaceFormatKHR format in formats)
                {
                    foreach (VkFormat desiredFormat in desiredFormats)
                    {
                        if (format.colorSpace == VkColorSpaceKHR.SrgbNonlinearKHR && format.format == desiredFormat)
                        {
                            surfaceFormat = format;
                            break;
                        }
                    }
                }
                if (surfaceFormat.format == VkFormat.Undefined)
                {
                    surfaceFormat = formats[0];

                    PixelFormat vdFormat = VkFormats.VkToVdPixelFormat(surfaceFormat.format);
                    if ((_colorSrgb && !FormatHelpers.IsSrgbFormat(vdFormat))
                        || (!_colorSrgb && FormatHelpers.IsSrgbFormat(vdFormat)))
                    {
                        throw new VeldridException($"Failed to identify a suitable Swapchain format.");
                    }
                }
            }

            uint presentModeCount = 0;
            result = vkGetPhysicalDeviceSurfacePresentModesKHR(_gd.PhysicalDevice, _surface, ref presentModeCount, null);
            CheckResult(result);
            VkPresentModeKHR[] presentModes = new VkPresentModeKHR[presentModeCount];
            result = vkGetPhysicalDeviceSurfacePresentModesKHR(_gd.PhysicalDevice, _surface, ref presentModeCount, out presentModes[0]);
            CheckResult(result);

            VkPresentModeKHR presentMode = VkPresentModeKHR.FifoKHR;

            if (_syncToVBlank)
            {
                if (presentModes.Contains(VkPresentModeKHR.FifoRelaxedKHR))
                {
                    presentMode = VkPresentModeKHR.FifoRelaxedKHR;
                }
            }
            else
            {
                if (presentModes.Contains(VkPresentModeKHR.MailboxKHR))
                {
                    presentMode = VkPresentModeKHR.MailboxKHR;
                }
                else if (presentModes.Contains(VkPresentModeKHR.ImmediateKHR))
                {
                    presentMode = VkPresentModeKHR.ImmediateKHR;
                }
            }

            uint maxImageCount = surfaceCapabilities.maxImageCount == 0 ? uint.MaxValue : surfaceCapabilities.maxImageCount;
            uint imageCount = Math.Min(maxImageCount, surfaceCapabilities.minImageCount + 1);

            VkSwapchainCreateInfoKHR swapchainCI = VkSwapchainCreateInfoKHR.New();
            swapchainCI.surface = _surface;
            swapchainCI.presentMode = presentMode;
            swapchainCI.imageFormat = surfaceFormat.format;
            swapchainCI.imageColorSpace = surfaceFormat.colorSpace;
            uint clampedWidth = Util.Clamp(width, surfaceCapabilities.minImageExtent.width, surfaceCapabilities.maxImageExtent.width);
            uint clampedHeight = Util.Clamp(height, surfaceCapabilities.minImageExtent.height, surfaceCapabilities.maxImageExtent.height);
            swapchainCI.imageExtent = new VkExtent2D { width = clampedWidth, height = clampedHeight };
            swapchainCI.minImageCount = imageCount;
            swapchainCI.imageArrayLayers = 1;
            swapchainCI.imageUsage = VkImageUsageFlags.ColorAttachment | VkImageUsageFlags.TransferDst;

            FixedArray2<uint> queueFamilyIndices = new FixedArray2<uint>(_gd.UniversalQueueIndex, _gd.PresentQueueIndex);

            if (_gd.UniversalQueueIndex != _gd.PresentQueueIndex)
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

            swapchainCI.preTransform = VkSurfaceTransformFlagsKHR.IdentityKHR;
            swapchainCI.compositeAlpha = VkCompositeAlphaFlagsKHR.OpaqueKHR;
            swapchainCI.clipped = true;

            VkSwapchainKHR oldSwapchain = _deviceSwapchain;
            swapchainCI.oldSwapchain = oldSwapchain;

            result = vkCreateSwapchainKHR(_gd.Device, ref swapchainCI, null, out _deviceSwapchain);
            CheckResult(result);
            if (oldSwapchain != VkSwapchainKHR.Null)
            {
                vkDestroySwapchainKHR(_gd.Device, oldSwapchain, null);
            }

            _framebuffer.SetNewSwapchain(_deviceSwapchain, width, height, surfaceFormat, swapchainCI.imageExtent);
            _framebuffers = _framebuffer.Framebuffers;
            _currentImageIndex = (uint)(_framebuffers.Length - 1);
            return true;
        }

        private bool GetPresentQueueIndex(out uint queueFamilyIndex)
        {
            uint graphicsQueueIndex = _gd.UniversalQueueIndex;
            uint presentQueueIndex = _gd.PresentQueueIndex;

            if (QueueSupportsPresent(graphicsQueueIndex, _surface))
            {
                queueFamilyIndex = graphicsQueueIndex;
                return true;
            }
            else if (graphicsQueueIndex != presentQueueIndex && QueueSupportsPresent(presentQueueIndex, _surface))
            {
                queueFamilyIndex = presentQueueIndex;
                return true;
            }

            queueFamilyIndex = 0;
            return false;
        }

        private bool QueueSupportsPresent(uint queueFamilyIndex, VkSurfaceKHR surface)
        {
            VkResult result = vkGetPhysicalDeviceSurfaceSupportKHR(
                _gd.PhysicalDevice,
                queueFamilyIndex,
                surface,
                out VkBool32 supported);
            CheckResult(result);
            return supported;
        }

        public override void Dispose()
        {
            RefCount.Decrement();
        }

        private void DisposeCore()
        {
            _imageAvailableFence.Dispose();
            _framebuffer.Dispose();
            vkDestroySwapchainKHR(_gd.Device, _deviceSwapchain, null);
        }
    }
}
