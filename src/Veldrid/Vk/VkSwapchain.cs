using System;
using System.Linq;
using TerraFX.Interop.Vulkan;
using static TerraFX.Interop.Vulkan.Vulkan;
using static TerraFX.Interop.Vulkan.VkFormat;
using static Veldrid.Vulkan.VulkanUtil;
using VulkanFence = TerraFX.Interop.Vulkan.VkFence;

namespace Veldrid.Vulkan
{
    internal sealed unsafe class VkSwapchain : Swapchain, IResourceRefCountTarget
    {
        private readonly VkGraphicsDevice _gd;
        private readonly VkSurfaceKHR _surface;
        private VkSwapchainKHR _deviceSwapchain;
        private readonly VkSwapchainFramebuffer _framebuffer;
        private VulkanFence _imageAvailableFence;
        private readonly uint _presentQueueIndex;
        private readonly VkQueue _presentQueue;
        private bool _syncToVBlank;
        private readonly SwapchainSource _swapchainSource;
        private readonly bool _colorSrgb;
        private bool? _newSyncToVBlank;
        private uint _currentImageIndex;
        private string? _name;
        private bool _disposed;

        public override string? Name { get => _name; set { _name = value; _gd.SetResourceName(this, value); } }

        public override Framebuffer Framebuffer => _framebuffer;

        public override bool SyncToVerticalBlank
        {
            get => _newSyncToVBlank ?? _syncToVBlank;
            set
            {
                if (_syncToVBlank != value)
                {
                    _newSyncToVBlank = value;
                }
            }
        }

        public override bool IsDisposed => _disposed;

        public VkSwapchainKHR DeviceSwapchain => _deviceSwapchain;
        public uint ImageIndex => _currentImageIndex;
        public VulkanFence ImageAvailableFence => _imageAvailableFence;
        public VkSurfaceKHR Surface => _surface;
        public VkQueue PresentQueue => _presentQueue;
        public uint PresentQueueIndex => _presentQueueIndex;
        public ResourceRefCount RefCount { get; }

        public VkSwapchain(VkGraphicsDevice gd, in SwapchainDescription description) : this(gd, description, default)
        {
        }

        public VkSwapchain(VkGraphicsDevice gd, in SwapchainDescription description, VkSurfaceKHR existingSurface)
        {
            _gd = gd;
            _syncToVBlank = description.SyncToVerticalBlank;
            _swapchainSource = description.Source;
            _colorSrgb = description.ColorSrgb;

            if (existingSurface == VkSurfaceKHR.NULL)
            {
                _surface = VkSurfaceUtil.CreateSurface(gd.Instance, _swapchainSource);
            }
            else
            {
                _surface = existingSurface;
            }

            if (!GetPresentQueueIndex(out _presentQueueIndex))
            {
                throw new VeldridException($"The system does not support presenting the given Vulkan surface.");
            }
            VkQueue presentQueue;
            vkGetDeviceQueue(_gd.Device, _presentQueueIndex, 0, &presentQueue);
            _presentQueue = presentQueue;

            _framebuffer = new VkSwapchainFramebuffer(gd, this, _surface, description.Width, description.Height, description.DepthFormat);

            CreateSwapchain(description.Width, description.Height);

            VkFenceCreateInfo fenceCI = new()
            {
                sType = VkStructureType.VK_STRUCTURE_TYPE_FENCE_CREATE_INFO
            };

            VulkanFence imageAvailableFence;
            vkCreateFence(_gd.Device, &fenceCI, null, &imageAvailableFence);

            AcquireNextImage(_gd.Device, default, imageAvailableFence);
            vkWaitForFences(_gd.Device, 1, &imageAvailableFence, true, ulong.MaxValue);
            vkResetFences(_gd.Device, 1, &imageAvailableFence);

            _imageAvailableFence = imageAvailableFence;

            RefCount = new ResourceRefCount(this);
        }

        public override void Resize(uint width, uint height)
        {
            RecreateAndReacquire(width, height);
        }

        public bool AcquireNextImage(VkDevice device, VkSemaphore semaphore, VulkanFence fence)
        {
            if (_newSyncToVBlank != null)
            {
                _syncToVBlank = _newSyncToVBlank.Value;
                _newSyncToVBlank = null;
                RecreateAndReacquire(_framebuffer.Width, _framebuffer.Height);
                return false;
            }

            uint imageIndex = _currentImageIndex;
            VkResult result = vkAcquireNextImageKHR(
                device,
                _deviceSwapchain,
                ulong.MaxValue,
                semaphore,
                fence,
                &imageIndex);
            _framebuffer.SetImageIndex(imageIndex);
            _currentImageIndex = imageIndex;

            if (result == VkResult.VK_ERROR_OUT_OF_DATE_KHR || result == VkResult.VK_SUBOPTIMAL_KHR)
            {
                CreateSwapchain(_framebuffer.Width, _framebuffer.Height);
                return false;
            }
            else if (result != VkResult.VK_SUCCESS)
            {
                throw new VeldridException("Could not acquire next image from the Vulkan swapchain.");
            }

            return true;
        }

