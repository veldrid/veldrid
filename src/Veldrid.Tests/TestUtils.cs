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
                Console.WriteLine($"Failed to initialize SDL2: {InitializationFailedMessage}");
                InitializedSdl2 = false;
            }
            else
            {
                InitializedSdl2 = true;
            }
        }

        public static GraphicsDevice CreateVulkanDevice()
        {
            Xunit.Skip.IfNot(GraphicsDevice.IsBackendSupported(GraphicsBackend.Vulkan), "Vulkan is not supported on this system.");
            return GraphicsDevice.CreateVulkan(new GraphicsDeviceOptions(true));
        }

        public static void CreateVulkanDeviceWithSwapchain(out Sdl2Window window, out GraphicsDevice gd)
        {
            Xunit.Skip.IfNot(GraphicsDevice.IsBackendSupported(GraphicsBackend.Vulkan), "Vulkan is not supported on this system.");
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
            Xunit.Skip.IfNot(GraphicsDevice.IsBackendSupported(GraphicsBackend.Direct3D11), "Direct3D11 is not supported on this system.");
            return GraphicsDevice.CreateD3D11(new GraphicsDeviceOptions(true));
        }

        public static void CreateD3D11DeviceWithSwapchain(out Sdl2Window window, out GraphicsDevice gd)
        {
            Xunit.Skip.IfNot(GraphicsDevice.IsBackendSupported(GraphicsBackend.Direct3D11), "Direct3D11 is not supported on this system.");
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
            Xunit.Skip.IfNot(GraphicsDevice.IsBackendSupported(GraphicsBackend.OpenGL), "OpenGL is not supported on this system.");
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

        internal static void CreateOpenGLESDevice(out Sdl2Window window, out GraphicsDevice gd)
        {
            Xunit.Skip.IfNot(GraphicsDevice.IsBackendSupported(GraphicsBackend.OpenGLES), "OpenGLES is not supported on this system.");
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

            VeldridStartup.CreateWindowAndGraphicsDevice(wci, options, GraphicsBackend.OpenGLES, out window, out gd);
        }

        public static GraphicsDevice CreateMetalDevice()
        {
            Xunit.Skip.IfNot(GraphicsDevice.IsBackendSupported(GraphicsBackend.Metal), "Metal is not supported on this system.");
            return GraphicsDevice.CreateMetal(new GraphicsDeviceOptions(true, null, false, ResourceBindingModel.Improved));
        }

        public static void CreateMetalDeviceWithSwapchain(out Sdl2Window window, out GraphicsDevice gd)
        {
            Xunit.Skip.IfNot(GraphicsDevice.IsBackendSupported(GraphicsBackend.Metal), "Metal is not supported on this system.");
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

            GraphicsDeviceOptions options = new GraphicsDeviceOptions(true, PixelFormat.R16_UNorm, false, ResourceBindingModel.Improved);

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
        private readonly DisposeCollectorResourceFactory _factory;
        private readonly RenderDoc _renderDoc;

        private T _graphiceDeviceCreator;

        public GraphicsDevice GD => _graphiceDeviceCreator.GraphicsDevice;
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
            _graphiceDeviceCreator = Activator.CreateInstance<T>();
            _factory = new DisposeCollectorResourceFactory(GD.ResourceFactory);
        }

        protected DeviceBuffer GetReadback(DeviceBuffer buffer)
        {
            DeviceBuffer readback;
            if ((buffer.Usage & BufferUsage.Staging) != 0)
            {
                readback = buffer;
            }
            else
            {
                readback = RF.CreateBuffer(new BufferDescription(buffer.SizeInBytes, BufferUsage.Staging));
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
                TextureDescription desc = new TextureDescription(
                    texture.Width, texture.Height, texture.Depth,
                    texture.MipLevels, layers,
                    texture.Format,
                    TextureUsage.Staging, texture.Type);
                Texture readback = RF.CreateTexture(ref desc);
                CommandList cl = RF.CreateCommandList();
                cl.Begin();
                cl.CopyTexture(texture, readback);
                cl.End();
                GD.SubmitCommands(cl);
                GD.WaitForIdle();
                return readback;
            }
        }

        public void Dispose()
        {
            GD.WaitForIdle();
            _factory.DisposeCollector.DisposeAll();
            _graphiceDeviceCreator.Dispose();
        }
    }

    public interface GraphicsDeviceCreator : IDisposable
    {
        public GraphicsDevice GraphicsDevice { get; }
    }

    public abstract class WindowedDeviceCreator : GraphicsDeviceCreator
    {
        public Sdl2Window Sdl2Window { get; private set; }
        public GraphicsDevice GraphicsDevice { get; private set; }

        protected delegate void WindowAndGraphicsDeviceCreationDelegate(out Sdl2Window window, out GraphicsDevice graphicsDevice);

        protected WindowedDeviceCreator(WindowAndGraphicsDeviceCreationDelegate creationDelegate)
        {
            creationDelegate(out var window, out var graphicsDevice);
            Sdl2Window = window;
            GraphicsDevice = graphicsDevice;
        }

        public void Dispose()
        {
            GraphicsDevice.Dispose();
            Sdl2Window.Close();
        }
    }

    public class VulkanDeviceCreator : GraphicsDeviceCreator
    {
        public GraphicsDevice GraphicsDevice { get; } = TestUtils.CreateVulkanDevice();

        public void Dispose()
        {
            GraphicsDevice.Dispose();
        }
    }

    public class VulkanDeviceCreatorWithMainSwapchain : WindowedDeviceCreator
    {
        public VulkanDeviceCreatorWithMainSwapchain() : base(TestUtils.CreateVulkanDeviceWithSwapchain) { }
    }

    public class D3D11DeviceCreator : GraphicsDeviceCreator
    {
        public GraphicsDevice GraphicsDevice { get; } = TestUtils.CreateD3D11Device();

        public void Dispose()
        {
            GraphicsDevice.Dispose();
        }
    }

    public class D3D11DeviceCreatorWithMainSwapchain : WindowedDeviceCreator
    {
        public D3D11DeviceCreatorWithMainSwapchain() : base(TestUtils.CreateD3D11DeviceWithSwapchain) { }
    }

    public class OpenGLDeviceCreator : WindowedDeviceCreator
    {
        public OpenGLDeviceCreator() : base(TestUtils.CreateOpenGLDevice) { }
    }

    public class OpenGLESDeviceCreator : WindowedDeviceCreator
    {
        public OpenGLESDeviceCreator() : base(TestUtils.CreateOpenGLESDevice) { }
    }

    public class MetalDeviceCreator : GraphicsDeviceCreator
    {
        public GraphicsDevice GraphicsDevice { get; } = TestUtils.CreateMetalDevice();

        public void Dispose()
        {
            GraphicsDevice.Dispose();
        }
    }

    public class MetalDeviceCreatorWithMainSwapchain : WindowedDeviceCreator
    {
        public MetalDeviceCreatorWithMainSwapchain() : base(TestUtils.CreateMetalDeviceWithSwapchain) { }
    }
}
