using Vulkan;
using Vulkan.Xlib;
using static Vulkan.VulkanNative;
using static Veldrid.Vk.VulkanUtil;
using Veldrid.Android;
using System;
using Veldrid.MetalBindings;

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
                case AndroidSurfaceSwapchainSource androidSource:
                    return CreateAndroidSurface(instance, androidSource);
                case NSWindowSwapchainSource nsWindowSource:
                    return CreateNSWindowSurface(instance, nsWindowSource);
                case UIViewSwapchainSource uiViewSource:
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
