using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Veldrid.Vk;
using Veldrid.Sdl2;
using Veldrid.OpenGL;

namespace Veldrid.StartupUtilities
{
    public static class VeldridStartup
    {
        public static void CreateWindowAndGraphicsDevice(
            ref WindowCreateInfo windowCI,
            ref GraphicsDeviceCreateInfo graphicsDeviceCI,
            out Sdl2Window window,
            out GraphicsDevice gd)
        {
            window = CreateWindow(ref windowCI);
            gd = CreateGraphicsDevice(ref graphicsDeviceCI, window);
        }

        public static Sdl2Window CreateWindow(ref WindowCreateInfo windowCI)
        {
            Sdl2Window window = new Sdl2Window(
                windowCI.WindowTitle,
                windowCI.X,
                windowCI.Y,
                windowCI.WindowWidth,
                windowCI.WindowHeight,
                SDL_WindowFlags.OpenGL | SDL_WindowFlags.Shown | SDL_WindowFlags.Resizable
                    | GetWindowFlags(windowCI.WindowInitialState),
                false);

            return window;
        }

        public static bool IsSupported(GraphicsBackend backend)
        {
            return true; // TODO
        }

        private static SDL_WindowFlags GetWindowFlags(WindowState state)
        {
            switch (state)
            {
                case WindowState.Normal:
                    return 0;
                case WindowState.FullScreen:
                    return SDL_WindowFlags.Fullscreen;
                case WindowState.Maximized:
                    return SDL_WindowFlags.Maximized;
                case WindowState.Minimized:
                    return SDL_WindowFlags.Minimized;
                case WindowState.BorderlessFullScreen:
                    return SDL_WindowFlags.FullScreenDesktop;
                default:
                    throw new VeldridException("Invalid WindowState: " + state);
            }
        }

        public static GraphicsDevice CreateGraphicsDevice(ref GraphicsDeviceCreateInfo graphicsDeviceCI, Sdl2Window window)
        {
            GraphicsBackend? backend = graphicsDeviceCI.Backend;
            if (!backend.HasValue)
            {
                backend = GetPlatformDefaultBackend();
            }
            switch (backend)
            {
                case GraphicsBackend.Direct3D11:
                    return CreateDefaultD3D11GraphicsDevice(ref graphicsDeviceCI, window);
                case GraphicsBackend.Vulkan:
                    return CreateVulkanGraphicsDevice(ref graphicsDeviceCI, window);
                case GraphicsBackend.OpenGL:
                    return CreateDefaultOpenGLGraphicsDevice(ref graphicsDeviceCI, window);
                //case GraphicsBackend.OpenGLES:
                //    return CreateDefaultOpenGLESRenderContext(ref contextCI, window);
                default:
                    throw new VeldridException("Invalid GraphicsBackend: " + graphicsDeviceCI.Backend);
            }
        }

