using System;
using System.Runtime.InteropServices;
using Veldrid.OpenGL.WGL;

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
            IntPtr hdc;
            IntPtr hwnd = Util.AssertSubtype<SwapchainSource, Win32SwapchainSource>(scDesc.Source).Hwnd;
            hdc = WindowsNative.GetDC(hwnd);
            WindowsExtensionCreationFunctions extensionFuncs = WindowsNative.GetExtensionFunctions();

            uint depthBits = 0;
            uint stencilBits = 0;
            if (scDesc.DepthFormat.HasValue)
            {
                PixelFormat depthFormat = scDesc.DepthFormat.Value;
                depthBits = (uint)GetDepthBits(depthFormat);
                stencilBits = (uint)GetStencilBits(depthFormat);
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

            //WindowsNative.wglMakeCurrent(hdc, glContext);

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
            wglSwapIntervalEXT setSwapInterval = null;
            //wglSwapIntervalEXT setSwapInterval = Marshal.GetDelegateForFunctionPointer<wglSwapIntervalEXT>(setSwapIntervalPtr);
            //setSwapInterval(scDesc.SyncToVerticalBlank ? 1 : 0);

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
