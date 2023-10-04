#if !EXCLUDE_VULKAN_BACKEND
using System;
using System.Collections.ObjectModel;
using TerraFX.Interop.Vulkan;
using Veldrid.Vulkan;

namespace Veldrid
{
    /// <summary>
    /// Exposes Vulkan-specific functionality,
    /// useful for interoperating with native components which interface directly with Vulkan.
    /// Can only be used on <see cref="GraphicsBackend.Vulkan"/>.
    /// </summary>
    public unsafe class BackendInfoVulkan
    {
        private readonly VkGraphicsDevice _gd;
        private readonly ReadOnlyCollection<string> _instanceLayers;
        private readonly ReadOnlyCollection<string> _instanceExtensions;
        private readonly Lazy<ReadOnlyCollection<ExtensionProperties>> _deviceExtensions;

        internal BackendInfoVulkan(VkGraphicsDevice gd)
        {
            _gd = gd;
            _instanceLayers = new ReadOnlyCollection<string>(VulkanUtil.EnumerateInstanceLayers());
            _instanceExtensions = new ReadOnlyCollection<string>(VulkanUtil.EnumerateInstanceExtensions());
            _deviceExtensions = new Lazy<ReadOnlyCollection<ExtensionProperties>>(EnumerateDeviceExtensions);
        }

        /// <summary>
        /// Gets the underlying VkInstance used by the GraphicsDevice.
        /// </summary>
        public IntPtr Instance => (IntPtr)_gd.Instance.Value;

        /// <summary>
        /// Gets the underlying VkDevice used by the GraphicsDevice.
        /// </summary>
        public IntPtr Device => (IntPtr)_gd.Device.Value;

        /// <summary>
        /// Gets the underlying VkPhysicalDevice used by the GraphicsDevice.
        /// </summary>
        public IntPtr PhysicalDevice => (IntPtr)_gd.PhysicalDevice.Value;

        /// <summary>
        /// Gets the VkQueue which is used by the GraphicsDevice to submit graphics work.
        /// </summary>
        public IntPtr GraphicsQueue => (IntPtr)_gd.GraphicsQueue.Value;

        /// <summary>
        /// Gets the queue family index of the graphics VkQueue.
        /// </summary>
        public uint GraphicsQueueFamilyIndex => _gd.GraphicsQueueIndex;

        /// <summary>
        /// Gets the driver name of the device. May be null.
        /// </summary>
        public string? DriverName => _gd.DriverName;

        /// <summary>
        /// Gets the driver information of the device. May be null.
        /// </summary>
        public string? DriverInfo => _gd.DriverInfo;

        public ReadOnlyCollection<string> AvailableInstanceLayers => _instanceLayers;

        public ReadOnlyCollection<string> AvailableInstanceExtensions => _instanceExtensions;

        public ReadOnlyCollection<ExtensionProperties> AvailableDeviceExtensions => _deviceExtensions.Value;

        /// <summary>
        /// Overrides the current VkImageLayout tracked by the given Texture. This should be used when a VkImage is created by
        /// an external library to inform Veldrid about its initial layout.
        /// </summary>
        /// <param name="texture">The Texture whose currently-tracked VkImageLayout will be overridden.</param>
        /// <param name="layout">The new VkImageLayout value.</param>
        public void OverrideImageLayout(Texture texture, uint layout)
        {
            VkTexture vkTex = Util.AssertSubtype<Texture, VkTexture>(texture);
            for (uint layer = 0; layer < vkTex.ArrayLayers; layer++)
            {
                for (uint level = 0; level < vkTex.MipLevels; level++)
                {
                    vkTex.SetImageLayout(level, layer, (VkImageLayout)layout);
                }
            }
        }

        /// <summary>
        /// Gets the underlying VkImage wrapped by the given Veldrid Texture. This method can not be used on Textures with
        /// TextureUsage.Staging.
        /// </summary>
        /// <param name="texture">The Texture whose underlying VkImage will be returned.</param>
        /// <returns>The underlying VkImage for the given Texture.</returns>
        public ulong GetVkImage(Texture texture)
        {
            VkTexture vkTexture = Util.AssertSubtype<Texture, VkTexture>(texture);
            if ((vkTexture.Usage & TextureUsage.Staging) != 0)
            {
                throw new VeldridException(
                    $"{nameof(GetVkImage)} cannot be used if the {nameof(Texture)} " +
                    $"has {nameof(TextureUsage)}.{nameof(TextureUsage.Staging)}.");
            }

            return vkTexture.OptimalDeviceImage.Value;
        }

        /// <summary>
        /// Transitions the given Texture's underlying VkImage into a new layout.
        /// </summary>
        /// <param name="texture">The Texture whose underlying VkImage will be transitioned.</param>
        /// <param name="layout">The new VkImageLayout value.</param>
        public void TransitionImageLayout(Texture texture, uint layout)
        {
            _gd.TransitionImageLayout(Util.AssertSubtype<Texture, VkTexture>(texture), (VkImageLayout)layout);
        }

        private unsafe ReadOnlyCollection<ExtensionProperties> EnumerateDeviceExtensions()
        {
            VkExtensionProperties[] vkProps = _gd.GetDeviceExtensionProperties();
            ExtensionProperties[] veldridProps = new ExtensionProperties[vkProps.Length];

            for (int i = 0; i < vkProps.Length; i++)
            {
                VkExtensionProperties prop = vkProps[i];
                veldridProps[i] = new ExtensionProperties(Util.GetString(prop.extensionName), prop.specVersion);
            }

            return new ReadOnlyCollection<ExtensionProperties>(veldridProps);
        }

        public readonly struct ExtensionProperties
        {
            public readonly string Name;
            public readonly uint SpecVersion;

            public ExtensionProperties(string name, uint specVersion)
            {
                Name = name ?? throw new ArgumentNullException(nameof(name));
                SpecVersion = specVersion;
            }

            public override string ToString()
            {
                return Name;
            }
        }
    }
}
#endif