        private static GraphicsBackend? GetPlatformDefaultBackend()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return GraphicsBackend.Direct3D11;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return GraphicsBackend.OpenGL;
            }
            else
            {
                return IsSupported(GraphicsBackend.Vulkan) ? GraphicsBackend.Vulkan : GraphicsBackend.OpenGL;
            }
        }

        public static unsafe GraphicsDevice CreateVulkanGraphicsDevice(ref GraphicsDeviceCreateInfo contextCI, Sdl2Window window)
        {
            IntPtr sdlHandle = window.SdlWindowHandle;
            SDL_SysWMinfo sysWmInfo;
            Sdl2Native.SDL_GetVersion(&sysWmInfo.version);
            Sdl2Native.SDL_GetWMWindowInfo(sdlHandle, &sysWmInfo);
            VkSurfaceSource surfaceSource = GetSurfaceSource(sysWmInfo);
            GraphicsDevice gd = GraphicsDevice.CreateVulkan(surfaceSource, (uint)window.Width, (uint)window.Height, contextCI.DebugDevice);

            return gd;
        }

        private static unsafe VkSurfaceSource GetSurfaceSource(SDL_SysWMinfo sysWmInfo)
        {
            switch (sysWmInfo.subsystem)
            {
                case SysWMType.Windows:
                    Win32WindowInfo w32Info = Unsafe.Read<Win32WindowInfo>(&sysWmInfo.info);
                    return VkSurfaceSource.CreateWin32(w32Info.hinstance, w32Info.Sdl2Window);
                case SysWMType.X11:
                    X11WindowInfo x11Info = Unsafe.Read<X11WindowInfo>(&sysWmInfo.info);
                    return VkSurfaceSource.CreateXlib(
                        (Vulkan.Xlib.Display*)x11Info.display,
                        new Vulkan.Xlib.Window() { Value = x11Info.Sdl2Window });
                default:
                    throw new PlatformNotSupportedException("Cannot create a Vulkan surface for " + sysWmInfo.subsystem + ".");
            }
        }

        //public static OpenGLESRenderContext CreateDefaultOpenGLESRenderContext(ref RenderContextCreateInfo contextCI, Sdl2Window window)
        //{
        //    if (contextCI.DebugContext)
        //    {
        //        Sdl2Native.SDL_GL_SetAttribute(SDL_GLAttribute.ContextFlags, (int)SDL_GLContextFlag.Debug);
        //    }

        //    IntPtr sdlHandle = window.SdlWindowHandle;
        //    Sdl2Native.SDL_GL_SetAttribute(SDL_GLAttribute.ContextProfileMask, (int)SDL_GLProfile.ES);
        //    Sdl2Native.SDL_GL_SetAttribute(SDL_GLAttribute.ContextMajorVersion, 3);
        //    Sdl2Native.SDL_GL_SetAttribute(SDL_GLAttribute.ContextMinorVersion, 0);
        //    IntPtr contextHandle = Sdl2Native.SDL_GL_CreateContext(sdlHandle);
        //    Sdl2Native.SDL_GL_MakeCurrent(sdlHandle, contextHandle);

        //    if (contextHandle == IntPtr.Zero)
        //    {
        //        unsafe
        //        {
        //            byte* error = Sdl2Native.SDL_GetError();
        //            string errorString = Utilities.GetString(error);
        //            throw new VeldridException("Unable to create GL Context: " + errorString);
        //        }
        //    }

        //    OpenGLPlatformContextInfo ci = new OpenGLPlatformContextInfo(
        //        contextHandle,
        //        Sdl2Native.SDL_GL_GetProcAddress,
        //        Sdl2Native.SDL_GL_GetCurrentContext,
        //        () => Sdl2Native.SDL_GL_SwapWindow(sdlHandle));
        //    OpenGLESRenderContext rc = new OpenGLESRenderContext(window, ci);
        //    if (contextCI.DebugContext)
        //    {
        //        rc.EnableDebugCallback(OpenTK.Graphics.ES30.DebugSeverity.DebugSeverityLow);
        //    }
        //    return rc;
        //}

        public static GraphicsDevice CreateDefaultOpenGLGraphicsDevice(ref GraphicsDeviceCreateInfo gdCI, Sdl2Window window)
        {
            IntPtr sdlHandle = window.SdlWindowHandle;
            if (gdCI.DebugDevice)
            {
                Sdl2Native.SDL_GL_SetAttribute(SDL_GLAttribute.ContextFlags, (int)SDL_GLContextFlag.Debug);
            }

            Sdl2Native.SDL_GL_SetAttribute(SDL_GLAttribute.ContextProfileMask, (int)SDL_GLProfile.Core);
            Sdl2Native.SDL_GL_SetAttribute(SDL_GLAttribute.ContextMajorVersion, 4);
            Sdl2Native.SDL_GL_SetAttribute(SDL_GLAttribute.ContextMinorVersion, 0);

            IntPtr contextHandle = Sdl2Native.SDL_GL_CreateContext(sdlHandle);
            if (contextHandle == IntPtr.Zero)
            {
                unsafe
                {
                    byte* error = Sdl2Native.SDL_GetError();
                    string errorString = GetString(error);
                    throw new VeldridException("Unable to create GL Context: " + errorString);
                }
            }

            int result = Sdl2Native.SDL_GL_SetSwapInterval(0);

            OpenGLPlatformInfo platformInfo = new OpenGLPlatformInfo(
                contextHandle,
                Sdl2Native.SDL_GL_GetProcAddress,
                context => Sdl2Native.SDL_GL_MakeCurrent(sdlHandle, context),
                () => Sdl2Native.SDL_GL_MakeCurrent(new SDL_Window(IntPtr.Zero), IntPtr.Zero),
                Sdl2Native.SDL_GL_DeleteContext,
                () => Sdl2Native.SDL_GL_SwapWindow(sdlHandle));

            return GraphicsDevice.CreateOpenGL(
                platformInfo,
                (uint)window.Width,
                (uint)window.Height,
                gdCI.DebugDevice);
        }

        public static GraphicsDevice CreateDefaultD3D11GraphicsDevice(ref GraphicsDeviceCreateInfo deviceCI, Sdl2Window window)
        {
            SharpDX.Direct3D11.DeviceCreationFlags flags = SharpDX.Direct3D11.DeviceCreationFlags.None;
            if (deviceCI.DebugDevice)
            {
                flags |= SharpDX.Direct3D11.DeviceCreationFlags.Debug;
            }

            return GraphicsDevice.CreateD3D11(window.Handle, (uint)window.Width, (uint)window.Height);
        }

        private static unsafe string GetString(byte* stringStart)
        {
            int characters = 0;
            while (stringStart[characters] != 0)
            {
                characters++;
            }

            return Encoding.UTF8.GetString(stringStart, characters);
        }

    }
}
