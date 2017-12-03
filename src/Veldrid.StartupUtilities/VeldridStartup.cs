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
            WindowCreateInfo windowCI,
            out Sdl2Window window,
            out GraphicsDevice gd)
            => CreateWindowAndGraphicsDevice(
                windowCI,
                new GraphicsDeviceOptions(),
                GetPlatformDefaultBackend(),
                out window,
                out gd);

        public static void CreateWindowAndGraphicsDevice(
            WindowCreateInfo windowCI,
            GraphicsDeviceOptions deviceOptions,
            out Sdl2Window window,
            out GraphicsDevice gd)
            => CreateWindowAndGraphicsDevice(windowCI, deviceOptions, GetPlatformDefaultBackend(), out window, out gd);

        public static void CreateWindowAndGraphicsDevice(
            WindowCreateInfo windowCI,
            GraphicsDeviceOptions deviceOptions,
            GraphicsBackend preferredBackend,
            out Sdl2Window window,
            out GraphicsDevice gd)
        {
            window = CreateWindow(ref windowCI);
            gd = CreateGraphicsDevice(window, deviceOptions, preferredBackend);
        }

        public static Sdl2Window CreateWindow(ref WindowCreateInfo windowCI)
        {
            SDL_WindowFlags flags = SDL_WindowFlags.OpenGL | SDL_WindowFlags.Resizable
                    | GetWindowFlags(windowCI.WindowInitialState);
            if (windowCI.WindowInitialState != WindowState.Hidden)
            {
                flags |= SDL_WindowFlags.Shown;
            }
            Sdl2Window window = new Sdl2Window(
                windowCI.WindowTitle,
                windowCI.X,
                windowCI.Y,
                windowCI.WindowWidth,
                windowCI.WindowHeight,
                flags,
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
                case WindowState.Hidden:
                    return SDL_WindowFlags.Hidden;
                default:
                    throw new VeldridException("Invalid WindowState: " + state);
            }
        }

        public static GraphicsDevice CreateGraphicsDevice(Sdl2Window window)
            => CreateGraphicsDevice(window, new GraphicsDeviceOptions(), GetPlatformDefaultBackend());
        public static GraphicsDevice CreateGraphicsDevice(Sdl2Window window, GraphicsDeviceOptions options)
            => CreateGraphicsDevice(window, options, GetPlatformDefaultBackend());
        public static GraphicsDevice CreateGraphicsDevice(Sdl2Window window, GraphicsBackend preferredBackend)
            => CreateGraphicsDevice(window, new GraphicsDeviceOptions(), preferredBackend);
        public static GraphicsDevice CreateGraphicsDevice(
            Sdl2Window window,
            GraphicsDeviceOptions options,
            GraphicsBackend preferredBackend)
        {
            switch (preferredBackend)
            {
                case GraphicsBackend.Direct3D11:
                    return CreateDefaultD3D11GraphicsDevice(options, window);
                case GraphicsBackend.Vulkan:
                    return CreateVulkanGraphicsDevice(options, window);
                case GraphicsBackend.OpenGL:
                    return CreateDefaultOpenGLGraphicsDevice(options, window);
                //case GraphicsBackend.OpenGLES:
                //    return CreateDefaultOpenGLESRenderContext(ref contextCI, window);
                default:
                    throw new VeldridException("Invalid GraphicsBackend: " + preferredBackend);
            }
        }

        private static GraphicsBackend GetPlatformDefaultBackend()
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

        public static unsafe GraphicsDevice CreateVulkanGraphicsDevice(GraphicsDeviceOptions options, Sdl2Window window)
        {
            IntPtr sdlHandle = window.SdlWindowHandle;
            SDL_SysWMinfo sysWmInfo;
            Sdl2Native.SDL_GetVersion(&sysWmInfo.version);
            Sdl2Native.SDL_GetWMWindowInfo(sdlHandle, &sysWmInfo);
            VkSurfaceSource surfaceSource = GetSurfaceSource(sysWmInfo);
            GraphicsDevice gd = GraphicsDevice.CreateVulkan(options, surfaceSource, (uint)window.Width, (uint)window.Height);

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

        public static GraphicsDevice CreateDefaultOpenGLGraphicsDevice(GraphicsDeviceOptions options, Sdl2Window window)
        {
            IntPtr sdlHandle = window.SdlWindowHandle;
            if (options.Debug)
            {
                Sdl2Native.SDL_GL_SetAttribute(SDL_GLAttribute.ContextFlags, (int)SDL_GLContextFlag.Debug);
            }

            Sdl2Native.SDL_GL_SetAttribute(SDL_GLAttribute.ContextProfileMask, (int)SDL_GLProfile.Core);
            Sdl2Native.SDL_GL_SetAttribute(SDL_GLAttribute.ContextMajorVersion, 4);
            Sdl2Native.SDL_GL_SetAttribute(SDL_GLAttribute.ContextMinorVersion, 0);

            int depthBits = 0;
            if (options.SwapchainDepthFormat.HasValue)
            {
                switch (options.SwapchainDepthFormat)
                {
                    case PixelFormat.R16_UNorm:
                        depthBits = 16;
                        break;
                    case PixelFormat.R32_Float:
                        depthBits = 32;
                        break;
                    default:
                        throw new VeldridException("Invalid depth format: " + options.SwapchainDepthFormat.Value);
                }
            }

            Sdl2Native.SDL_GL_SetAttribute(SDL_GLAttribute.DepthSize, depthBits);

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

            int result = Sdl2Native.SDL_GL_SetSwapInterval(options.SyncToVerticalBlank ? 1 : 0);

            OpenGLPlatformInfo platformInfo = new OpenGLPlatformInfo(
                contextHandle,
                Sdl2Native.SDL_GL_GetProcAddress,
                context => Sdl2Native.SDL_GL_MakeCurrent(sdlHandle, context),
                () => Sdl2Native.SDL_GL_GetCurrentContext(),
                () => Sdl2Native.SDL_GL_MakeCurrent(new SDL_Window(IntPtr.Zero), IntPtr.Zero),
                Sdl2Native.SDL_GL_DeleteContext,
                () => Sdl2Native.SDL_GL_SwapWindow(sdlHandle),
                sync => Sdl2Native.SDL_GL_SetSwapInterval(sync ? 1 : 0));

            return GraphicsDevice.CreateOpenGL(
                options,
                platformInfo,
                (uint)window.Width,
                (uint)window.Height);
        }

        public static GraphicsDevice CreateDefaultD3D11GraphicsDevice(GraphicsDeviceOptions options, Sdl2Window window)
        {
            return GraphicsDevice.CreateD3D11(options, window.Handle, (uint)window.Width, (uint)window.Height);
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
