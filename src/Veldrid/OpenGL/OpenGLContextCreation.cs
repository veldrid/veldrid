using System;
using System.Runtime.InteropServices;
using Veldrid.OpenGL.WGL;
using static Veldrid.OpenGL.GLX.GLXNative;

namespace Veldrid.OpenGL
{
    internal static class OpenGLContextCreation
    {
        public static OpenGLPlatformInfo CreateContext(
            GraphicsDeviceOptions options,
            SwapchainDescription scDesc,
            GraphicsBackend backend,
            IntPtr shareContext)
        {
            if (scDesc.Source is Win32SwapchainSource win32Source)
            {
                return CreateContextWin32(
                    options,
                    win32Source.Hwnd,
                    scDesc.DepthFormat,
                    scDesc.SyncToVerticalBlank,
                    backend,
                    shareContext);
            }
            else if (scDesc.Source is XlibSwapchainSource xlibSource)
            {
                throw new NotImplementedException();
            }
            else
            {
                throw new NotSupportedException(
                    $"Creating an OpenGL context from a {scDesc.Source.GetType().Name} is not supported.");
            }
        }

        public static OpenGLPlatformInfo CreateContextWin32(
            GraphicsDeviceOptions options,
            IntPtr hwnd,
            PixelFormat? depthFormat,
            bool syncToVerticalBlank,
            GraphicsBackend backend,
            IntPtr shareContext)
        {
            IntPtr hdc;
            hdc = WindowsNative.GetDC(hwnd);
            WindowsExtensionCreationFunctions extensionFuncs = WindowsNative.GetExtensionFunctions();

            uint depthBits = 0;
            uint stencilBits = 0;
            if (depthFormat.HasValue)
            {
                depthBits = (uint)GetDepthBits(depthFormat.Value);
                stencilBits = (uint)GetStencilBits(depthFormat.Value);
            }

            IntPtr glContext;
            if (extensionFuncs.IsSupported)
            {
                (int major, int minor) = WindowsNative.GetMaxGLVersion(backend == GraphicsBackend.OpenGLES);
                if (!WindowsNative.CreateContextWithExtension(
                    extensionFuncs,
                    backend,
                    hdc,
                    options.Debug,
                    depthBits,
                    stencilBits,
                    major,
                    minor,
                    IntPtr.Zero,
                    out glContext))
                {
                    throw new VeldridException($"Failed to create OpenGL context.");
                }
            }
            else
            {
                if (backend == GraphicsBackend.OpenGLES)
                {
                    throw new VeldridException($"OpenGL ES is not supported on this system.");
                }

                if (!WindowsNative.CreateContextRegular(hdc, depthBits, stencilBits, out glContext))
                {
                    throw new VeldridException($"Failed to create OpenGL context.");
                }
            }

            WindowsNative.wglMakeCurrent(hdc, glContext);

            IntPtr glLibHandle = WindowsNative.GetOpengl32Lib();
            Func<string, IntPtr> getProcAddress = name =>
            {
                IntPtr ret = WindowsNative.wglGetProcAddress(name);
                if (ret == IntPtr.Zero)
                {
                    ret = WindowsNative.GetProcAddress(glLibHandle, name);
                }
                return ret;
            };

            IntPtr setSwapIntervalPtr = getProcAddress("wglSwapIntervalEXT");
            wglSwapIntervalEXT setSwapInterval = Marshal.GetDelegateForFunctionPointer<wglSwapIntervalEXT>(setSwapIntervalPtr);
            setSwapInterval(syncToVerticalBlank ? 1 : 0);

            WindowsNative.wglMakeCurrent(IntPtr.Zero, IntPtr.Zero);

            Action<IntPtr> deleteContext = ctx =>
            {
                WindowsNative.ReleaseDC(hwnd, hdc);
                WindowsNative.wglDeleteContext(ctx);
            };

            OpenGLPlatformInfo platformInfo = new OpenGLPlatformInfo(
                glContext,
                getProcAddress,
                ctx => WindowsNative.wglMakeCurrent(hdc, ctx),
                () => WindowsNative.wglGetCurrentContext(),
                () => WindowsNative.wglMakeCurrent(IntPtr.Zero, IntPtr.Zero),
                deleteContext,
                () => WindowsNative.SwapBuffers(hdc),
                sync => setSwapInterval(sync ? 1 : 0));

            return platformInfo;
        }

        public static unsafe OpenGLPlatformInfo CreateContextXlib(
            GraphicsDeviceOptions options,
            IntPtr display,
            IntPtr window,
            PixelFormat? depthFormat,
            bool syncToVerticalBlank,
            GraphicsBackend backend,
            IntPtr shareContext)
        {
            IntPtr screen = XDefaultScreenOfDisplay(display);
            int screenId = XDefaultScreen(display);

            XWindowAttributes windowAttribs;
            int result = XGetWindowAttributes(display, window, &windowAttribs);
            if (result == 0)
            {
                throw new VeldridException($"Failed to retrieve the X11 window attributes.");
            }

            XVisualInfo vinfoTemplate;
            vinfoTemplate.visual = windowAttribs.visual;
            int retCount;
            XVisualInfo* visualInfos = XGetVisualInfo(display, 0, &vinfoTemplate, &retCount);

            int profileMask = backend == GraphicsBackend.OpenGL
                ? GLX_CONTEXT_CORE_PROFILE_BIT_ARB
                : GLX_CONTEXT_ES_PROFILE_BIT_EXT;

            int[] context_attribs =
            {
                GLX_CONTEXT_MAJOR_VERSION_ARB, 3,
                GLX_CONTEXT_MINOR_VERSION_ARB, 2,
                GLX_CONTEXT_PROFILE_MASK_ARB, profileMask,
                0
            };

            IntPtr fbConfig = glXGetFBConfigFromVisualSGIX(display, &visualInfos[0]);

            IntPtr context;
            fixed (int* attribsPtr = context_attribs)
            {
                context = glXCreateContextAttribsARB(display, fbConfig, shareContext, 1, attribsPtr);
            }

            Func<string, IntPtr> getProcAddress = name => glXGetProcAddress(name);

            Action<IntPtr> makeCurrent = ctx => glXMakeCurrent(display, window, ctx);

            Func<IntPtr> getCurrentContext = () => glXGetCurrentContext();

            Action<IntPtr> deleteContext = ctx => glXDestroyContext(display, ctx);

            Action clearCurrentContext = () => glXMakeCurrent(IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);

            Action swapBuffers = () => glXSwapBuffers(display, window);

            Action<bool> setSyncToVerticalBlank = val => { }; // TODO

            return new OpenGLPlatformInfo(
                context,
                getProcAddress,
                makeCurrent,
                getCurrentContext,
                clearCurrentContext,
                deleteContext,
                swapBuffers,
                setSyncToVerticalBlank);
        }

        private static int GetDepthBits(PixelFormat value)
        {
            switch (value)
            {
                case PixelFormat.R16_UNorm:
                    return 16;
                case PixelFormat.R32_Float:
                    return 32;
                default:
                    throw new VeldridException($"Unsupported depth format: {value}");
            }
        }

        private static int GetStencilBits(PixelFormat format)
        {
            switch (format)
            {
                case PixelFormat.D24_UNorm_S8_UInt:
                case PixelFormat.D32_Float_S8_UInt:
                    return 8;
                default:
                    return 0;
            }
        }
    }
}
