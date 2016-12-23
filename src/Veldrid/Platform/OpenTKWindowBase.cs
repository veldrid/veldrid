using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using OpenTK.Platform;
using System;
using System.Collections.Generic;

namespace Veldrid.Platform
{
    public abstract class OpenTKWindowBase : Window, OpenTKWindow
    {
        private NativeWindow _nativeWindow;

        public event Action Resized;
        public event Action Closing;
        public event Action Closed;
        public event Action FocusGained;
        public event Action FocusLost;

        public System.Numerics.Vector2 ScaleFactor { get; private set; }

        protected SimpleInputSnapshot CurrentSnapshot = new SimpleInputSnapshot();

        private bool[] _mouseDown = new bool[13];
        private Size _previousSize;
        private Point _previousPosition;

        public OpenTKWindowBase()
            : this(960, 540, WindowState.Normal) { }

        public OpenTKWindowBase(int width, int height, WindowState initialState)
        {
            ConstructDefaultWindow(width, height, initialState);
        }

        protected virtual void ConstructDefaultWindow(int desiredWidth, int desiredHeight, WindowState windowState)
        {
            _nativeWindow = new NativeWindow(
                desiredWidth,
                desiredHeight,
                "Veldrid Render Window",
                 GameWindowFlags.Default,
                 new GraphicsMode(32, 24, 0, 8),
                 DisplayDevice.Default);
            int actualWidth = _nativeWindow.Width;
            int actualHeight = _nativeWindow.Height;

            ScaleFactor = new System.Numerics.Vector2((float)actualWidth / desiredWidth, (float)actualHeight / desiredHeight);

            var resolutions = DisplayDevice.Default.AvailableResolutions;
            DisplayResolution hiRes = null;
            foreach (var res in resolutions)
            {
                if (res.Width > 2200)
                {
                    hiRes = res;
                }
            }

            if (hiRes != null)
            {
                ScaleFactor = new System.Numerics.Vector2(2.0f, 2.0f);
            }

            _nativeWindow.X = 100;
            _nativeWindow.Y = 100;
            WindowState = windowState;
            _nativeWindow.Visible = true;

            _nativeWindow.Resize += OnWindowResized;
            _nativeWindow.KeyDown += OnKeyDown;
            _nativeWindow.KeyUp += OnKeyUp;
            _nativeWindow.KeyPress += OnKeyPress;
            _nativeWindow.MouseDown += OnMouseDown;
            _nativeWindow.MouseUp += OnMouseUp;
            _nativeWindow.Closing += OnWindowClosing;
            _nativeWindow.Closed += OnWindowClosed;
            _nativeWindow.MouseWheel += OnMouseWheel;

            _nativeWindow.FocusedChanged += OnWindowFocusChanged;
        }

        private void OnWindowFocusChanged(object sender, EventArgs e)
        {
            if (NativeWindow.Focused)
            {
                FocusGained?.Invoke();
            }
            else
            {
                FocusLost?.Invoke();
            }
        }

        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            CurrentSnapshot.WheelDelta += e.DeltaPrecise;
        }

        /// <summary>Gets the NativeWindow wrapped by this instance.</summary>
        public NativeWindow NativeWindow => _nativeWindow;

        /// <summary>Gets whether the native window exists.</summary>
        public bool Exists => _nativeWindow.Exists;

        /// <summary>Gets the native handle of the window.</summary>
        public IntPtr Handle => _nativeWindow.WindowInfo.Handle;

        /// <summary>Gets or sets the width of the window.</summary>
        public int Width
        {
            get { return _nativeWindow.Width; }
            set { _nativeWindow.Width = value; }
        }

        /// <summary>Gets or sets the height of the window.</summary>
        public int Height
        {
            get { return _nativeWindow.Height; }
            set { _nativeWindow.Height = value; }
        }

        /// <summary>Gets or sets the window title.</summary>
        public string Title
        {
            get { return _nativeWindow.Title; }
            set { _nativeWindow.Title = value; }
        }

        /// <summary>Gets or sets the visibility of the window.</summary>
        public bool Visible
        {
            get { return _nativeWindow.Visible; }
            set { _nativeWindow.Visible = value; }
        }

