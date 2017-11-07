using System;

namespace Veldrid
{
    /// <summary>
    /// Represents an abstract graphics device, capable of creating device resources and executing commands.
    /// </summary>
    public abstract class GraphicsDevice : IDisposable
    {
        internal GraphicsDevice() { }

        /// <summary>
        /// Gets a value identifying the specific graphics API used by this instance.
        /// </summary>
        public abstract GraphicsBackend BackendType { get; }

        /// <summary>
        /// Gets the <see cref="ResourceFactory"/> controlled by this instance.
        /// </summary>
        public abstract ResourceFactory ResourceFactory { get; }

        /// <summary>
        /// Submits the given <see cref="CommandList"/> for execution by this device.
        /// Commands submitted in this way may not be completed when this method returns.
        /// Use <see cref="WaitForIdle"/> to wait for all submitted commands to complete.
        /// <see cref="CommandList.End"/> must have been called on <paramref name="commandList"/> for this method to succeed.
        /// </summary>
        /// <param name="commandList">The completed <see cref="CommandList"/> to execute. <see cref="CommandList.End"/> must have
        /// been previously called on this object.</param>
        public abstract void ExecuteCommands(CommandList commandList);

        /// <summary>
        /// Swaps the buffers of the swapchain and presents the rendered image to the screen.
        /// </summary>
        public abstract void SwapBuffers();

        /// <summary>
        /// Gets a <see cref="Framebuffer"/> object representing the render targets of the main swapchain.
        /// </summary>
        public abstract Framebuffer SwapchainFramebuffer { get; }

        /// <summary>
        /// Notifies this instance that the main window has been resized. This causes the <see cref="SwapchainFramebuffer"/> to
        /// be appropriately resized and recreated.
        /// </summary>
        /// <param name="width">The new width of the main window.</param>
        /// <param name="height">The new height of the main window.</param>
        public abstract void ResizeMainWindow(uint width, uint height);

        /// <summary>
        /// A blocking method that returns when all submitted <see cref="CommandList"/> objects have fully completed.
        /// </summary>
        public abstract void WaitForIdle();

        /// <summary>
        /// Associates an arbitrary name with a resource. On some systems, this functionality requires special device creation
        /// options. Can be used to overwrite or remove a previously-assigned name.
        /// </summary>
        /// <param name="resource">The target resource.</param>
        /// <param name="name">The name.</param>
        public abstract void SetResourceName(DeviceResource resource, string name);

        /// <summary>
        /// Performs API-specific disposal of resources controlled by this instance.
        /// </summary>
        protected abstract void PlatformDispose();

        /// <summary>
        /// Creates and caches common device resources after device creation completes.
        /// </summary>
        protected void PostDeviceCreated()
        {
            PointSampler = ResourceFactory.CreateSampler(SamplerDescription.Point);
            LinearSampler = ResourceFactory.CreateSampler(SamplerDescription.Linear);
            Aniso4xSampler = ResourceFactory.CreateSampler(SamplerDescription.Aniso4x);
        }

        /// <summary>
        /// Gets a simple point-filtered <see cref="Sampler"/> object owned by this instance.
        /// This object is created with <see cref="SamplerDescription.Point"/>.
        /// </summary>
        public Sampler PointSampler { get; private set; }

        /// <summary>
        /// Gets a simple linear-filtered <see cref="Sampler"/> object owned by this instance.
        /// This object is created with <see cref="SamplerDescription.Linear"/>.
        /// </summary>
        public Sampler LinearSampler { get; private set; }

        /// <summary>
        /// Gets a simple 4x anisotropic-filtered <see cref="Sampler"/> object owned by this instance.
        /// This object is created with <see cref="SamplerDescription.Aniso4x"/>.
        /// </summary>
        public Sampler Aniso4xSampler { get; private set; }

        /// <summary>
        /// Frees unmanaged resources controlled by this device.
        /// All created child resources must be Disposed prior to calling this method.
        /// </summary>
        public void Dispose()
        {
            PointSampler.Dispose();
            LinearSampler.Dispose();
            Aniso4xSampler.Dispose();
            PlatformDispose();
        }

        /// <summary>
        /// Creates a new <see cref="GraphicsDevice"/> using Direct3D 11.
        /// </summary>
        /// <param name="hwnd">The Win32 window handle to render into.</param>
        /// <param name="width">The initial width of the window.</param>
        /// <param name="height">The initial height of the window.</param>
        /// <returns>A new <see cref="GraphicsDevice"/> using the Direct3D 11 API.</returns>
        public static GraphicsDevice CreateD3D11(IntPtr hwnd, uint width, uint height)
        {
            return new D3D11.D3D11GraphicsDevice(hwnd, (int)width, (int)height);
        }

        /// <summary>
        /// Creates a new <see cref="GraphicsDevice"/> using Vulkan.
        /// </summary>
        /// <param name="surfaceSource">The source from which a Vulkan surface can be created.</param>
        /// <param name="width">The initial width of the window.</param>
        /// <param name="height">The initial height of the window.</param>
        /// <param name="debugDevice">Indicates whether the Vulkan device will support debug features, provided they are
        /// supported by the host system.</param>
        /// <returns>A new <see cref="GraphicsDevice"/> using the Vulkan API.</returns>
        public static GraphicsDevice CreateVulkan(Vk.VkSurfaceSource surfaceSource, uint width, uint height, bool debugDevice)
        {
            return new Vk.VkGraphicsDevice(surfaceSource, width, height, debugDevice);
        }

        /// <summary>
        /// Creates a new <see cref="GraphicsDevice"/> using OpenGL.
        /// </summary>
        /// <param name="platformInfo">An <see cref="OpenGL.OpenGLPlatformInfo"/> object encapsulating necessary OpenGL context
        /// information.</param>
        /// <param name="width">The initial width of the window.</param>
        /// <param name="height">The initial height of the window.</param>
        /// <param name="debugDevice">Indicates whether the OpenGL device will support debug features, provided they are
        /// supported by the host system.</param>
        /// <returns>A new <see cref="GraphicsDevice"/> using the OpenGL API.</returns>
        public static GraphicsDevice CreateOpenGL(
            OpenGL.OpenGLPlatformInfo platformInfo,
            uint width,
            uint height,
            bool debugDevice)
        {
            return new OpenGL.OpenGLGraphicsDevice(platformInfo, width, height, debugDevice);
        }
    }
}
