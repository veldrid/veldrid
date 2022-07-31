using System;
using TerraFX.Interop.Vulkan;

namespace Veldrid.Vulkan
{
    /// <summary>
    /// An object which can be used to create a <see cref="VkSurfaceKHR"/>.
    /// </summary>
    public abstract class VkSurfaceSource
    {
        internal VkSurfaceSource()
        {
        }

        /// <summary>
        /// Creates a new <see cref="VkSurfaceKHR"/> attached to this source.
        /// </summary>
        /// <param name="instance">The <see cref="VkInstance"/> to use.</param>
        /// <returns>A new <see cref="VkSurfaceKHR"/>.</returns>
        public abstract VkSurfaceKHR CreateSurface(VkInstance instance);

        /// <summary>
        /// Creates a new <see cref="VkSurfaceSource"/> from the given Win32 instance and window handle.
        /// </summary>
        /// <param name="hinstance">The Win32 instance handle.</param>
        /// <param name="hwnd">The Win32 window handle.</param>
        /// <returns>A new <see cref="VkSurfaceSource"/>.</returns>
        public static VkSurfaceSource CreateWin32(IntPtr hinstance, IntPtr hwnd)
        {
            return new Win32VkSurfaceInfo(hinstance, hwnd);
        }

        /// <summary>
        /// Creates a new <see cref="VkSurfaceSource"/> from the given Xlib information.
        /// </summary>
        /// <param name="display">A pointer to the Xlib display.</param>
        /// <param name="window">An Xlib window.</param>
        /// <returns>A new <see cref="VkSurfaceSource"/>.</returns>
        public static VkSurfaceSource CreateXlib(IntPtr display, IntPtr window)
        {
            return new XlibVkSurfaceInfo(display, window);
        }

        /// <summary>
        /// Creates a new <see cref="VkSurfaceSource"/> from the given Wayland information.
        /// </summary>
        /// <param name="display">A pointer to the Wayland display.</param>
        /// <param name="surface">A Wayland surface.</param>
        /// <returns>A new <see cref="VkSurfaceSource"/>.</returns>
        public static VkSurfaceSource CreateWayland(IntPtr display, IntPtr surface)
        {
            return new WaylandVkSurfaceInfo(display, surface);
        }

        internal abstract SwapchainSource GetSurfaceSource();
    }

    internal sealed class Win32VkSurfaceInfo : VkSurfaceSource
    {
        private readonly IntPtr _hinstance;
        private readonly IntPtr _hwnd;

        public Win32VkSurfaceInfo(IntPtr hinstance, IntPtr hwnd)
        {
            _hinstance = hinstance;
            _hwnd = hwnd;
        }

        public override VkSurfaceKHR CreateSurface(VkInstance instance)
        {
            return VkSurfaceUtil.CreateSurface(instance, GetSurfaceSource());
        }

        internal override SwapchainSource GetSurfaceSource()
        {
            return new Win32SwapchainSource(_hwnd, _hinstance);
        }
    }

    internal sealed class XlibVkSurfaceInfo : VkSurfaceSource
    {
        private readonly IntPtr _display;
        private readonly IntPtr _window;

        public XlibVkSurfaceInfo(IntPtr display, IntPtr window)
        {
            _display = display;
            _window = window;
        }

        public override VkSurfaceKHR CreateSurface(VkInstance instance)
        {
            return VkSurfaceUtil.CreateSurface(instance, GetSurfaceSource());
        }

        internal override SwapchainSource GetSurfaceSource()
        {
            return new XlibSwapchainSource(_display, _window);
        }
    }

    internal sealed class WaylandVkSurfaceInfo : VkSurfaceSource
    {
        private readonly IntPtr _display;
        private readonly IntPtr _surface;

        public WaylandVkSurfaceInfo(IntPtr display, IntPtr surface)
        {
            _display = display;
            _surface = surface;
        }

        public override VkSurfaceKHR CreateSurface(VkInstance instance)
        {
            return VkSurfaceUtil.CreateSurface(instance, GetSurfaceSource());
        }

        internal override SwapchainSource GetSurfaceSource()
        {
            return new WaylandSwapchainSource(_display, _surface);
        }
    }
}