        private void RecreateAndReacquire(uint width, uint height)
        {
            if (CreateSwapchain(width, height))
            {
                VulkanFence imageAvailableFence = _imageAvailableFence;
                if (AcquireNextImage(_gd.Device, default, imageAvailableFence))
                {
                    vkWaitForFences(_gd.Device, 1, &imageAvailableFence, true, ulong.MaxValue);
                    vkResetFences(_gd.Device, 1, &imageAvailableFence);
                }
            }
        }

        private bool CreateSwapchain(uint width, uint height)
        {
            // Obtain the surface capabilities first -- this will indicate whether the surface has been lost.
            VkSurfaceCapabilitiesKHR surfaceCapabilities;
            VkResult result = vkGetPhysicalDeviceSurfaceCapabilitiesKHR(_gd.PhysicalDevice, _surface, &surfaceCapabilities);
            if (result == VkResult.VK_ERROR_SURFACE_LOST_KHR)
            {
                throw new VeldridException($"The Swapchain's underlying surface has been lost.");
            }

            if (surfaceCapabilities.minImageExtent.width == 0 && surfaceCapabilities.minImageExtent.height == 0
                && surfaceCapabilities.maxImageExtent.width == 0 && surfaceCapabilities.maxImageExtent.height == 0)
            {
                return false;
            }

            if (_deviceSwapchain != VkSwapchainKHR.NULL)
            {
                _gd.WaitForIdle();
            }

            _currentImageIndex = 0;
            uint surfaceFormatCount = 0;
            result = vkGetPhysicalDeviceSurfaceFormatsKHR(_gd.PhysicalDevice, _surface, &surfaceFormatCount, null);
            CheckResult(result);
            VkSurfaceFormatKHR[] formats = new VkSurfaceFormatKHR[surfaceFormatCount];
            fixed (VkSurfaceFormatKHR* formatsPtr = formats)
            {
                result = vkGetPhysicalDeviceSurfaceFormatsKHR(_gd.PhysicalDevice, _surface, &surfaceFormatCount, formatsPtr);
                CheckResult(result);
            }

            VkFormat desiredFormat = _colorSrgb
                ? VK_FORMAT_B8G8R8A8_SRGB
                : VK_FORMAT_B8G8R8A8_UNORM;

            VkSurfaceFormatKHR surfaceFormat = new();
            if (formats.Length == 1 && formats[0].format == VK_FORMAT_UNDEFINED)
            {
                surfaceFormat.format = desiredFormat;
                surfaceFormat.colorSpace = VkColorSpaceKHR.VK_COLORSPACE_SRGB_NONLINEAR_KHR;
            }
            else
            {
                foreach (VkSurfaceFormatKHR format in formats)
                {
                    if (format.colorSpace == VkColorSpaceKHR.VK_COLORSPACE_SRGB_NONLINEAR_KHR && format.format == desiredFormat)
                    {
                        surfaceFormat = format;
                        break;
                    }
                }
                if (surfaceFormat.format == VK_FORMAT_UNDEFINED)
                {
                    if (_colorSrgb && surfaceFormat.format != VK_FORMAT_R8G8B8A8_SRGB)
                    {
                        throw new VeldridException($"Unable to create an sRGB Swapchain for this surface.");
                    }

                    surfaceFormat = formats[0];
                }
            }

            uint presentModeCount = 0;
            result = vkGetPhysicalDeviceSurfacePresentModesKHR(_gd.PhysicalDevice, _surface, &presentModeCount, null);
            CheckResult(result);
            VkPresentModeKHR[] presentModes = new VkPresentModeKHR[presentModeCount];
            fixed (VkPresentModeKHR* presentModesPtr = presentModes)
            {
                result = vkGetPhysicalDeviceSurfacePresentModesKHR(_gd.PhysicalDevice, _surface, &presentModeCount, presentModesPtr);
                CheckResult(result);
            }

            VkPresentModeKHR presentMode = VkPresentModeKHR.VK_PRESENT_MODE_FIFO_KHR;

            if (_syncToVBlank)
            {
                if (presentModes.Contains(VkPresentModeKHR.VK_PRESENT_MODE_FIFO_RELAXED_KHR))
                {
                    presentMode = VkPresentModeKHR.VK_PRESENT_MODE_FIFO_RELAXED_KHR;
                }
            }
            else
            {
                if (presentModes.Contains(VkPresentModeKHR.VK_PRESENT_MODE_MAILBOX_KHR))
                {
                    presentMode = VkPresentModeKHR.VK_PRESENT_MODE_MAILBOX_KHR;
                }
                else if (presentModes.Contains(VkPresentModeKHR.VK_PRESENT_MODE_IMMEDIATE_KHR))
                {
                    presentMode = VkPresentModeKHR.VK_PRESENT_MODE_IMMEDIATE_KHR;
                }
            }

            uint maxImageCount = surfaceCapabilities.maxImageCount == 0 ? uint.MaxValue : surfaceCapabilities.maxImageCount;
            uint imageCount = Math.Min(maxImageCount, surfaceCapabilities.minImageCount + 1);

            uint clampedWidth = Util.Clamp(width, surfaceCapabilities.minImageExtent.width, surfaceCapabilities.maxImageExtent.width);
            uint clampedHeight = Util.Clamp(height, surfaceCapabilities.minImageExtent.height, surfaceCapabilities.maxImageExtent.height);
            VkSwapchainCreateInfoKHR swapchainCI = new()
            {
                sType = VkStructureType.VK_STRUCTURE_TYPE_SWAPCHAIN_CREATE_INFO_KHR,
                surface = _surface,
                presentMode = presentMode,
                imageFormat = surfaceFormat.format,
                imageColorSpace = surfaceFormat.colorSpace,
                imageExtent = new VkExtent2D() { width = clampedWidth, height = clampedHeight },
                minImageCount = imageCount,
                imageArrayLayers = 1,
                imageUsage = VkImageUsageFlags.VK_IMAGE_USAGE_COLOR_ATTACHMENT_BIT | VkImageUsageFlags.VK_IMAGE_USAGE_TRANSFER_DST_BIT
            };

            FixedArray2<uint> queueFamilyIndices = new(_gd.GraphicsQueueIndex, _gd.PresentQueueIndex);

            if (_gd.GraphicsQueueIndex != _gd.PresentQueueIndex)
            {
                swapchainCI.imageSharingMode = VkSharingMode.VK_SHARING_MODE_CONCURRENT;
                swapchainCI.queueFamilyIndexCount = 2;
                swapchainCI.pQueueFamilyIndices = &queueFamilyIndices.First;
            }
            else
            {
                swapchainCI.imageSharingMode = VkSharingMode.VK_SHARING_MODE_EXCLUSIVE;
                swapchainCI.queueFamilyIndexCount = 0;
            }

            swapchainCI.preTransform = VkSurfaceTransformFlagsKHR.VK_SURFACE_TRANSFORM_IDENTITY_BIT_KHR;
            swapchainCI.compositeAlpha = VkCompositeAlphaFlagsKHR.VK_COMPOSITE_ALPHA_OPAQUE_BIT_KHR;
            swapchainCI.clipped = true;

            VkSwapchainKHR oldSwapchain = _deviceSwapchain;
            swapchainCI.oldSwapchain = oldSwapchain;

            VkSwapchainKHR deviceSwapchain;
            result = vkCreateSwapchainKHR(_gd.Device, &swapchainCI, null, &deviceSwapchain);
            CheckResult(result);
            _deviceSwapchain = deviceSwapchain;

            if (oldSwapchain != VkSwapchainKHR.NULL)
            {
                vkDestroySwapchainKHR(_gd.Device, oldSwapchain, null);
            }

            _framebuffer.SetNewSwapchain(_deviceSwapchain, width, height, surfaceFormat, swapchainCI.imageExtent);
            return true;
        }

        private bool GetPresentQueueIndex(out uint queueFamilyIndex)
        {
            uint graphicsQueueIndex = _gd.GraphicsQueueIndex;
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
            VkBool32 supported;
            VkResult result = vkGetPhysicalDeviceSurfaceSupportKHR(
                _gd.PhysicalDevice,
                queueFamilyIndex,
                surface,
                &supported);
            CheckResult(result);
            return supported;
        }

        public override void Dispose()
        {
            RefCount.Decrement();
        }

        void IResourceRefCountTarget.RefZeroed()
        {
            if (!_disposed)
            {
                _disposed = true;
                vkDestroyFence(_gd.Device, _imageAvailableFence, null);
                _framebuffer.Dispose();
                vkDestroySwapchainKHR(_gd.Device, _deviceSwapchain, null);
                vkDestroySurfaceKHR(_gd.Instance, _surface, null);
            }
        }
    }
}
