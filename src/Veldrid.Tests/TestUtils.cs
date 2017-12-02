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

        public static void CreateVulkanDevice(out Sdl2Window window, out GraphicsDevice gd)
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

            GraphicsDeviceOptions options = new GraphicsDeviceOptions(true, null, false);

            VeldridStartup.CreateWindowAndGraphicsDevice(wci, options, GraphicsBackend.Vulkan, out window, out gd);
        }

        internal static void CreateD3D11Device(out Sdl2Window window, out GraphicsDevice gd)
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

            GraphicsDeviceOptions options = new GraphicsDeviceOptions(true, null, false);

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

            GraphicsDeviceOptions options = new GraphicsDeviceOptions(true, null, false);

            VeldridStartup.CreateWindowAndGraphicsDevice(wci, options, GraphicsBackend.OpenGL, out window, out gd);
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

        public GraphicsDeviceTestBase()
        {
            Activator.CreateInstance<T>().CreateGrapicsDevice(out _window, out _gd);
            _factory = new DisposeCollectorResourceFactory(_gd.ResourceFactory);
        }

        public void Dispose()
        {
            _window.Close();
            _window.PumpEvents();
            GD.WaitForIdle();
            _factory.DisposeCollector.DisposeAll();
            if (GD.BackendType != GraphicsBackend.OpenGL)
            {
                GD.Dispose();
            }
        }
    }

    public interface GraphicsDeviceCreator
    {
        void CreateGrapicsDevice(out Sdl2Window window, out GraphicsDevice gd);
    }

    public class VulkanDeviceCreator : GraphicsDeviceCreator
    {
        public unsafe void CreateGrapicsDevice(out Sdl2Window window, out GraphicsDevice gd)
        {
            TestUtils.CreateVulkanDevice(out window, out gd);
        }
    }

    public class D3D11DeviceCreator : GraphicsDeviceCreator
    {
        public unsafe void CreateGrapicsDevice(out Sdl2Window window, out GraphicsDevice gd)
        {
            TestUtils.CreateD3D11Device(out window, out gd);
        }
    }

    public class OpenGLDeviceCreator : GraphicsDeviceCreator
    {
        public unsafe void CreateGrapicsDevice(out Sdl2Window window, out GraphicsDevice gd)
        {
            TestUtils.CreateOpenGLDevice(out window, out gd);
        }
    }
}
