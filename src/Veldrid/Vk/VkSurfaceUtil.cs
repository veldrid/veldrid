using Vulkan;
using Vulkan.Xlib;
using Vulkan.Wayland;
using static Vulkan.VulkanNative;
using static Veldrid.Vk.VulkanUtil;
using Veldrid.Android;
using System;
using Veldrid.MetalBindings;

namespace Veldrid.Vk
{
    internal static unsafe class VkSurfaceUtil
    {
        internal static VkSurfaceKHR CreateSurface(VkGraphicsDevice gd, VkInstance instance, SwapchainSource swapchainSource)
        {
            // TODO a null GD is passed from VkSurfaceSource.CreateSurface for compatibility
            //      when VkSurfaceInfo is removed we do not have to handle gd == null anymore
            var doCheck = gd != null;

            if (doCheck && !gd.HasSurfaceExtension(CommonStrings.VK_KHR_SURFACE_EXTENSION_NAME))
                throw new VeldridException($"The required instance extension was not available: {CommonStrings.VK_KHR_SURFACE_EXTENSION_NAME}");

            switch (swapchainSource)
            {
                case XlibSwapchainSource xlibSource:
                    if (doCheck && !gd.HasSurfaceExtension(CommonStrings.VK_KHR_XLIB_SURFACE_EXTENSION_NAME))
                    {
                        throw new VeldridException($"The required instance extension was not available: {CommonStrings.VK_KHR_XLIB_SURFACE_EXTENSION_NAME}");
                    }
                    return CreateXlib(instance, xlibSource);
                case WaylandSwapchainSource waylandSource:
                    if (doCheck && !gd.HasSurfaceExtension(CommonStrings.VK_KHR_WAYLAND_SURFACE_EXTENSION_NAME))
                    {
                        throw new VeldridException($"The required instance extension was not available: {CommonStrings.VK_KHR_WAYLAND_SURFACE_EXTENSION_NAME}");
                    }
                    return CreateWayland(instance, waylandSource);
                case Win32SwapchainSource win32Source:
                    if (doCheck && !gd.HasSurfaceExtension(CommonStrings.VK_KHR_WIN32_SURFACE_EXTENSION_NAME))
                    {
                        throw new VeldridException($"The required instance extension was not available: {CommonStrings.VK_KHR_WIN32_SURFACE_EXTENSION_NAME}");
                    }
                    return CreateWin32(instance, win32Source);
                case AndroidSurfaceSwapchainSource androidSource:
                    if (doCheck && !gd.HasSurfaceExtension(CommonStrings.VK_KHR_ANDROID_SURFACE_EXTENSION_NAME))
                    {
                        throw new VeldridException($"The required instance extension was not available: {CommonStrings.VK_KHR_ANDROID_SURFACE_EXTENSION_NAME}");
                    }
                    return CreateAndroidSurface(instance, androidSource);
                case NSWindowSwapchainSource nsWindowSource:
                    if (doCheck && !gd.HasSurfaceExtension(CommonStrings.VK_MVK_MACOS_SURFACE_EXTENSION_NAME))
                    {
                        throw new VeldridException($"The required instance extension was not available: {CommonStrings.VK_MVK_MACOS_SURFACE_EXTENSION_NAME}");
                    }
                    return CreateNSWindowSurface(instance, nsWindowSource);
                case UIViewSwapchainSource uiViewSource:
                    if (doCheck && !gd.HasSurfaceExtension(CommonStrings.VK_MVK_IOS_SURFACE_EXTENSION_NAME))
                    {
                        throw new VeldridException($"The required instance extension was not available: {CommonStrings.VK_MVK_IOS_SURFACE_EXTENSION_NAME}");
                    }
                    return CreateUIViewSurface(instance, uiViewSource);
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

        private static VkSurfaceKHR CreateWayland(VkInstance instance, WaylandSwapchainSource waylandSource)
        {
            VkWaylandSurfaceCreateInfoKHR wsci = VkWaylandSurfaceCreateInfoKHR.New();
            wsci.display = (wl_display*) waylandSource.Display;
            wsci.surface = (wl_surface*) waylandSource.Surface;
            VkResult result = vkCreateWaylandSurfaceKHR(instance, ref wsci, null, out VkSurfaceKHR surface);
            CheckResult(result);
            return surface;
        }

        private static VkSurfaceKHR CreateAndroidSurface(VkInstance instance, AndroidSurfaceSwapchainSource androidSource)
        {
            IntPtr aNativeWindow = AndroidRuntime.ANativeWindow_fromSurface(androidSource.JniEnv, androidSource.Surface);

            VkAndroidSurfaceCreateInfoKHR androidSurfaceCI = VkAndroidSurfaceCreateInfoKHR.New();
            androidSurfaceCI.window = (Vulkan.Android.ANativeWindow*)aNativeWindow;
            VkResult result = vkCreateAndroidSurfaceKHR(instance, ref androidSurfaceCI, null, out VkSurfaceKHR surface);
            CheckResult(result);
            return surface;
        }

        private static VkSurfaceKHR CreateNSWindowSurface(VkInstance instance, NSWindowSwapchainSource nsWindowSource)
        {
            CAMetalLayer metalLayer = CAMetalLayer.New();
            NSWindow nswindow = new NSWindow(nsWindowSource.NSWindow);
            NSView contentView = nswindow.contentView;
            contentView.wantsLayer = true;
            contentView.layer = metalLayer.NativePtr;

            VkMacOSSurfaceCreateInfoMVK surfaceCI = VkMacOSSurfaceCreateInfoMVK.New();
            surfaceCI.pView = contentView.NativePtr.ToPointer();
            VkResult result = vkCreateMacOSSurfaceMVK(instance, ref surfaceCI, null, out VkSurfaceKHR surface);
            CheckResult(result);
            return surface;
        }

        private static VkSurfaceKHR CreateUIViewSurface(VkInstance instance, UIViewSwapchainSource uiViewSource)
        {
            CAMetalLayer metalLayer = CAMetalLayer.New();
            UIView uiView = new UIView(uiViewSource.UIView);
            metalLayer.frame = uiView.frame;
            metalLayer.opaque = true;
            uiView.layer.addSublayer(metalLayer.NativePtr);

            VkIOSSurfaceCreateInfoMVK surfaceCI = VkIOSSurfaceCreateInfoMVK.New();
            surfaceCI.pView = uiView.NativePtr.ToPointer();
            VkResult result = vkCreateIOSSurfaceMVK(instance, ref surfaceCI, null, out VkSurfaceKHR surface);
            return surface;
        }
    }
}
