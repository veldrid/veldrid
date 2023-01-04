using ImGuiNET;
using Instancing;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Snake;
using System.Numerics;
using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;
using Vulkan.Xcb;
using Vulkan.Xlib;

[assembly: SupportedOSPlatform("browser")]

namespace Veldrid.SampleGallery
{
    public partial class WebGalleryDriver : IComponent, IGalleryDriver
    {
        private RenderHandle _renderHandle;
        private JSObject _window;
        private JSObject _canvas;
        private Action<double> _loop;
        private double _deltaSeconds;
        private double _previousMilliseconds;
        private AdvancedFrameLoop _frameloop;

        private uint _width;
        private uint _height;
        private InputState _inputState = new InputState();

        public uint Width => (uint)_canvas.GetPropertyAsInt32("clientWidth");
        public uint Height => (uint)_canvas.GetPropertyAsInt32("clientHeight");

        public GraphicsDevice Device { get; set; }

        private Vector2 _previousMousePosition;
        private Gallery _gallery;

        public Swapchain MainSwapchain => Device.MainSwapchain;

        public uint FrameIndex => MainSwapchain.LastAcquiredImage;

        public uint BufferCount => MainSwapchain.BufferCount;

        public bool SupportsImGui => true;

        public event Action Resized;
        public event Action<double> Update;
        public event Func<double, CommandBuffer[]> Render;

        public void Attach(RenderHandle renderHandle)
        {
            _renderHandle = renderHandle;

            _window = JSHost.GlobalThis.GetPropertyAsJSObject("window")!;
            _canvas = _window.GetPropertyAsJSObject("canvas")!;

            Device = GraphicsDevice.CreateOpenGLES(
                GraphicsDeviceOptions.Recommended_4_7_0,
                new SwapchainDescription(SwapchainSource.CreateHtml5Canvas(_canvas), Width, Height, PixelFormat.R32_Float, true));

            GraphicsDeviceOptions options = Gallery.GetPreferredOptions();
            _loop = new Action<double>(Loop);
            _frameloop = new AdvancedFrameLoop(Device, Device.MainSwapchain);

            AddEventListener("mousemove", new Action<JSObject>((mouseEvent) =>
            {
                JSObject canvasRect = GetElementBoundingClientRect(_canvas);
                int canvasLeft = (int)canvasRect.GetPropertyAsDouble("left");
                int canvasTop = (int)canvasRect.GetPropertyAsDouble("left");
                int mX = (int)mouseEvent.GetPropertyAsDouble("clientX");
                int mY = (int)mouseEvent.GetPropertyAsDouble("clientY");
                int scrollY = (int)_window.GetPropertyAsDouble("scrollY");
                _inputState.MousePosition = new Vector2(mX - canvasLeft, mY - canvasTop + scrollY);
                if (InputTracker.WantCaptureMouse) { EventPreventDefault(mouseEvent); }

                mouseEvent.Dispose();
            }), true);
            AddEventListener("mousedown", new Action<JSObject>((mouseEvent) =>
            {
                int button = mouseEvent.GetPropertyAsInt32("button");
                _inputState.MouseDown[button] = true;
                if (InputTracker.WantCaptureMouse) { EventPreventDefault(mouseEvent); }

                mouseEvent.Dispose();
            }), true);
            AddEventListener("mouseup", new Action<JSObject>((mouseEvent) =>
            {
                int button = mouseEvent.GetPropertyAsInt32("button");
                _inputState.MouseDown[button] = false;
                if (InputTracker.WantCaptureMouse) { EventPreventDefault(mouseEvent); }

                mouseEvent.Dispose();
            }), true);
            AddEventListener("keydown", new Action<JSObject>((keyEvent) =>
            {
                string keyStr = keyEvent.GetPropertyAsString("code")!;
                if (TryParseKeyCode(keyStr, out Key key))
                {
                    _inputState.KeyEvents.Add(new KeyEvent(key, true, ModifierKeys.None));
                    if (InputTracker.WantCaptureKeyboard) { EventPreventDefault(keyEvent); }
                }
                keyEvent.Dispose();
            }), true);
            AddEventListener("keyup", new Action<JSObject>((keyEvent) =>
            {
                string keyStr = keyEvent.GetPropertyAsString("code")!;
                if (TryParseKeyCode(keyStr, out Key key))
                {
                    _inputState.KeyEvents.Add(new KeyEvent(key, false, ModifierKeys.None));
                    if (InputTracker.WantCaptureKeyboard) { EventPreventDefault(keyEvent); }
                }

                keyEvent.Dispose();
            }), true);

            _gallery = new Gallery(this);
            _gallery.RegisterExample("Simple Mesh Render", () => new SimpleMeshRender());
            _gallery.RegisterExample("Snake", () => new SnakeExample());
            _gallery.RegisterExample("Instancing", () => new InstancingExample());
            _gallery.LoadExample("Instancing");

            RequestAnimationFrame(_loop);
            ImGui.GetStyle().ScaleAllSizes(2.0f);
        }

