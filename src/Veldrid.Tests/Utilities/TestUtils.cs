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
            try
            {
                int result = Sdl2Native.SDL_Init(SDLInitFlags.Video);
                if (result != 0)
                {
                    InitializationFailedMessage = GetString(Sdl2Native.SDL_GetError());
                    Console.WriteLine($"Failed to initialize SDL2: {InitializationFailedMessage}");
                    InitializedSdl2 = false;
                }
                else
                {
                    InitializedSdl2 = true;
                }
            }
            catch (Exception ex)
            {
                InitializationFailedMessage = ex.ToString();
                Console.WriteLine($"SDL2 intializer threw exception: {ex}");
                InitializedSdl2 = false;
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

            WindowCreateInfo wci = new()
            {
                WindowWidth = 200,
                WindowHeight = 200,
                WindowInitialState = WindowState.Hidden,
            };

            GraphicsDeviceOptions options = new(true, PixelFormat.R16_UNorm, false);

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

            WindowCreateInfo wci = new()
            {
                WindowWidth = 200,
                WindowHeight = 200,
                WindowInitialState = WindowState.Hidden,
            };

            GraphicsDeviceOptions options = new(true, PixelFormat.R16_UNorm, false);

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

            WindowCreateInfo wci = new()
            {
                WindowWidth = 200,
                WindowHeight = 200,
                WindowInitialState = WindowState.Hidden,
            };

            GraphicsDeviceOptions options = new(true, PixelFormat.R16_UNorm, false);

            VeldridStartup.CreateWindowAndGraphicsDevice(wci, options, GraphicsBackend.OpenGL, out window, out gd);
        }

        internal static void CreateOpenGLESDevice(out IDisposable window, out GraphicsDevice gd)
        {
            WindowCreateInfo wci = new()
            {
                WindowWidth = 200,
                WindowHeight = 200,
                WindowInitialState = WindowState.Hidden,
            };

            GraphicsDeviceOptions options = new(true, PixelFormat.R16_UNorm, false);

#if ANDROID
            // TODO: dependency inject this?
            Android.Utilities.AndroidStartup.CreateWindowAndGraphicsDevice(
                wci.WindowWidth, wci.WindowHeight, options, GraphicsBackend.OpenGLES, out window, out gd);
#else
            if (!InitializedSdl2)
            {
                window = null;
                gd = null;
                return;
            }

            VeldridStartup.CreateWindowAndGraphicsDevice(wci, options, GraphicsBackend.OpenGLES, out Sdl2Window sdlWindow, out gd);
            window = new WindowClosable(sdlWindow);
#endif
        }

        public static GraphicsDevice CreateMetalDevice()
        {
            if (!GraphicsDevice.IsBackendSupported(GraphicsBackend.Metal))
            {
                Console.WriteLine("Metal is not supported on this system.");
                return null;
            }
            return GraphicsDevice.CreateMetal(new GraphicsDeviceOptions(true, null, false, ResourceBindingModel.Improved));
        }

        public static void CreateMetalDeviceWithSwapchain(out Sdl2Window window, out GraphicsDevice gd)
        {
            if (!InitializedSdl2)
            {
                window = null;
                gd = null;
                return;
            }

            WindowCreateInfo wci = new()
            {
                WindowWidth = 200,
                WindowHeight = 200,
                WindowInitialState = WindowState.Hidden,
            };

            GraphicsDeviceOptions options = new(true, PixelFormat.R16_UNorm, false, ResourceBindingModel.Improved);

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

        private class WindowClosable : IDisposable
        {
            public Sdl2Window Window { get; }

            public WindowClosable(Sdl2Window window)
            {
                Window = window;
            }

            public void Dispose()
            {
                Window.Close();
            }
        }
    }

    public abstract class GraphicsDeviceTestBase<T> : IDisposable
        where T : IGraphicsDeviceCreator
    {
        private readonly GraphicsDevice _gd;
        private readonly DisposeCollectorResourceFactory _factory;
        private readonly RenderDoc _renderDoc;
        private readonly T _deviceCreator;

        public GraphicsDevice GD => _gd;
        public ResourceFactory RF => _factory;
        public RenderDoc RenderDoc => _renderDoc;

        public GraphicsDeviceTestBase()
        {
            if (Environment.GetEnvironmentVariable("VELDRID_TESTS_ENABLE_RENDERDOC") == "1"
                && RenderDoc.Load(out _renderDoc))
            {
                _renderDoc.APIValidation = true;
                _renderDoc.DebugOutputMute = false;
            }
            _deviceCreator = Activator.CreateInstance<T>();
            _deviceCreator.CreateGraphicsDevice(out _gd);
            _factory = new DisposeCollectorResourceFactory(_gd.ResourceFactory);
        }

        protected DeviceBuffer GetReadback(DeviceBuffer buffer)
        {
            DeviceBuffer readback;
            if ((buffer.Usage & BufferUsage.StagingRead) != 0)
            {
                readback = buffer;
            }
            else
            {
                readback = RF.CreateBuffer(new BufferDescription(buffer.SizeInBytes, BufferUsage.StagingReadWrite));
                readback.Name = $"Readback for ({buffer.Name})";

                CommandList cl = RF.CreateCommandList();
                cl.Begin();
                cl.CopyBuffer(buffer, 0, readback, 0, buffer.SizeInBytes);
                cl.End();
                GD.SubmitCommands(cl);
                GD.WaitForIdle();
            }

            return readback;
        }

        protected Texture GetReadback(Texture texture)
        {
            if ((texture.Usage & TextureUsage.Staging) != 0)
            {
                return texture;
            }
            else
            {
                uint layers = texture.ArrayLayers;
                if ((texture.Usage & TextureUsage.Cubemap) != 0)
                {
                    layers *= 6;
                }
                TextureDescription desc = new(
                    texture.Width, texture.Height, texture.Depth,
                    texture.MipLevels, layers,
                    texture.Format,
                    TextureUsage.Staging, texture.Type);
                Texture readback = RF.CreateTexture(ref desc);
                Fence fence = RF.CreateFence(false);
                CommandList cl = RF.CreateCommandList();
                cl.Begin();
                cl.CopyTexture(texture, readback);
                cl.End();
                GD.SubmitCommands(cl, fence);
                GD.WaitForFence(fence);
                return readback;
            }
        }

        public void Dispose()
        {
            GD.WaitForIdle();
            _factory.DisposeCollector.DisposeAll();
            GD.Dispose();
            (_deviceCreator as IDisposable)?.Dispose();
        }
    }

    public interface IGraphicsDeviceCreator
    {
        void CreateGraphicsDevice(out GraphicsDevice gd);
    }

    public abstract class WindowedDeviceCreator : IGraphicsDeviceCreator, IDisposable
    {
        protected Sdl2Window window;

        public abstract void CreateGraphicsDevice(out GraphicsDevice gd);

        public void Dispose()
        {
            if (window != null)
            {
                window.Close();
                window = null;
            }
        }
    }

    public class VulkanDeviceCreator : IGraphicsDeviceCreator
    {
        public void CreateGraphicsDevice(out GraphicsDevice gd)
        {
            gd = TestUtils.CreateVulkanDevice();
        }
    }

    public class VulkanDeviceCreatorWithMainSwapchain : WindowedDeviceCreator
    {
        public override void CreateGraphicsDevice(out GraphicsDevice gd)
        {
            TestUtils.CreateVulkanDeviceWithSwapchain(out window, out gd);
        }
    }

    public class D3D11DeviceCreator : IGraphicsDeviceCreator
    {
        public void CreateGraphicsDevice(out GraphicsDevice gd)
        {
            gd = TestUtils.CreateD3D11Device();
        }
    }

    public class D3D11DeviceCreatorWithMainSwapchain : WindowedDeviceCreator
    {
        public override void CreateGraphicsDevice(out GraphicsDevice gd)
        {
            TestUtils.CreateD3D11DeviceWithSwapchain(out window, out gd);
        }
    }

    public class OpenGLDeviceCreator : WindowedDeviceCreator
    {
        public override void CreateGraphicsDevice(out GraphicsDevice gd)
        {
            TestUtils.CreateOpenGLDevice(out window, out gd);
        }
    }

    public class OpenGLESDeviceCreator : IGraphicsDeviceCreator, IDisposable
    {
        private IDisposable? window;

        public void CreateGraphicsDevice(out GraphicsDevice gd)
        {
            TestUtils.CreateOpenGLESDevice(out window, out gd);
        }

        public void Dispose()
        {
            if (window != null)
            {
                window.Dispose();
                window = null;
            }
        }
    }

    public class MetalDeviceCreator : IGraphicsDeviceCreator
    {
        public void CreateGraphicsDevice(out GraphicsDevice gd)
        {
            gd = TestUtils.CreateMetalDevice();
        }
    }

    public class MetalDeviceCreatorWithMainSwapchain : WindowedDeviceCreator
    {
        public override void CreateGraphicsDevice(out GraphicsDevice gd)
        {
            TestUtils.CreateMetalDeviceWithSwapchain(out window, out gd);
        }
    }
}
