using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Veldrid.Sdl2.Sdl2Native;

namespace Veldrid.Sdl2
{
    public delegate void SDLEventHandler(ref SDL_Event ev);

    public unsafe class Sdl2Window
    {
        public delegate void DropFileAction(DropFileEvent file);
        public delegate void DropTextAction(DropTextEvent text);
        public delegate void TextInputAction(TextInputEvent textInput);
        public delegate void TextEditingAction(TextEditingEvent textEditing);

        private readonly List<SDL_Event> _events = new();
        private IntPtr _window;
        public uint WindowID { get; private set; }
        private bool _exists;

        private SimpleInputSnapshot _publicSnapshot = new();
        private SimpleInputSnapshot _privateSnapshot = new();
        private SimpleInputSnapshot _privateBackbuffer = new();

        // Threaded Sdl2Window flags
        private readonly bool _threadedProcessing;

        private bool _shouldClose;
        public bool LimitPollRate { get; set; }
        public float PollIntervalInMs { get; set; }

        // Current input states
        private int _currentMouseX;
        private int _currentMouseY;
        private MouseButton _currentMouseDown;
        private Vector2 _currentMouseDelta;

        // Cached Sdl2Window state (for threaded processing)
        private BufferedValue<Point> _cachedPosition = new();
        private BufferedValue<Point> _cachedSize = new();
        private string? _cachedWindowTitle;
        private bool _newWindowTitleReceived;
        private bool _firstMouseEvent = true;
        private Func<bool>? _closeRequestedHandler;

        public Sdl2Window(string title, int x, int y, int width, int height, SDL_WindowFlags flags, bool threadedProcessing)
        {
            SDL_SetHint("SDL_MOUSE_FOCUS_CLICKTHROUGH", "1");
            _threadedProcessing = threadedProcessing;
            if (threadedProcessing)
            {
                using ManualResetEvent mre = new(false);
                WindowParams wp = new()
                {
                    Title = title,
                    X = x,
                    Y = y,
                    Width = width,
                    Height = height,
                    WindowFlags = flags,
                    ResetEvent = mre
                };

                Task.Factory.StartNew(WindowOwnerRoutine, wp, TaskCreationOptions.LongRunning);
                mre.WaitOne();
            }
            else
            {
                _window = SDL_CreateWindow(title, x, y, width, height, flags);
                WindowID = SDL_GetWindowID(_window);
                Sdl2WindowRegistry.RegisterWindow(this);
                PostWindowCreated(flags);
            }
        }

        public Sdl2Window(IntPtr windowHandle, bool threadedProcessing)
        {
            _threadedProcessing = threadedProcessing;
            if (threadedProcessing)
            {
                using ManualResetEvent mre = new(false);
                WindowParams wp = new()
                {
                    WindowHandle = windowHandle,
                    WindowFlags = 0,
                    ResetEvent = mre
                };

                Task.Factory.StartNew(WindowOwnerRoutine, wp, TaskCreationOptions.LongRunning);
                mre.WaitOne();
            }
            else
            {
                _window = SDL_CreateWindowFrom(windowHandle);
                WindowID = SDL_GetWindowID(_window);
                Sdl2WindowRegistry.RegisterWindow(this);
                PostWindowCreated(0);
            }
        }

        public int X { get => _cachedPosition.Value.X; set => SetWindowPosition(value, Y); }
        public int Y { get => _cachedPosition.Value.Y; set => SetWindowPosition(X, value); }

        public int Width { get => GetWindowSize().X; set => SetWindowSize(value, Height); }
        public int Height { get => GetWindowSize().Y; set => SetWindowSize(Width, value); }

        public IntPtr Handle => GetUnderlyingWindowHandle();

        public string? Title { get => _cachedWindowTitle; set => SetWindowTitle(value); }

        private void SetWindowTitle(string? value)
        {
            _cachedWindowTitle = value;
            _newWindowTitleReceived = true;
        }

