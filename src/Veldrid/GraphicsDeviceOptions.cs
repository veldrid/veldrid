namespace Veldrid
{
    /// <summary>
    /// A structure describing several common properties of a GraphicsDevice.
    /// </summary>
    public struct GraphicsDeviceOptions
    {
        /// <summary>
        /// Indicates whether the GraphicsDevice will support debug features, provided they are supported by the host system.
        /// </summary>
        public bool Debug;
        /// <summary>
        /// Indicates whether the Graphicsdevice will include a "main" Swapchain. If this value is true, then the GraphicsDevice
        /// must be created with one of the overloads that provides Swapchain source information.
        /// </summary>
        public bool HasMainSwapchain;
        /// <summary>
        /// An optional <see cref="PixelFormat"/> to be used for the depth buffer of the swapchain. If this value is null, then
        /// no depth buffer will be present on the swapchain.
        /// </summary>
        public PixelFormat? SwapchainDepthFormat;
        /// <summary>
        /// Indicates whether the main Swapchain will be synchronized to the window system's vertical refresh rate.
        /// </summary>
        public bool SyncToVerticalBlank;
        /// <summary>
        /// Specifies which model the rendering backend should use for binding resources. This can be overridden per-pipeline
        /// by specifying a value in <see cref="GraphicsPipelineDescription.ResourceBindingModel"/>.
        /// </summary>
        public ResourceBindingModel ResourceBindingModel;

        /// <summary>
        /// Constructs a new GraphicsDeviceOptions for a device with no main Swapchain.
        /// </summary>
        /// <param name="debug">Indicates whether the GraphicsDevice will support debug features, provided they are supported by
        /// the host system.</param>
        public GraphicsDeviceOptions(bool debug)
        {
            Debug = debug;
            HasMainSwapchain = false;
            SwapchainDepthFormat = null;
            SyncToVerticalBlank = false;
            ResourceBindingModel = ResourceBindingModel.Default;
        }

        /// <summary>
        /// Constructs a new GraphicsDeviceOptions for a device with a main Swapchain.
        /// </summary>
        /// <param name="debug">Indicates whether the GraphicsDevice will enable debug features, provided they are supported by
        /// the host system.</param>
        /// <param name="swapchainDepthFormat">An optional <see cref="PixelFormat"/> to be used for the depth buffer of the
        /// swapchain. If this value is null, then no depth buffer will be present on the swapchain.</param>
        /// <param name="syncToVerticalBlank">Indicates whether the main Swapchain will be synchronized to the window system's
        /// vertical refresh rate.</param>
        public GraphicsDeviceOptions(bool debug, PixelFormat? swapchainDepthFormat, bool syncToVerticalBlank)
        {
            Debug = debug;
            HasMainSwapchain = true;
            SwapchainDepthFormat = swapchainDepthFormat;
            SyncToVerticalBlank = syncToVerticalBlank;
            ResourceBindingModel = ResourceBindingModel.Default;
        }
    }
}
