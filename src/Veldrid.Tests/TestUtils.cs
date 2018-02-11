using System;
using System.Text;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;
using Veldrid.Utilities;

namespace Veldrid.Tests
{
    public static class TestUtils
    {
        private static readonly bool InitializedSdl2;
        private static readonly string InitializationFailedMessage;

        static unsafe TestUtils()
        {
            int result = Sdl2Native.SDL_Init(SDLInitFlags.Video);
            if (result != 0)
            {
                InitializationFailedMessage = GetString(Sdl2Native.SDL_GetError());
                InitializedSdl2 = false;
            }
            else
            {
                InitializedSdl2 = true;
            }
        }

        public static GraphicsDevice CreateVulkanDevice()
        {
            return GraphicsDevice.CreateVulkan(new GraphicsDeviceOptions(true));
        }

        public static void CreateVulkanDeviceWithSwapchain(out Sdl2Window window, out GraphicsDevice gd)
        {
            if (!InitializedSdl2)
            {
                window = null;
                gd = null;
                return;
            }

            WindowCreateInfo wci = new WindowCreateInfo
            {
                WindowWidth = 200,
                WindowHeight = 200,
                WindowInitialState = WindowState.Hidden,
            };

            GraphicsDeviceOptions options = new GraphicsDeviceOptions(true, PixelFormat.R16_UNorm, false);

            VeldridStartup.CreateWindowAndGraphicsDevice(wci, options, GraphicsBackend.Vulkan, out window, out gd);
        }

        public static GraphicsDevice CreateD3D11Device()
        {
            return GraphicsDevice.CreateD3D11(new GraphicsDeviceOptions(true));
        }

        public static void CreateD3D11DeviceWithSwapchain(out Sdl2Window window, out GraphicsDevice gd)
        {
            if (!InitializedSdl2)
            {
                window = null;
                gd = null;
                return;
            }

            WindowCreateInfo wci = new WindowCreateInfo
            {
                WindowWidth = 200,
                WindowHeight = 200,
                WindowInitialState = WindowState.Hidden,
            };

            GraphicsDeviceOptions options = new GraphicsDeviceOptions(true, PixelFormat.R16_UNorm, false);

            VeldridStartup.CreateWindowAndGraphicsDevice(wci, options, GraphicsBackend.Direct3D11, out window, out gd);
        }

        internal static void CreateOpenGLDevice(out Sdl2Window window, out GraphicsDevice gd)
        {
            if (!InitializedSdl2)
            {
                window = null;
                gd = null;
                return;
            }

            WindowCreateInfo wci = new WindowCreateInfo
            {
                WindowWidth = 200,
                WindowHeight = 200,
                WindowInitialState = WindowState.Hidden,
            };

            GraphicsDeviceOptions options = new GraphicsDeviceOptions(true, PixelFormat.R16_UNorm, false);

            VeldridStartup.CreateWindowAndGraphicsDevice(wci, options, GraphicsBackend.OpenGL, out window, out gd);
        }

        public static GraphicsDevice CreateMetalDevice()
        {
            return GraphicsDevice.CreateMetal(new GraphicsDeviceOptions(true));
        }

        public static void CreateMetalDeviceWithSwapchain(out Sdl2Window window, out GraphicsDevice gd)
        {
            if (!InitializedSdl2)
            {
                window = null;
                gd = null;
                return;
            }

            WindowCreateInfo wci = new WindowCreateInfo
            {
                WindowWidth = 200,
                WindowHeight = 200,
                WindowInitialState = WindowState.Hidden,
            };

            GraphicsDeviceOptions options = new GraphicsDeviceOptions(true, PixelFormat.R16_UNorm, false);

            VeldridStartup.CreateWindowAndGraphicsDevice(wci, options, GraphicsBackend.Metal, out window, out gd);
        }

        internal static unsafe string GetString(byte* stringStart)
        {
            int characters = 0;
            while (stringStart[characters] != 0)
            {
                characters++;
            }

            return Encoding.UTF8.GetString(stringStart, characters);
        }
    }

    public abstract class GraphicsDeviceTestBase<T> : IDisposable where T : GraphicsDeviceCreator
    {
        private readonly Sdl2Window _window;
        private readonly GraphicsDevice _gd;
        private readonly DisposeCollectorResourceFactory _factory;

        public GraphicsDevice GD => _gd;
        public ResourceFactory RF => _factory;
        public Sdl2Window Window => _window;

        public GraphicsDeviceTestBase()
        {
            Activator.CreateInstance<T>().CreateGraphicsDevice(out _window, out _gd);
            _factory = new DisposeCollectorResourceFactory(_gd.ResourceFactory);
        }

        public void Dispose()
        {
            GD.WaitForIdle();
            _factory.DisposeCollector.DisposeAll();
            GD.Dispose();
            _window?.Close();
        }
    }

    public interface GraphicsDeviceCreator
    {
        void CreateGraphicsDevice(out Sdl2Window window, out GraphicsDevice gd);
    }

    public class VulkanDeviceCreator : GraphicsDeviceCreator
    {
        public void CreateGraphicsDevice(out Sdl2Window window, out GraphicsDevice gd)
        {
            window = null;
            gd = TestUtils.CreateVulkanDevice();
        }
    }

    public class VulkanDeviceCreatorWithMainSwapchain : GraphicsDeviceCreator
    {
        public unsafe void CreateGraphicsDevice(out Sdl2Window window, out GraphicsDevice gd)
        {
            TestUtils.CreateVulkanDeviceWithSwapchain(out window, out gd);
        }
    }

    public class D3D11DeviceCreator : GraphicsDeviceCreator
    {
        public unsafe void CreateGraphicsDevice(out Sdl2Window window, out GraphicsDevice gd)
        {
            window = null;
            gd = TestUtils.CreateD3D11Device();
        }
    }

    public class D3D11DeviceCreatorWithMainSwapchain : GraphicsDeviceCreator
    {
        public unsafe void CreateGraphicsDevice(out Sdl2Window window, out GraphicsDevice gd)
        {
            TestUtils.CreateD3D11DeviceWithSwapchain(out window, out gd);
        }
    }

    public class OpenGLDeviceCreator : GraphicsDeviceCreator
    {
        public unsafe void CreateGraphicsDevice(out Sdl2Window window, out GraphicsDevice gd)
        {
            TestUtils.CreateOpenGLDevice(out window, out gd);
        }
    }

    public class MetalDeviceCreator : GraphicsDeviceCreator
    {
        public unsafe void CreateGraphicsDevice(out Sdl2Window window, out GraphicsDevice gd)
        {
            window = null;
            gd = TestUtils.CreateMetalDevice();
        }
    }

    public class MetalDeviceCreatorWithMainSwapchain : GraphicsDeviceCreator
    {
        public unsafe void CreateGraphicsDevice(out Sdl2Window window, out GraphicsDevice gd)
        {
            TestUtils.CreateMetalDeviceWithSwapchain(out window, out gd);
        }
    }
}