        public WindowState WindowState
        {
            get
            {
                SDL_WindowFlags flags = SDL_GetWindowFlags(_window);
                if (((flags & SDL_WindowFlags.FullScreenDesktop) == SDL_WindowFlags.FullScreenDesktop)
                    || ((flags & (SDL_WindowFlags.Borderless | SDL_WindowFlags.Fullscreen)) == (SDL_WindowFlags.Borderless | SDL_WindowFlags.Fullscreen)))
                {
                    return WindowState.BorderlessFullScreen;
                }
                else if ((flags & SDL_WindowFlags.Minimized) == SDL_WindowFlags.Minimized)
                {
                    return WindowState.Minimized;
                }
                else if ((flags & SDL_WindowFlags.Fullscreen) == SDL_WindowFlags.Fullscreen)
                {
                    return WindowState.FullScreen;
                }
                else if ((flags & SDL_WindowFlags.Maximized) == SDL_WindowFlags.Maximized)
                {
                    return WindowState.Maximized;
                }
                else if ((flags & SDL_WindowFlags.Hidden) == SDL_WindowFlags.Hidden)
                {
                    return WindowState.Hidden;
                }

                return WindowState.Normal;
            }
            set
            {
                switch (value)
                {
                    case WindowState.Normal:
                        SDL_SetWindowFullscreen(_window, SDL_FullscreenMode.Windowed);
                        break;
                    case WindowState.FullScreen:
                        SDL_SetWindowFullscreen(_window, SDL_FullscreenMode.Fullscreen);
                        break;
                    case WindowState.Maximized:
                        SDL_MaximizeWindow(_window);
                        break;
                    case WindowState.Minimized:
                        SDL_MinimizeWindow(_window);
                        break;
                    case WindowState.BorderlessFullScreen:
                        SDL_SetWindowFullscreen(_window, SDL_FullscreenMode.FullScreenDesktop);
                        break;
                    case WindowState.Hidden:
                        SDL_HideWindow(_window);
                        break;
                    default:
                        throw new InvalidOperationException("Illegal WindowState value: " + value);
                }
            }
        }

        public bool Exists => _exists;

        public bool Visible
        {
            get => (SDL_GetWindowFlags(_window) & SDL_WindowFlags.Shown) != 0;
            set
            {
                if (value)
                {
                    SDL_ShowWindow(_window);
                }
                else
                {
                    SDL_HideWindow(_window);
                }
            }
        }

        public Vector2 ScaleFactor => Vector2.One;

        public Rectangle Bounds => new(_cachedPosition, GetWindowSize());

        public bool CursorVisible
        {
            get
            {
                return SDL_ShowCursor(SDL_QUERY) == 1;
            }
            set
            {
                int toggle = value ? SDL_ENABLE : SDL_DISABLE;
                SDL_ShowCursor(toggle);
            }
        }

        public float Opacity
        {
            get
            {
                float opacity = float.NaN;
                if (SDL_GetWindowOpacity(_window, &opacity) == 0)
                {
                    return opacity;
                }
                return float.NaN;
            }
            set
            {
                SDL_SetWindowOpacity(_window, value);
            }
        }

        public bool Focused => (SDL_GetWindowFlags(_window) & SDL_WindowFlags.InputFocus) != 0;

        public bool Resizable
        {
            get => (SDL_GetWindowFlags(_window) & SDL_WindowFlags.Resizable) != 0;
            set => SDL_SetWindowResizable(_window, value ? 1u : 0u);
        }

        public bool BorderVisible
        {
            get => (SDL_GetWindowFlags(_window) & SDL_WindowFlags.Borderless) == 0;
            set => SDL_SetWindowBordered(_window, value ? 1u : 0u);
        }

        public IntPtr SdlWindowHandle => _window;

