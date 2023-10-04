using System;
using System.Collections.Generic;
using TerraFX.Interop.Vulkan;
using Veldrid.Android;
using Veldrid.MetalBindings;
using static Veldrid.Vulkan.VulkanUtil;

namespace Veldrid.Vulkan
{
    internal static unsafe class VkSurfaceUtil
    {
        public static string KHR_SURFACE_EXTENSION_NAME { get; } = "VK_KHR_surface";
        public static string KHR_WIN32_SURFACE_EXTENSION_NAME { get; } = "VK_KHR_win32_surface";
        public static string KHR_XCB_SURFACE_EXTENSION_NAME { get; } = "VK_KHR_xcb_surface";
        public static string KHR_XLIB_SURFACE_EXTENSION_NAME { get; } = "VK_KHR_xlib_surface";
        public static string KHR_WAYLAND_SURFACE_EXTENSION_NAME { get; } = "VK_KHR_wayland_surface";
        public static string KHR_ANDROID_SURFACE_EXTENSION_NAME { get; } = "VK_KHR_android_surface";
        public static string MVK_MACOS_SURFACE_EXTENSION_NAME { get; } = "VK_MVK_macos_surface";
        public static string MVK_IOS_SURFACE_EXTENSION_NAME { get; } = "VK_MVK_ios_surface";
        public static string EXT_METAL_SURFACE_EXTENSION_NAME { get; } = "VK_EXT_metal_surface";

        internal static VkSurfaceKHR CreateSurface(VkInstance instance, SwapchainSource swapchainSource)
        {
            HashSet<string> instanceExtensions = new(EnumerateInstanceExtensions());

            void ThrowIfMissing(string name)
            {
                if (!instanceExtensions.Contains(name))
                {
                    throw new VeldridException($"The required instance extension was not available: {name}");
                }
            }

            ThrowIfMissing(KHR_SURFACE_EXTENSION_NAME);

            switch (swapchainSource)
            {
                case XlibSwapchainSource xlibSource:
                    ThrowIfMissing(KHR_XLIB_SURFACE_EXTENSION_NAME);
                    return CreateXlib(GetInstanceProcAddr(instance, "vkCreateXlibSurfaceKHR"), instance, xlibSource);

                case WaylandSwapchainSource waylandSource:
                    ThrowIfMissing(KHR_WAYLAND_SURFACE_EXTENSION_NAME);
                    return CreateWayland(GetInstanceProcAddr(instance, "vkCreateWaylandSurfaceKHR"), instance, waylandSource);

                case Win32SwapchainSource win32Source:
                    ThrowIfMissing(KHR_WIN32_SURFACE_EXTENSION_NAME);
                    return CreateWin32(GetInstanceProcAddr(instance, "vkCreateWin32SurfaceKHR"), instance, win32Source);

                case AndroidSurfaceSwapchainSource androidSource:
                    ThrowIfMissing(KHR_ANDROID_SURFACE_EXTENSION_NAME);
                    return CreateAndroidSurface(GetInstanceProcAddr(instance, "vkCreateAndroidSurfaceKHR"), instance, androidSource);

                case NSWindowSwapchainSource nsWindowSource:
                    if (instanceExtensions.Contains(EXT_METAL_SURFACE_EXTENSION_NAME))
                    {
                        return CreateNSWindowSurfaceExt(GetInstanceProcAddr(instance, "vkCreateMetalSurfaceEXT"), instance, nsWindowSource);
                    }
                    if (instanceExtensions.Contains(MVK_MACOS_SURFACE_EXTENSION_NAME))
                    {
                        return CreateNSWindowSurfaceMvk(GetInstanceProcAddr(instance, "vkCreateMacOSSurfaceMVK"), instance, nsWindowSource);
                    }
                    throw new VeldridException($"Neither macOS surface extension was available: " +
                        $"{EXT_METAL_SURFACE_EXTENSION_NAME}, {MVK_MACOS_SURFACE_EXTENSION_NAME}");

                case NSViewSwapchainSource nsViewSource:
                    if (instanceExtensions.Contains(EXT_METAL_SURFACE_EXTENSION_NAME))
                    {
                        return CreateNSViewSurfaceExt(GetInstanceProcAddr(instance, "vkCreateMetalSurfaceEXT"), instance, nsViewSource);
                    }
                    if (instanceExtensions.Contains(MVK_MACOS_SURFACE_EXTENSION_NAME))
                    {
                        return CreateNSViewSurfaceMvk(GetInstanceProcAddr(instance, "vkCreateMacOSSurfaceMVK"), instance, nsViewSource);
                    }
                    throw new VeldridException($"Neither macOS surface extension was available: " +
                        $"{EXT_METAL_SURFACE_EXTENSION_NAME}, {MVK_MACOS_SURFACE_EXTENSION_NAME}");

                case UIViewSwapchainSource uiViewSource:
                    if (instanceExtensions.Contains(EXT_METAL_SURFACE_EXTENSION_NAME))
                    {
                        return CreateUIViewSurfaceExt(GetInstanceProcAddr(instance, "vkCreateMetalSurfaceEXT"), instance, uiViewSource);
                    }
                    if (instanceExtensions.Contains(MVK_IOS_SURFACE_EXTENSION_NAME))
                    {
                        return CreateUIViewSurfaceMvk(GetInstanceProcAddr(instance, "vkCreateIOSSurfaceMVK"), instance, uiViewSource);
                    }
                    throw new VeldridException($"Neither macOS surface extension was available: " +
                        $"{EXT_METAL_SURFACE_EXTENSION_NAME}, {MVK_IOS_SURFACE_EXTENSION_NAME}");

                default:
                    throw new VeldridException($"The provided SwapchainSource cannot be used to create a Vulkan surface.");
            }
        }

