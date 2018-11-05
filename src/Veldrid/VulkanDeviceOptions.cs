namespace Veldrid
{
    public struct VulkanDeviceOptions
    {
        public string[] InstanceExtensions;
        public string[] DeviceExtensions;

        public VulkanDeviceOptions(string[] instanceExtensions, string[] deviceExtensions)
        {
            InstanceExtensions = instanceExtensions;
            DeviceExtensions = deviceExtensions;
        }
    }
}
