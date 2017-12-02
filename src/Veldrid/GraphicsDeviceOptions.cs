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
        /// An optional <see cref="PixelFormat"/> to be used for the depth buffer of the swapchain. If this value is null, then
        /// no depth buffer will be present on the swapchain.
        /// </summary>
        public PixelFormat? SwapchainDepthFormat;
        /// <summary>
        /// Indicates whether the GraphicsDevice will be synchronized to the window system's vertical refresh rate.
        /// </summary>
        public bool SyncToVerticalBlank;

        /// <summary>
        /// Constructs a new GraphicsDeviceOptions.
        /// </summary>
        /// <param name="debug">Indicates whether the GraphicsDevice will support debug features, provided they are supported by
        /// the host system.</param>
        /// <param name="swapchainDepthFormat">An optional <see cref="PixelFormat"/> to be used for the depth buffer of the
        /// swapchain. If this value is null, then no depth buffer will be present on the swapchain.</param>
        /// <param name="syncToVerticalBlank">Indicates whether the GraphicsDevice will be synchronized to the window system's
        /// vertical refresh rate.</param>
        public GraphicsDeviceOptions(bool debug, PixelFormat? swapchainDepthFormat, bool syncToVerticalBlank)
        {
            Debug = debug;
            SwapchainDepthFormat = swapchainDepthFormat;
            SyncToVerticalBlank = syncToVerticalBlank;
        }
    }
}