        /// <summary>Gets or sets the fullscreen state of the window.</summary>
        public WindowState WindowState
        {
            get
            {
                return OpenTKToVeldridState(_nativeWindow.WindowState, _nativeWindow.WindowBorder);
            }

            set
            {
                switch (value)
                {
                    case WindowState.Normal:
                        _nativeWindow.WindowBorder = WindowBorder.Resizable;
                        _nativeWindow.WindowState = OpenTK.WindowState.Normal;
                        if (_previousSize != default(Size))
                        {
                            _nativeWindow.ClientSize = _previousSize;
                        }
                        if (_previousPosition != default(Point))
                        {
                            _nativeWindow.X = _previousPosition.X;
                            _nativeWindow.Y = _previousPosition.Y;
                        }
                        break;
                    case WindowState.Minimized:
                        _nativeWindow.WindowBorder = WindowBorder.Resizable;
                        _nativeWindow.WindowState = OpenTK.WindowState.Minimized;
                        break;
                    case WindowState.Maximized:
                        _nativeWindow.WindowBorder = WindowBorder.Resizable;
                        _nativeWindow.WindowState = OpenTK.WindowState.Maximized;
                        break;
                    case WindowState.FullScreen:
                        _nativeWindow.WindowState = OpenTK.WindowState.Fullscreen;
                        break;
                    case WindowState.BorderlessFullScreen:
                        _nativeWindow.WindowBorder = WindowBorder.Hidden;
                        if (_nativeWindow.WindowState != OpenTK.WindowState.Normal)
                        {
                            _nativeWindow.WindowState = OpenTK.WindowState.Normal;
                        }

                        _previousSize = _nativeWindow.Size;

                        _previousPosition = new Point(_nativeWindow.X, _nativeWindow.Y);
                        SetCenteredFullScreenWindow(_previousPosition);
                        break;
                    default:
                        throw Illegal.Value<WindowState>();
                }
            }
        }

        private void SetCenteredFullScreenWindow(Point position)
        {
            int x = position.X;
            int actualX = 0;
            Size size = default(Size);
            DisplayIndex index = DisplayIndex.Default;
            while (x >= 0)
            {
                var display = DisplayDevice.GetDisplay(index);
                x -= display.Width;
                if (x > 0)
                {
                    actualX += display.Width;
                }
                else
                {
                    size = new Size(display.Width, display.Height);
                }

                index += 1;
            }

            if (size == default(Size))
            {
                throw new InvalidOperationException("SetCenteredFullScreen failed. Couldn't determine size.");
            }

            var bounds = _nativeWindow.Bounds;
            bounds.X = actualX;
            bounds.Y = 0;
            bounds.Width = size.Width;
            bounds.Height = size.Height;
            _nativeWindow.Bounds = bounds;
        }

        private static WindowState OpenTKToVeldridState(OpenTK.WindowState openTKState, WindowBorder border)
        {
            switch (openTKState)
            {
                case OpenTK.WindowState.Normal:
                    return border == WindowBorder.Hidden ? WindowState.BorderlessFullScreen : WindowState.Normal;
                case OpenTK.WindowState.Minimized:
                    return WindowState.Minimized;
                case OpenTK.WindowState.Maximized:
                    return WindowState.Maximized;
                case OpenTK.WindowState.Fullscreen:
                    return WindowState.FullScreen;
                default:
                    throw Illegal.Value<WindowState>();
            }
        }

        private static OpenTK.WindowState VeldridToOpenTKState(WindowState state)
        {
            switch (state)
            {
                case WindowState.Normal:
                    return OpenTK.WindowState.Normal;
                case WindowState.FullScreen:
                    return OpenTK.WindowState.Fullscreen;
                case WindowState.Maximized:
                    return OpenTK.WindowState.Maximized;
                case WindowState.Minimized:
                    return OpenTK.WindowState.Minimized;
                default:
                    throw Illegal.Value<WindowState>();
            }
        }

        IWindowInfo OpenTKWindow.OpenTKWindowInfo => _nativeWindow.WindowInfo;

        /// <summary>Gets the bounds of the window.</summary>
        public System.Drawing.Rectangle Bounds
        {
            get
            {
                var nativeBounds = _nativeWindow.Bounds;
                return new System.Drawing.Rectangle(nativeBounds.X, nativeBounds.Y, nativeBounds.Width, nativeBounds.Height);
            }
        }

        public bool CursorVisible
        {
            get
            {
                return _nativeWindow.CursorVisible;
            }
            set
            {
                SetCursorVisible(value);
            }
        }

        protected virtual void SetCursorVisible(bool value)
        {
            _nativeWindow.CursorVisible = value;
        }

        public bool Focused => _nativeWindow.Focused;

