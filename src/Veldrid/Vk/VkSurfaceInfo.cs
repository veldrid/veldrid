using System;
using Vulkan;
using Vulkan.Xlib;
using static Veldrid.Vk.VulkanUtil;
using static Vulkan.VulkanNative;

namespace Veldrid.Vk
{
    /// <summary>
    /// An object which can be used to create a VkSurfaceKHR.
    /// </summary>
    public abstract class VkSurfaceSource
    {
        internal VkSurfaceSource() { }

        /// <summary>
        /// Creates a new VkSurfaceKHR attached to this source.
        /// </summary>
        /// <param name="instance">The VkInstance to use.</param>
        /// <returns>A new VkSurfaceKHR.</returns>
        public abstract VkSurfaceKHR CreateSurface(VkInstance instance);

        /// <summary>
        /// Creates a new <see cref="VkSurfaceSource"/> from the given Win32 instance and window handle.
        /// </summary>
        /// <param name="hinstance">The Win32 instance handle.</param>
        /// <param name="hwnd">The Win32 window handle.</param>
        /// <returns>A new VkSurfaceSource.</returns>
        public static VkSurfaceSource CreateWin32(IntPtr hinstance, IntPtr hwnd) => new Win32VkSurfaceInfo(hinstance, hwnd);
        /// <summary>
        /// Creates a new VkSurfaceSource from the given Xlib information.
        /// </summary>
        /// <param name="display">A pointer to the Xlib Display.</param>
        /// <param name="window">An Xlib window.</param>
        /// <returns>A new VkSurfaceSource.</returns>
        public unsafe static VkSurfaceSource CreateXlib(Display* display, Window window) => new XlibVkSurfaceInfo(display, window);

        internal abstract SwapchainSource GetSurfaceSource();

        internal static unsafe VkSurfaceSource CreateFromSwapchainSource(SwapchainSource source)
        {
            if (source is Win32SwapchainSource win32Source)
            {
                return new Win32VkSurfaceInfo(win32Source.Hinstance, win32Source.Hwnd);
            }
            else if (source is XlibSwapchainSource xlibSource)
            {
                return new XlibVkSurfaceInfo((Display*)xlibSource.Display, new Window { Value = xlibSource.Window });
            }
            else
            {
                throw new VeldridException("Unsupported Vulkan SwapchainSource.");
            }
        }
    }

    internal class Win32VkSurfaceInfo : VkSurfaceSource
    {
        private readonly IntPtr _hinstance;
        private readonly IntPtr _hwnd;

        public Win32VkSurfaceInfo(IntPtr hinstance, IntPtr hwnd)
        {
            _hinstance = hinstance;
            _hwnd = hwnd;
        }

        public unsafe override VkSurfaceKHR CreateSurface(VkInstance instance)
        {
            VkWin32SurfaceCreateInfoKHR surfaceCI = VkWin32SurfaceCreateInfoKHR.New();
            surfaceCI.hwnd = _hwnd;
            surfaceCI.hinstance = _hinstance;
            VkResult result = vkCreateWin32SurfaceKHR(instance, ref surfaceCI, null, out VkSurfaceKHR surface);
            CheckResult(result);
            return surface;
        }

        internal override SwapchainSource GetSurfaceSource()
        {
            return new Win32SwapchainSource(_hwnd, _hinstance);
        }
    }

    internal class XlibVkSurfaceInfo : VkSurfaceSource
    {
        private readonly unsafe Display* _display;
        private readonly Window _window;

        public unsafe XlibVkSurfaceInfo(Display* display, Window window)
        {
            _display = display;
            _window = window;
        }

        public unsafe override VkSurfaceKHR CreateSurface(VkInstance instance)
        {
            VkXlibSurfaceCreateInfoKHR xsci = VkXlibSurfaceCreateInfoKHR.New();
            xsci.dpy = _display;
            xsci.window = _window;
            VkResult result = vkCreateXlibSurfaceKHR(instance, ref xsci, null, out VkSurfaceKHR surface);
            CheckResult(result);
            return surface;
        }

        internal unsafe override SwapchainSource GetSurfaceSource()
        {
            return new XlibSwapchainSource((IntPtr)_display, _window.Value);
        }
    }
}