        private static VkSurfaceKHR CreateWin32(
            IntPtr khr, VkInstance instance, Win32SwapchainSource win32Source)
        {
            VkWin32SurfaceCreateInfoKHR surfaceCI = new()
            {
                sType = VkStructureType.VK_STRUCTURE_TYPE_WIN32_SURFACE_CREATE_INFO_KHR,
                hwnd = win32Source.Hwnd,
                hinstance = win32Source.Hinstance
            };
            VkSurfaceKHR surface;
            VkResult result = ((delegate* unmanaged<VkInstance, VkWin32SurfaceCreateInfoKHR*, VkAllocationCallbacks*, VkSurfaceKHR*, VkResult>)khr)(
                instance, &surfaceCI, null, &surface);
            CheckResult(result);
            return surface;
        }

        private static VkSurfaceKHR CreateXlib(
            IntPtr khr, VkInstance instance, XlibSwapchainSource xlibSource)
        {
            VkXlibSurfaceCreateInfoKHR xsci = new()
            {
                sType = VkStructureType.VK_STRUCTURE_TYPE_XLIB_SURFACE_CREATE_INFO_KHR,
                dpy = xlibSource.Display,
                window = (nuint)xlibSource.Window.ToPointer()
            };
            VkSurfaceKHR surface;
            VkResult result = ((delegate* unmanaged<VkInstance, VkXlibSurfaceCreateInfoKHR*, VkAllocationCallbacks*, VkSurfaceKHR*, VkResult>)khr)(
                instance, &xsci, null, &surface);
            CheckResult(result);
            return surface;
        }

        private static VkSurfaceKHR CreateWayland(
            IntPtr khr, VkInstance instance, WaylandSwapchainSource waylandSource)
        {
            VkWaylandSurfaceCreateInfoKHR wsci = new()
            {
                sType = VkStructureType.VK_STRUCTURE_TYPE_WAYLAND_SURFACE_CREATE_INFO_KHR,
                display = waylandSource.Display,
                surface = waylandSource.Surface
            };
            VkSurfaceKHR surface;
            VkResult result = ((delegate* unmanaged<VkInstance, VkWaylandSurfaceCreateInfoKHR*, VkAllocationCallbacks*, VkSurfaceKHR*, VkResult>)khr)(
                instance, &wsci, null, &surface);
            CheckResult(result);
            return surface;
        }

        private static VkSurfaceKHR CreateAndroidSurface(
            IntPtr khr, VkInstance instance, AndroidSurfaceSwapchainSource androidSource)
        {
            IntPtr aNativeWindow = AndroidRuntime.ANativeWindow_fromSurface(androidSource.JniEnv, androidSource.Surface);
            VkAndroidSurfaceCreateInfoKHR androidSurfaceCI = new()
            {
                sType = VkStructureType.VK_STRUCTURE_TYPE_ANDROID_SURFACE_CREATE_INFO_KHR,
                window = aNativeWindow
            };
            VkSurfaceKHR surface;
            VkResult result = ((delegate* unmanaged<VkInstance, VkAndroidSurfaceCreateInfoKHR*, VkAllocationCallbacks*, VkSurfaceKHR*, VkResult>)khr)(
                instance, &androidSurfaceCI, null, &surface);
            CheckResult(result);
            return surface;
        }

        private static unsafe VkSurfaceKHR CreateNSWindowSurfaceExt(IntPtr ext, VkInstance instance, NSWindowSwapchainSource nsWindowSource)
        {
            NSWindow nswindow = new(nsWindowSource.NSWindow);
            return CreateNSViewSurfaceExt(ext, instance, new NSViewSwapchainSource(nswindow.contentView.NativePtr));
        }