        public event Action? Resized;
        public event Action? Closing;
        public event Action? Closed;
        public event Action? FocusLost;
        public event Action? FocusGained;
        public event Action? Shown;
        public event Action? Hidden;
        public event Action? MouseEntered;
        public event Action? MouseLeft;
        public event Action? Exposed;
        public event Action? KeyMapChanged;
        public event Action<Point>? Moved;
        public event Action<MouseWheelEvent>? MouseWheel;
        public event Action<MouseMoveEvent>? MouseMove;
        public event Action<MouseButtonEvent>? MouseDown;
        public event Action<MouseButtonEvent>? MouseUp;
        public event Action<KeyEvent>? KeyDown;
        public event Action<KeyEvent>? KeyUp;
        public event TextInputAction? TextInput;
        public event TextEditingAction? TextEditing;
        public event Action? DropBegin;
        public event Action? DropComplete;
        public event DropFileAction? DropFile;
        public event DropTextAction? DropText;

        public Point ClientToScreen(Point p)
        {
            Point position = _cachedPosition;
            return new Point(p.X + position.X, p.Y + position.Y);
        }

        public void SetMousePosition(Vector2 position) => SetMousePosition((int)position.X, (int)position.Y);
        public void SetMousePosition(int x, int y)
        {
            if (_exists)
            {
                SDL_WarpMouseInWindow(_window, x, y);
                _currentMouseX = x;
                _currentMouseY = y;
            }
        }

        public Vector2 MouseDelta => _currentMouseDelta;

        public void SetCloseRequestedHandler(Func<bool>? handler)
        {
            _closeRequestedHandler = handler;
        }

        public void Close()
        {
            if (_threadedProcessing)
            {
                _shouldClose = true;
            }
            else
            {
                CloseCore();
            }
        }

        private bool CloseCore()
        {
            if (_closeRequestedHandler?.Invoke() ?? false)
            {
                _shouldClose = false;
                return false;
            }

            Sdl2WindowRegistry.RemoveWindow(this);
            Closing?.Invoke();
            SDL_DestroyWindow(_window);
            _exists = false;
            Closed?.Invoke();

            return true;
        }

        private void WindowOwnerRoutine(object? state)
        {
            WindowParams wp = (WindowParams)state!;
            _window = wp.Create();
            WindowID = SDL_GetWindowID(_window);
            Sdl2WindowRegistry.RegisterWindow(this);
            PostWindowCreated(wp.WindowFlags);
            wp.ResetEvent!.Set();

            double previousPollTimeMs = 0;
            Stopwatch sw = new();
            sw.Start();

            while (_exists)
            {
                if (_shouldClose && CloseCore())
                {
                    return;
                }

                double currentTick = sw.ElapsedTicks;
                double currentTimeMs = sw.ElapsedTicks * (1000.0 / Stopwatch.Frequency);
                if (LimitPollRate && currentTimeMs - previousPollTimeMs < PollIntervalInMs)
                {
                    Thread.Sleep(0);
                }
                else
                {
                    previousPollTimeMs = currentTimeMs;
                    ProcessEvents(null);
                }
            }
        }

        private void PostWindowCreated(SDL_WindowFlags flags)
        {
            RefreshCachedPosition();
            RefreshCachedSize();
            if ((flags & SDL_WindowFlags.Shown) == SDL_WindowFlags.Shown)
            {
                SDL_ShowWindow(_window);
            }

            _exists = true;
        }

        // Called by Sdl2EventProcessor when an event for this window is encountered.
        internal void AddEvent(SDL_Event ev)
        {
            _events.Add(ev);
        }

        public InputSnapshot PumpEvents()
        {
            _currentMouseDelta = new Vector2();
            if (_threadedProcessing)
            {
                SimpleInputSnapshot snapshot = Interlocked.Exchange(ref _privateSnapshot, _privateBackbuffer);
                snapshot.CopyTo(_publicSnapshot);
                snapshot.Clear();
            }
            else
            {
                ProcessEvents(null);
                _privateSnapshot.CopyTo(_publicSnapshot);
                _privateSnapshot.Clear();
            }

            return _publicSnapshot;
        }

        private void ProcessEvents(SDLEventHandler? eventHandler)
        {
            CheckNewWindowTitle();

            Sdl2Events.ProcessEvents();
            Span<SDL_Event> events = CollectionsMarshal.AsSpan(_events);
            for (int i = 0; i < events.Length; i++)
            {
                ref SDL_Event ev = ref events[i];
                if (eventHandler == null)
                {
                    HandleEvent(ref ev);
                }
                else
                {
                    eventHandler(ref ev);
                }
            }
            _events.Clear();
        }