        public Task SetParametersAsync(ParameterView parameters)
        {
            return Task.CompletedTask;
        }

        [JSImport("globalThis.document.addEventListener")]
        private static partial void AddEventListener(string type, [JSMarshalAs<JSType.Function<JSType.Object>>] Action<JSObject> callback, bool capture);

        [JSImport("globalThis.window.requestAnimationFrame")]
        private static partial void RequestAnimationFrame([JSMarshalAs<JSType.Function<JSType.Number>>] Action<double> callback);

        [JSImport("globalThis.window.getElementBoundingClientRect")]
        private static partial JSObject GetElementBoundingClientRect([JSMarshalAs<JSType.Object>] JSObject element);

        [JSImport("globalThis.window.eventPreventDefault")]
        private static partial JSObject EventPreventDefault([JSMarshalAs<JSType.Object>] JSObject ev);

        [JSImport("globalThis.window.elementRequestFullscreen")]
        private static partial JSObject ElementRequestFullscreen([JSMarshalAs<JSType.Object>] JSObject element);

        [JSImport("globalThis.window.documentExitFullscreen")]
        private static partial JSObject DocumentExitFullscreen();

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
                case "Digit1":
                    key = Key.Number1; return true;
                case "Digit2":
                    key = Key.Number2; return true;
                case "Digit3":
                    key = Key.Number3; return true;
                case "Digit4":
                    key = Key.Number4; return true;
                case "Digit5":
                    key = Key.Number5; return true;
                case "Digit6":
                    key = Key.Number6; return true;
                case "Digit7":
                    key = Key.Number7; return true;
                case "Digit8":
                    key = Key.Number8; return true;
                case "Digit9":
                    key = Key.Number9; return true;
                case "Digit0":
                    key = Key.Number0; return true;

                default:
                    Console.WriteLine($"Couldn't parse key string: {keyStr}");
                    return false;
            }
        }

        public InputStateView GetInputState() => _inputState.View;

        bool forceResize = false;
        private void ResizeCanvas()
        {
            int canvasWidth = _canvas.GetPropertyAsInt32("width");
            int canvasheight = _canvas.GetPropertyAsInt32("height");
            uint clientWidth = Width;
            uint clientHeight = Height;
            if (canvasWidth != clientWidth || canvasheight != clientHeight || forceResize)
            {
                forceResize = false;
                _canvas.SetProperty("width", clientWidth);
                _canvas.SetProperty("height", clientHeight);
                MainSwapchain.Resize(clientWidth, clientHeight);
                Resized?.Invoke();
            }
        }

        private bool _fullscreen = false;

        private void Loop(double milliseconds)
        {
            RequestAnimationFrame(_loop);

            _deltaSeconds = (milliseconds - _previousMilliseconds) / 1000;
            _previousMilliseconds = milliseconds;
            if (_deltaSeconds <= 0) { return; }

            FlushInput();
            Update?.Invoke(_deltaSeconds);
            ResizeCanvas();

            _frameloop.RunFrame(HandleFrame);
        }

        private void FlushInput()
        {
            _inputState.MouseDelta = _inputState.MousePosition - _previousMousePosition;
            _previousMousePosition = _inputState.MousePosition;
        }

        private CommandBuffer[] HandleFrame(uint frameIndex, Framebuffer fb)
        {
            return Render.Invoke(_deltaSeconds);
        }

        public void DrawMainMenuBars()
        {
            if (ImGui.BeginMenu("View"))
            {
                if (_fullscreen)
                {
                    if (ImGui.MenuItem("Exit Full Screen", "F11") || InputTracker.GetKeyDown(Key.F11))
                    {
                        _fullscreen = false;
                        DocumentExitFullscreen();
                    }
                }
                else
                {
                    if (ImGui.MenuItem("Enter Full Screen", "F11") || InputTracker.GetKeyDown(Key.F11))
                    {
                        _fullscreen = true;
                        ElementRequestFullscreen(_canvas);
                    }
                }

                ImGui.EndMenu();
            }
        }
    }
}
