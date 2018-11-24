namespace Veldrid
{
    /// <summary>
    /// A structure describing Vulkan-specific device creation options.
    /// </summary>
    public struct VulkanDeviceOptions
    {
        /// <summary>
        /// An array of required Vulkan instance extensions. Entries in this array will be enabled in the GraphicsDevice's
        /// created VkInstance.
        /// </summary>
        public string[] InstanceExtensions;
        /// <summary>
        /// An array of required Vulkan device extensions. Entries in this array will be enabled in the GraphicsDevice's
        /// created VkDevice.
        /// </summary>
        public string[] DeviceExtensions;

        /// <summary>
        /// Constructs a new VulkanDeviceOptions.
        /// </summary>
        /// <param name="instanceExtensions">An array of required Vulkan instance extensions. Entries in this array will be
        /// enabled in the GraphicsDevice's created VkInstance.</param>
        /// <param name="deviceExtensions">An array of required Vulkan device extensions. Entries in this array will be enabled
        /// in the GraphicsDevice's created VkDevice.</param>
        public VulkanDeviceOptions(string[] instanceExtensions, string[] deviceExtensions)
        {
            InstanceExtensions = instanceExtensions;
            DeviceExtensions = deviceExtensions;
        }
    }
}
