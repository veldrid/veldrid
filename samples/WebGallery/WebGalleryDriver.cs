using Microsoft.AspNetCore.Components;
using Snake;
using System.Numerics;
using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;

[assembly: SupportedOSPlatform("browser")]

namespace Veldrid.SampleGallery
{
    public partial class WebGalleryDriver : IComponent, IGalleryDriver
    {
        private RenderHandle _renderHandle;

        const int CanvasWidth = 1280;
        const int CanvasHeight = 720;

        private Action<double> _loop;
        private double _deltaSeconds;
        private double _previousMilliseconds;
        private AdvancedFrameLoop _frameloop;

        private uint _width;
        private uint _height;
        private InputState _inputState = new InputState();

        public uint Width => MainSwapchain.Width;
        public uint Height => MainSwapchain.Height;

        public GraphicsDevice Device { get; set; }

        private Vector2 _previousMousePosition;

        public Swapchain MainSwapchain => Device.MainSwapchain;

        public uint FrameIndex => MainSwapchain.LastAcquiredImage;

        public uint BufferCount => MainSwapchain.BufferCount;

        public bool SupportsImGui => false;

        public event Action Resized;
        public event Action<double> Update;
        public event Func<double, CommandBuffer[]> Render;

        public void Attach(RenderHandle renderHandle)
        {
            _renderHandle = renderHandle;

            JSObject canvas = JSHost.GlobalThis.GetPropertyAsJSObject("window")!.GetPropertyAsJSObject("canvas")!;
            uint width = (uint)canvas.GetPropertyAsInt32("width");
            uint height = (uint)canvas.GetPropertyAsInt32("height");

            Device = GraphicsDevice.CreateOpenGLES(
                GraphicsDeviceOptions.Recommended_4_7_0,
                new SwapchainDescription(SwapchainSource.CreateHtml5Canvas(canvas), width, height, PixelFormat.R32_Float, true));

            GraphicsDeviceOptions options = Gallery.GetPreferredOptions();
            _loop = new Action<double>(Loop);
            _frameloop = new AdvancedFrameLoop(Device, Device.MainSwapchain);

            AddEventListener("mousemove", new Action<JSObject>((mouseEvent) =>
            {
                int mX = mouseEvent.GetPropertyAsInt32("clientX");
                int mY = mouseEvent.GetPropertyAsInt32("clientY");
                _inputState.MousePosition = new Vector2(mX, mY);

                mouseEvent.Dispose();
            }), false);
            AddEventListener("mousedown", new Action<JSObject>((mouseEvent) =>
            {
                int button = mouseEvent.GetPropertyAsInt32("button");
                _inputState.MouseDown[button] = true;

                mouseEvent.Dispose();
            }), false);
            AddEventListener("mouseup", new Action<JSObject>((mouseEvent) =>
            {
                int button = mouseEvent.GetPropertyAsInt32("button");
                _inputState.MouseDown[button] = false;

                mouseEvent.Dispose();
            }), false);

            Gallery gallery = new Gallery(this);
            gallery.RegisterExample("Simple Mesh Render", () => new SimpleMeshRender());
            gallery.RegisterExample("Snake", () => new SnakeExample());
            gallery.LoadExample("Simple Mesh Render");
            //gallery.LoadExample("Snake");

            RequestAnimationFrame(_loop);

            AddEventListener("keydown", new Action<JSObject>((keyEvent) =>
            {
                string keyStr = keyEvent.GetPropertyAsString("code")!;
                if (TryParseKeyCode(keyStr, out Key key))
                {
                    _inputState.KeyEvents.Add(new KeyEvent(key, true, ModifierKeys.None));
                }

                keyEvent.Dispose();
            }), false);

            AddEventListener("keyup", new Action<JSObject>((keyEvent) =>
            {
                string keyStr = keyEvent.GetPropertyAsString("code")!;
                if (TryParseKeyCode(keyStr, out Key key))
                {
                    _inputState.KeyEvents.Add(new KeyEvent(key, false, ModifierKeys.None));
                }

                keyEvent.Dispose();
            }), false);
        }

        public Task SetParametersAsync(ParameterView parameters)
        {
            return Task.CompletedTask;
        }

        [JSImport("globalThis.document.addEventListener")]
        private static partial void AddEventListener(string type, [JSMarshalAs<JSType.Function<JSType.Object>>] Action<JSObject> callback, bool capture);

        [JSImport("globalThis.window.requestAnimationFrame")]
        private static partial void RequestAnimationFrame([JSMarshalAs<JSType.Function<JSType.Number>>] Action<double> callback);

        private bool TryParseKeyCode(string keyStr, out Key key)
        {
            if (Enum.TryParse(keyStr, ignoreCase: true, out key)) { return true; }
            if (keyStr.StartsWith("Key") && Enum.TryParse(keyStr.Substring(3), ignoreCase: true, out key)) { return true; }

            switch (keyStr)
            {
                case "ArrowLeft":
                    key = Key.Left; return true;
                case "ArrowRight":
                    key = Key.Right; return true;
                case "ArrowUp":
                    key = Key.Up; return true;
                case "ArrowDown":
                    key = Key.Down; return true;
                case " ":
                    key = Key.Space; return true;
                case "Shift":
                    key = Key.ShiftLeft; return true;
                case "Control":
                    key = Key.ControlLeft; return true;

                default:
                    Console.WriteLine($"Couldn't parse key string: {keyStr}");
                    return false;
            }
        }

        public InputStateView GetInputState() => _inputState.View;

        private void Loop(double milliseconds)
        {
            _deltaSeconds = (milliseconds - _previousMilliseconds) / 1000;
            _previousMilliseconds = milliseconds;
            FlushInput();
            Update?.Invoke(_deltaSeconds);
            _frameloop.RunFrame(HandleFrame);

            RequestAnimationFrame(_loop);
        }

        private void FlushInput()
        {
            _inputState.MouseDelta = _inputState.MousePosition - _previousMousePosition;
            _previousMousePosition = _inputState.MousePosition;
        }

        private CommandBuffer[] HandleFrame(uint frameIndex, Framebuffer fb)
        {
            return Render?.Invoke(_deltaSeconds);
        }
    }
}