        public void PumpEvents(SDLEventHandler? eventHandler)
        {
            ProcessEvents(eventHandler);
        }

        private unsafe void HandleEvent(ref SDL_Event ev)
        {
            switch (ev.type)
            {
                case SDL_EventType.Quit:
                    Close();
                    break;
                case SDL_EventType.Terminating:
                    Close();
                    break;
                case SDL_EventType.WindowEvent:
                    SDL_WindowEvent windowEvent = Unsafe.As<SDL_Event, SDL_WindowEvent>(ref ev);
                    HandleWindowEvent(windowEvent);
                    break;
                case SDL_EventType.KeyDown:
                case SDL_EventType.KeyUp:
                    SDL_KeyboardEvent keyboardEvent = Unsafe.As<SDL_Event, SDL_KeyboardEvent>(ref ev);
                    HandleKeyboardEvent(keyboardEvent);
                    break;
                case SDL_EventType.TextEditing:
                    SDL_TextEditingEvent textEditingEvent = Unsafe.As<SDL_Event, SDL_TextEditingEvent>(ref ev);
                    HandleTextEditingEvent(textEditingEvent);
                    break;
                case SDL_EventType.TextInput:
                    SDL_TextInputEvent textInputEvent = Unsafe.As<SDL_Event, SDL_TextInputEvent>(ref ev);
                    HandleTextInputEvent(textInputEvent);
                    break;
                case SDL_EventType.KeyMapChanged:
                    KeyMapChanged?.Invoke();
                    break;
                case SDL_EventType.MouseMotion:
                    SDL_MouseMotionEvent mouseMotionEvent = Unsafe.As<SDL_Event, SDL_MouseMotionEvent>(ref ev);
                    HandleMouseMotionEvent(mouseMotionEvent);
                    break;
                case SDL_EventType.MouseButtonDown:
                case SDL_EventType.MouseButtonUp:
                    SDL_MouseButtonEvent mouseButtonEvent = Unsafe.As<SDL_Event, SDL_MouseButtonEvent>(ref ev);
                    HandleMouseButtonEvent(mouseButtonEvent);
                    break;
                case SDL_EventType.MouseWheel:
                    SDL_MouseWheelEvent mouseWheelEvent = Unsafe.As<SDL_Event, SDL_MouseWheelEvent>(ref ev);
                    HandleMouseWheelEvent(mouseWheelEvent);
                    break;
                case SDL_EventType.DropBegin:
                    DropBegin?.Invoke();
                    break;
                case SDL_EventType.DropComplete:
                    DropComplete?.Invoke();
                    break;
                case SDL_EventType.DropFile:
                case SDL_EventType.DropText:
                    SDL_DropEvent dropEvent = Unsafe.As<SDL_Event, SDL_DropEvent>(ref ev);
                    HandleDropEvent(dropEvent);
                    break;
                default:
                    // Ignore
                    break;
            }
        }

        private void CheckNewWindowTitle()
        {
            if (_newWindowTitleReceived)
            {
                _newWindowTitleReceived = false;
                SDL_SetWindowTitle(_window, _cachedWindowTitle);
            }
        }

        private static int ParseTextEvent(ReadOnlySpan<byte> utf8, Span<Rune> runes)
        {
            int byteCount = utf8.IndexOf((byte)0);
            if (byteCount != -1)
                utf8 = utf8[..byteCount];

            int runeCount = 0;
            while (Rune.DecodeFromUtf8(utf8, out Rune rune, out int consumed) == OperationStatus.Done)
            {
                runes[runeCount++] = rune;
                utf8 = utf8[consumed..];
            }

            return runeCount;
        }

