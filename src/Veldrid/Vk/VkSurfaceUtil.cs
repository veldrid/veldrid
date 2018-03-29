using Vulkan;
using Vulkan.Xlib;
using static Vulkan.VulkanNative;
using static Veldrid.Vk.VulkanUtil;

namespace Veldrid.Vk
{
    internal static unsafe class VkSurfaceUtil
    {
        internal static VkSurfaceKHR CreateSurface(VkInstance instance, SwapchainSource swapchainSource)
        {
            switch (swapchainSource)
            {
                case XlibSwapchainSource xlibSource:
                    return CreateXlib(instance, xlibSource);
                case Win32SwapchainSource win32Source:
                    return CreateWin32(instance, win32Source);
                case ANativeWindowSwapchainSource anwSource:
                    return CreateANativeWindow(instance, anwSource);
                default:
                    throw new VeldridException($"The provided SwapchainSource cannot be used to create a Vulkan surface.");
            }
        }

        private static VkSurfaceKHR CreateWin32(VkInstance instance, Win32SwapchainSource win32Source)
        {
            VkWin32SurfaceCreateInfoKHR surfaceCI = VkWin32SurfaceCreateInfoKHR.New();
            surfaceCI.hwnd = win32Source.Hwnd;
            surfaceCI.hinstance = win32Source.Hinstance;
            VkResult result = vkCreateWin32SurfaceKHR(instance, ref surfaceCI, null, out VkSurfaceKHR surface);
            CheckResult(result);
            return surface;
        }

        private static VkSurfaceKHR CreateXlib(VkInstance instance, XlibSwapchainSource xlibSource)
        {
            VkXlibSurfaceCreateInfoKHR xsci = VkXlibSurfaceCreateInfoKHR.New();
            xsci.dpy = (Display*)xlibSource.Display;
            xsci.window = new Window { Value = xlibSource.Window };
            VkResult result = vkCreateXlibSurfaceKHR(instance, ref xsci, null, out VkSurfaceKHR surface);
            CheckResult(result);
            return surface;
        }

        private static VkSurfaceKHR CreateANativeWindow(VkInstance instance, ANativeWindowSwapchainSource anwSource)
        {
            VkAndroidSurfaceCreateInfoKHR androidSurfaceCI = VkAndroidSurfaceCreateInfoKHR.New();
            androidSurfaceCI.window = (Vulkan.Android.ANativeWindow*)anwSource.ANativeWindow;
            VkResult result = vkCreateAndroidSurfaceKHR(instance, ref androidSurfaceCI, null, out VkSurfaceKHR surface);
            CheckResult(result);
            return surface;
        }
    }
}