        /// <summary>Closes the window.</summary>
        public abstract void Close();

        /// <summary>Gets an InputSnapshot containing input information gatheres since the
        /// last time GetInputSnapshot was called.</summary>
        public InputSnapshot GetInputSnapshot()
        {
            SimpleInputSnapshot snapshot = GetAvailableSnapshot();
            if (NativeWindow.Exists)
            {
                MouseState cursorState = Mouse.GetCursorState();
                Point windowPoint = NativeWindow.PointToClient(new Point(cursorState.X, cursorState.Y));
                snapshot.MousePosition = new System.Numerics.Vector2(
                    windowPoint.X,
                    windowPoint.Y) / ScaleFactor;
            }
            _mouseDown.CopyTo(snapshot.MouseDown, 0);
            return snapshot;
        }

        protected abstract SimpleInputSnapshot GetAvailableSnapshot();

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            _mouseDown[(int)e.Button] = false;
            CurrentSnapshot.MouseEventsList.Add(new MouseEvent((MouseButton)e.Button, false));
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            _mouseDown[(int)e.Button] = true;
            CurrentSnapshot.MouseEventsList.Add(new MouseEvent((MouseButton)e.Button, true));
        }

        private void OnKeyUp(object sender, KeyboardKeyEventArgs e)
        {
            CurrentSnapshot.KeyEventsList.Add(new KeyEvent((Key)e.Key, false, ConvertModifiers(e.Modifiers)));
        }

        private void OnKeyPress(object sender, KeyPressEventArgs e)
        {
            CurrentSnapshot.KeyCharPressesList.Add(e.KeyChar);
        }

        private void OnKeyDown(object sender, KeyboardKeyEventArgs e)
        {
            CurrentSnapshot.KeyEventsList.Add(new KeyEvent((Key)e.Key, true, ConvertModifiers(e.Modifiers)));
        }

        private ModifierKeys ConvertModifiers(KeyModifiers modifiers)
        {
            ModifierKeys modifierKeys = ModifierKeys.None;
            if ((modifiers & KeyModifiers.Alt) == KeyModifiers.Alt)
            {
                modifierKeys |= ModifierKeys.Alt;
            }
            if ((modifiers & KeyModifiers.Control) == KeyModifiers.Control)
            {
                modifierKeys |= ModifierKeys.Control;
            }
            if ((modifiers & KeyModifiers.Shift) == KeyModifiers.Shift)
            {
                modifierKeys |= ModifierKeys.Shift;
            }

            return modifierKeys;
        }

        private void OnWindowResized(object sender, EventArgs e)
        {
            Resized?.Invoke();
        }

        private void OnWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Closing?.Invoke();
        }

        private void OnWindowClosed(object sender, EventArgs e)
        {
            Closed?.Invoke();
        }

        /// <summary>Converts a screen-space point to a client-space point.</summary>
        public System.Drawing.Point ScreenToClient(System.Drawing.Point p)
        {
            var tkPoint = _nativeWindow.PointToClient(new OpenTK.Point(p.X, p.Y));
            return new System.Drawing.Point(tkPoint.X, tkPoint.Y);
        }

        public System.Drawing.Point ClientToScreen(System.Drawing.Point p)
        {
            return new System.Drawing.Point(_nativeWindow.X + p.X, _nativeWindow.Y + p.Y);
        }

        protected class SimpleInputSnapshot : InputSnapshot
        {
            public List<KeyEvent> KeyEventsList { get; private set; } = new List<KeyEvent>();
            public List<MouseEvent> MouseEventsList { get; private set; } = new List<MouseEvent>();
            public List<char> KeyCharPressesList { get; private set; } = new List<char>();

            public IReadOnlyList<KeyEvent> KeyEvents => KeyEventsList;

            public IReadOnlyList<MouseEvent> MouseEvents => MouseEventsList;

            public IReadOnlyList<char> KeyCharPresses => KeyCharPressesList;

            public System.Numerics.Vector2 MousePosition { get; set; }

            private bool[] _mouseDown = new bool[13];
            public bool[] MouseDown => _mouseDown;
            public float WheelDelta { get; set; }

            public bool IsMouseDown(MouseButton button)
            {
                return _mouseDown[(int)button];
            }

            internal void Clear()
            {
                KeyEventsList.Clear();
                MouseEventsList.Clear();
                KeyCharPressesList.Clear();
                WheelDelta = 0f;
            }
        }
    }
}