        private void HandleTextInputEvent(SDL_TextInputEvent textInputEvent)
        {
            ReadOnlySpan<byte> utf8 = new(textInputEvent.text, SDL_TextInputEvent.MaxTextSize);
            Span<Rune> runes = stackalloc Rune[SDL_TextInputEvent.MaxTextSize];
            runes = runes[..ParseTextEvent(utf8, runes)];

            SimpleInputSnapshot snapshot = _privateSnapshot;
            for (int i = 0; i < runes.Length; i++)
            {
                snapshot.InputEvents.Add(runes[i]);
            }

            TextInputEvent inputEvent = new(
                textInputEvent.timestamp,
                textInputEvent.windowID,
                runes);
            TextInput?.Invoke(inputEvent);
        }

        private void HandleTextEditingEvent(SDL_TextEditingEvent textEditingEvent)
        {
            ReadOnlySpan<byte> utf8 = new(textEditingEvent.text, SDL_TextEditingEvent.MaxTextSize);
            Span<Rune> runes = stackalloc Rune[SDL_TextEditingEvent.MaxTextSize];
            runes = runes[..ParseTextEvent(utf8, runes)];

            TextEditingEvent editingEvent = new(
                textEditingEvent.timestamp,
                textEditingEvent.windowID,
                runes,
                textEditingEvent.start,
                textEditingEvent.length);
            TextEditing?.Invoke(editingEvent);
        }

        private void HandleMouseWheelEvent(SDL_MouseWheelEvent mouseWheelEvent)
        {
            Vector2 delta = new(mouseWheelEvent.x, mouseWheelEvent.y);

            SimpleInputSnapshot snapshot = _privateSnapshot;
            snapshot.WheelDelta += delta;

            MouseWheelEvent wheelEvent = new(
                mouseWheelEvent.timestamp,
                mouseWheelEvent.windowID,
                delta);
            MouseWheel?.Invoke(wheelEvent);
        }

        private void HandleDropEvent(SDL_DropEvent dropEvent)
        {
            if (dropEvent.file != null)
            {
                int characters = 0;
                while (dropEvent.file[characters] != 0)
                {
                    characters++;
                }

                ReadOnlySpan<byte> utf8 = new(dropEvent.file, characters);
                try
                {
                    if (dropEvent.type == SDL_EventType.DropFile)
                    {
                        DropFile?.Invoke(new DropFileEvent(utf8, dropEvent.timestamp, dropEvent.windowID));
                    }
                    else if (dropEvent.type == SDL_EventType.DropText)
                    {
                        DropText?.Invoke(new DropTextEvent(utf8, dropEvent.timestamp, dropEvent.windowID));
                    }
                }
                finally
                {
                    SDL_free(dropEvent.file);
                }
            }
        }

        private void HandleMouseButtonEvent(SDL_MouseButtonEvent mouseButtonEvent)
        {
            MouseButton button = MapMouseButton(mouseButtonEvent.button);
            bool down = mouseButtonEvent.state == 1;

            SimpleInputSnapshot snapshot = _privateSnapshot;
            if (down)
            {
                _currentMouseDown |= button;
                snapshot.MouseDown |= button;
            }
            else
            {
                _currentMouseDown &= ~button;
                snapshot.MouseDown &= ~button;
            }

            MouseButtonEvent mouseEvent = new(
                mouseButtonEvent.timestamp,
                mouseButtonEvent.windowID,
                button,
                down,
                mouseButtonEvent.clicks);
            snapshot.MouseEvents.Add(mouseEvent);

            if (down)
            {
                MouseDown?.Invoke(mouseEvent);
            }
            else
            {
                MouseUp?.Invoke(mouseEvent);
            }
        }

        private static MouseButton MapMouseButton(SDL_MouseButton button)
        {
            return button switch
            {
                SDL_MouseButton.Left => MouseButton.Left,
                SDL_MouseButton.Middle => MouseButton.Middle,
                SDL_MouseButton.Right => MouseButton.Right,
                SDL_MouseButton.X1 => MouseButton.Button1,
                SDL_MouseButton.X2 => MouseButton.Button2,
                _ => MouseButton.Left,
            };
        }

