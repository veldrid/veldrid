using System.IO;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;
using Veldrid.Utilities;

public abstract class SampleApplication
{
    protected readonly Sdl2Window _window;
    protected readonly GraphicsDevice _gd;
    private bool _windowResized;

    public SampleApplication()
    {
        WindowCreateInfo wci = new WindowCreateInfo
        {
            X = 100,
            Y = 100,
            WindowWidth = 960,
            WindowHeight = 540,
            WindowTitle = GetTitle(),
        };
        _window = VeldridStartup.CreateWindow(ref wci);
        _window.Resized += () =>
        {
            _windowResized = true;
            OnWindowResized();
        };
        _window.MouseMove += OnMouseMove;
        _window.KeyDown += OnKeyDown;

        GraphicsDeviceOptions options = new GraphicsDeviceOptions(false, PixelFormat.R16_UNorm, true);
#if DEBUG
        options.Debug = true;
#endif
        _gd = VeldridStartup.CreateGraphicsDevice(_window, options);
    }

    protected virtual void OnMouseMove(MouseMoveEventArgs mouseMoveEvent)
    {
    }

    protected virtual void OnKeyDown(KeyEvent keyEvent)
    {
    }

    protected virtual string GetTitle() => GetType().Name;

    public void Run()
    {
        DisposeCollectorResourceFactory factory = new DisposeCollectorResourceFactory(_gd.ResourceFactory);
        CreateResources(factory);

        while (_window.Exists)
        {
            _window.PumpEvents();

            if (_window.Exists)
            {
                if (_windowResized)
                {
                    _gd.ResizeMainWindow((uint)_window.Width, (uint)_window.Height);
                    HandleWindowResize();
                }
                Draw();
            }
        }

        factory.DisposeCollector.DisposeAll();
        _gd.Dispose();
    }

    protected abstract void CreateResources(ResourceFactory factory);

    protected abstract void Draw();

    protected virtual void OnWindowResized() { }

    protected virtual void HandleWindowResize() { }

    public static Shader LoadShader(ResourceFactory factory, string set, ShaderStages stage, string entryPoint)
    {
        string path = Path.Combine(
            System.AppContext.BaseDirectory,
            "Shaders",
            $"{set}-{stage.ToString().ToLower()}.{GetExtension(factory.BackendType)}");
        return factory.CreateShader(new ShaderDescription(stage, File.ReadAllBytes(path), entryPoint));
    }

    private static string GetExtension(GraphicsBackend backendType)
    {
        return (backendType == GraphicsBackend.Direct3D11)
            ? "hlsl.bytes"
            : (backendType == GraphicsBackend.Vulkan)
                ? "450.glsl.spv"
                : (backendType == GraphicsBackend.Metal)
                    ? "metal"
                    : "330.glsl";
    }
}