        private static unsafe VkSurfaceKHR CreateNSWindowSurfaceMvk(IntPtr mvk, VkInstance instance, NSWindowSwapchainSource nsWindowSource)
        {
            NSWindow nswindow = new (nsWindowSource.NSWindow);
            return CreateNSViewSurfaceMvk(mvk, instance, new NSViewSwapchainSource(nswindow.contentView.NativePtr));
        }

        private static void GetMetalLayerFromNSView(NSView contentView, out CAMetalLayer metalLayer)
        {
            if (!CAMetalLayer.TryCast(contentView.layer, out metalLayer))
            {
                metalLayer = CAMetalLayer.New();
                contentView.wantsLayer = true;
                contentView.layer = metalLayer.NativePtr;
            }
        }

        private static unsafe VkSurfaceKHR CreateNSViewSurfaceExt(
            IntPtr ext, VkInstance instance, NSViewSwapchainSource nsViewSource)
        {
            NSView contentView = new(nsViewSource.NSView);
            GetMetalLayerFromNSView(contentView, out CAMetalLayer metalLayer);

            VkMetalSurfaceCreateInfoEXT surfaceCI = new()
            {
                sType = VkStructureType.VK_STRUCTURE_TYPE_METAL_SURFACE_CREATE_INFO_EXT,
                pLayer = (nint*)metalLayer.NativePtr
            };
            VkSurfaceKHR surface;
            VkResult result = ((delegate* unmanaged<VkInstance, VkMetalSurfaceCreateInfoEXT*, VkAllocationCallbacks*, VkSurfaceKHR*, VkResult>)ext)(
                instance, &surfaceCI, null, &surface);
            CheckResult(result);
            return surface;
        }

        private static unsafe VkSurfaceKHR CreateNSViewSurfaceMvk(
            IntPtr mvk, VkInstance instance, NSViewSwapchainSource nsViewSource)
        {
            NSView contentView = new(nsViewSource.NSView);
            GetMetalLayerFromNSView(contentView, out _);

            VkMacOSSurfaceCreateInfoMVK surfaceCI = new()
            {
                sType = VkStructureType.VK_STRUCTURE_TYPE_MACOS_SURFACE_CREATE_INFO_MVK,
                pView = (void*)contentView.NativePtr
            };
            VkSurfaceKHR surface;
            VkResult result = ((delegate* unmanaged<VkInstance, VkMacOSSurfaceCreateInfoMVK*, VkAllocationCallbacks*, VkSurfaceKHR*, VkResult>)mvk)(
                instance, &surfaceCI, null, &surface);
            CheckResult(result);
            return surface;
        }

        private static void GetMetalLayerFromUIView(UIView uiView, out CAMetalLayer metalLayer)
        {
            if (!CAMetalLayer.TryCast(uiView.layer, out metalLayer))
            {
                metalLayer = CAMetalLayer.New();
                metalLayer.frame = uiView.frame;
                metalLayer.opaque = true;
                uiView.layer.addSublayer(metalLayer.NativePtr);
            }
        }

        private static VkSurfaceKHR CreateUIViewSurfaceExt(
            IntPtr ext, VkInstance instance, UIViewSwapchainSource uiViewSource)
        {
            UIView uiView = new(uiViewSource.UIView);
            GetMetalLayerFromUIView(uiView, out CAMetalLayer metalLayer);

            VkMetalSurfaceCreateInfoEXT surfaceCI = new()
            {
                sType = VkStructureType.VK_STRUCTURE_TYPE_METAL_SURFACE_CREATE_INFO_EXT,
                pLayer = (nint*)metalLayer.NativePtr
            };
            VkSurfaceKHR surface;
            VkResult result = ((delegate* unmanaged<VkInstance, VkMetalSurfaceCreateInfoEXT*, VkAllocationCallbacks*, VkSurfaceKHR*, VkResult>)ext)(
                instance, &surfaceCI, null, &surface);
            CheckResult(result);
            return surface;
        }

        private static VkSurfaceKHR CreateUIViewSurfaceMvk(
            IntPtr mvk, VkInstance instance, UIViewSwapchainSource uiViewSource)
        {
            UIView uiView = new(uiViewSource.UIView);
            GetMetalLayerFromUIView(uiView, out _);

            VkIOSSurfaceCreateInfoMVK surfaceCI = new()
            {
                sType = VkStructureType.VK_STRUCTURE_TYPE_IOS_SURFACE_CREATE_INFO_MVK,
                pView = uiView.NativePtr.ToPointer()
            };
            VkSurfaceKHR surface;
            VkResult result = ((delegate* unmanaged<VkInstance, VkIOSSurfaceCreateInfoMVK*, VkAllocationCallbacks*, VkSurfaceKHR*, VkResult>)mvk)(
                instance, &surfaceCI, null, &surface);
            CheckResult(result);
            return surface;
        }
    }
}