        private void HandleMouseMotionEvent(SDL_MouseMotionEvent mouseMotionEvent)
        {
            Vector2 mousePos = new(mouseMotionEvent.x, mouseMotionEvent.y);
            Vector2 delta = new(mouseMotionEvent.xrel, mouseMotionEvent.yrel);
            _currentMouseX = (int)mousePos.X;
            _currentMouseY = (int)mousePos.Y;
            _privateSnapshot.MousePosition = mousePos;

            if (!_firstMouseEvent)
            {
                _currentMouseDelta += delta;

                MouseMoveEvent motionEvent = new(
                    mouseMotionEvent.timestamp,
                    mouseMotionEvent.windowID,
                    mousePos,
                    delta);
                MouseMove?.Invoke(motionEvent);
            }
            _firstMouseEvent = false;
        }

        private void HandleKeyboardEvent(SDL_KeyboardEvent keyboardEvent)
        {
            KeyEvent keyEvent = new(
                keyboardEvent.timestamp,
                keyboardEvent.windowID,
                keyboardEvent.state == 1,
                keyboardEvent.repeat == 1,
                (Key)keyboardEvent.keysym.scancode,
                (VKey)keyboardEvent.keysym.sym,
                (ModifierKeys)keyboardEvent.keysym.mod);

            _privateSnapshot.KeyEvents.Add(keyEvent);
            if (keyboardEvent.state == 1)
            {
                KeyDown?.Invoke(keyEvent);
            }
            else
            {
                KeyUp?.Invoke(keyEvent);
            }
        }

        private void HandleWindowEvent(SDL_WindowEvent windowEvent)
        {
            switch (windowEvent.@event)
            {
                case SDL_WindowEventID.Resized:
                case SDL_WindowEventID.SizeChanged:
                case SDL_WindowEventID.Minimized:
                case SDL_WindowEventID.Maximized:
                case SDL_WindowEventID.Restored:
                    HandleResizedMessage();
                    break;
                case SDL_WindowEventID.FocusGained:
                    FocusGained?.Invoke();
                    break;
                case SDL_WindowEventID.FocusLost:
                    FocusLost?.Invoke();
                    break;
                case SDL_WindowEventID.Close:
                    Close();
                    break;
                case SDL_WindowEventID.Shown:
                    Shown?.Invoke();
                    break;
                case SDL_WindowEventID.Hidden:
                    Hidden?.Invoke();
                    break;
                case SDL_WindowEventID.Enter:
                    MouseEntered?.Invoke();
                    break;
                case SDL_WindowEventID.Leave:
                    MouseLeft?.Invoke();
                    break;
                case SDL_WindowEventID.Exposed:
                    Exposed?.Invoke();
                    break;
                case SDL_WindowEventID.Moved:
                    _cachedPosition.Value = new Point(windowEvent.data1, windowEvent.data2);
                    Moved?.Invoke(new Point(windowEvent.data1, windowEvent.data2));
                    break;
                default:
                    Debug.WriteLine("Unhandled SDL WindowEvent: " + windowEvent.@event);
                    break;
            }
        }

        private void HandleResizedMessage()
        {
            RefreshCachedSize();
            Resized?.Invoke();
        }

        private void RefreshCachedSize()
        {
            int w, h;
            SDL_GetWindowSize(_window, &w, &h);
            _cachedSize.Value = new Point(w, h);
        }

        private void RefreshCachedPosition()
        {
            int x, y;
            SDL_GetWindowPosition(_window, &x, &y);
            _cachedPosition.Value = new Point(x, y);
        }

        private MouseState GetCurrentMouseState()
        {
            return new MouseState(_currentMouseX, _currentMouseY, _currentMouseDown);
        }

        public Point ScreenToClient(Point p)
        {
            Point position = _cachedPosition;
            return new Point(p.X - position.X, p.Y - position.Y);
        }

        private void SetWindowPosition(int x, int y)
        {
            SDL_SetWindowPosition(_window, x, y);
            _cachedPosition.Value = new Point(x, y);
        }

        private Point GetWindowSize()
        {
            return _cachedSize;
        }

        private void SetWindowSize(int width, int height)
        {
            SDL_SetWindowSize(_window, width, height);
            _cachedSize.Value = new Point(width, height);
        }

        private IntPtr GetUnderlyingWindowHandle()
        {
            SDL_SysWMinfo wmInfo;
            SDL_GetVersion(&wmInfo.version);
            SDL_GetWMWindowInfo(_window, &wmInfo);
            switch (wmInfo.subsystem)
            {
                case SysWMType.Windows:
                    Win32WindowInfo win32Info = Unsafe.Read<Win32WindowInfo>(&wmInfo.info);
                    return win32Info.Sdl2Window;
                case SysWMType.X11:
                    X11WindowInfo x11Info = Unsafe.Read<X11WindowInfo>(&wmInfo.info);
                    return x11Info.Sdl2Window;
                case SysWMType.Wayland:
                    WaylandWindowInfo waylandInfo = Unsafe.Read<WaylandWindowInfo>(&wmInfo.info);
                    return waylandInfo.surface;
                case SysWMType.Cocoa:
                    CocoaWindowInfo cocoaInfo = Unsafe.Read<CocoaWindowInfo>(&wmInfo.info);
                    return cocoaInfo.Window;
                case SysWMType.Android:
                    AndroidWindowInfo androidInfo = Unsafe.Read<AndroidWindowInfo>(&wmInfo.info);
                    return androidInfo.window;
                default:
                    return _window;
            }
        }

        private class SimpleInputSnapshot : InputSnapshot
        {
            public List<Rune> InputEvents { get; private set; } = new List<Rune>();
            public List<KeyEvent> KeyEvents { get; private set; } = new List<KeyEvent>();
            public List<MouseButtonEvent> MouseEvents { get; private set; } = new List<MouseButtonEvent>();

            public Vector2 MousePosition { get; set; }
            public Vector2 WheelDelta { get; set; }
            public MouseButton MouseDown { get; set; }

            ReadOnlySpan<Rune> InputSnapshot.InputEvents => CollectionsMarshal.AsSpan(InputEvents);
            ReadOnlySpan<KeyEvent> InputSnapshot.KeyEvents => CollectionsMarshal.AsSpan(KeyEvents);
            ReadOnlySpan<MouseButtonEvent> InputSnapshot.MouseEvents => CollectionsMarshal.AsSpan(MouseEvents);

            internal void Clear()
            {
                InputEvents.Clear();
                KeyEvents.Clear();
                MouseEvents.Clear();
                WheelDelta = Vector2.Zero;
            }

            public void CopyTo(SimpleInputSnapshot other)
            {
                Debug.Assert(this != other);

                other.InputEvents.Clear();
                other.InputEvents.AddRange(InputEvents);

                other.MouseEvents.Clear();
                other.MouseEvents.AddRange(MouseEvents);

                other.KeyEvents.Clear();
                other.KeyEvents.AddRange(KeyEvents);

                other.MousePosition = MousePosition;
                other.WheelDelta = WheelDelta;
                other.MouseDown = MouseDown;
            }
        }

        private class WindowParams
        {
            public int X { get; set; }
            public int Y { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }
            public string? Title { get; set; }
            public SDL_WindowFlags WindowFlags { get; set; }

            public IntPtr WindowHandle { get; set; }

            public ManualResetEvent? ResetEvent { get; set; }

            public SDL_Window Create()
            {
                if (WindowHandle != IntPtr.Zero)
                {
                    return SDL_CreateWindowFrom(WindowHandle);
                }
                else
                {
                    return SDL_CreateWindow(Title, X, Y, Width, Height, WindowFlags);
                }
            }
        }
    }

    [DebuggerDisplay("{DebuggerDisplayString,nq}")]
    public class BufferedValue<T> where T : struct
    {
        public T Value
        {
            get => Current.Value;
            set
            {
                Back.Value = value;
                Back = Interlocked.Exchange(ref Current, Back);
            }
        }

        private ValueHolder Current = new();
        private ValueHolder Back = new();

        public static implicit operator T(BufferedValue<T> bv) => bv.Value;

        private string DebuggerDisplayString => $"{Current.Value}";

        private class ValueHolder
        {
            public T Value;
        }
    }
}
