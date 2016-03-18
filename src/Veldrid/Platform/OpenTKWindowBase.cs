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

        public System.Numerics.Vector2 ScaleFactor { get; private set; }

        protected SimpleInputSnapshot CurrentSnapshot = new SimpleInputSnapshot();

        public OpenTKWindowBase()
        {
            ConstructDefaultWindow();
        }

        protected virtual void ConstructDefaultWindow()
        {
            int desiredWidth = 960;
            int desiredHeight = 540;
            _nativeWindow = new NativeWindow(
                desiredWidth,
                desiredHeight,
                "Veldrid Render Window",
                 GameWindowFlags.Default,
                 GraphicsMode.Default,
                 DisplayDevice.Default);

            int actualWidth = _nativeWindow.Width;
            int actualHeight = _nativeWindow.Height;

            ScaleFactor = new System.Numerics.Vector2((float)actualWidth / desiredWidth, (float)actualHeight / desiredHeight);

            _nativeWindow.Visible = true;
            _nativeWindow.X = 100;
            _nativeWindow.Y = 100;

            _nativeWindow.Resize += OnWindowResized;
            _nativeWindow.KeyDown += OnKeyDown;
            _nativeWindow.KeyUp += OnKeyUp;
            _nativeWindow.MouseDown += OnMouseDown;
            _nativeWindow.MouseUp += OnMouseUp;
            _nativeWindow.MouseMove += OnMouseMove;
            _nativeWindow.Closing += OnWindowClosing;
            _nativeWindow.Closed += OnWindowClosed;
        }

        public NativeWindow NativeWindow => _nativeWindow;

        public bool Exists => _nativeWindow.Exists;

        public IntPtr Handle => _nativeWindow.WindowInfo.Handle;

        public int Height
        {
            get
            {
                return _nativeWindow.Height;
            }

            set
            {
                _nativeWindow.Height = value;
            }
        }

        public string Title
        {
            get
            {
                return _nativeWindow.Title;
            }

            set
            {
                _nativeWindow.Title = value;
            }
        }

        public bool Visible
        {
            get
            {
                return _nativeWindow.Visible;
            }

            set
            {
                _nativeWindow.Visible = value;
            }
        }

        public int Width
        {
            get
            {
                return _nativeWindow.Width;
            }

            set
            {
                _nativeWindow.Width = value;
            }
        }

        public WindowState WindowState
        {
            get
            {
                switch (_nativeWindow.WindowState)
                {
                    case OpenTK.WindowState.Normal:
                        return WindowState.Normal;
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

            set
            {
                switch (value)
                {
                    case WindowState.Normal:
                        _nativeWindow.WindowState = OpenTK.WindowState.Normal;
                        break;
                    case WindowState.Minimized:
                        _nativeWindow.WindowState = OpenTK.WindowState.Minimized;
                        break;
                    case WindowState.Maximized:
                        _nativeWindow.WindowState = OpenTK.WindowState.Maximized;
                        break;
                    case WindowState.FullScreen:
                        _nativeWindow.WindowState = OpenTK.WindowState.Fullscreen;
                        break;
                    default:
                        throw Illegal.Value<WindowState>();
                }
            }
        }

        IWindowInfo OpenTKWindow.OpenTKWindowInfo => _nativeWindow.WindowInfo;

        public abstract void Close();

        public abstract InputSnapshot GetInputSnapshot();

        private void OnMouseMove(object sender, MouseMoveEventArgs e)
        {
            CurrentSnapshot.MousePosition = new System.Numerics.Vector2(e.X, e.Y);
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            CurrentSnapshot.MouseEventsList.Add(new MouseEvent(e.Button, false));
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            CurrentSnapshot.MouseEventsList.Add(new MouseEvent(e.Button, true));
        }

        private void OnKeyUp(object sender, KeyboardKeyEventArgs e)
        {
            CurrentSnapshot.KeyEventsList.Add(new KeyEvent(e.Key, false, ConvertModifiers(e.Modifiers)));
        }

        private void OnKeyDown(object sender, KeyboardKeyEventArgs e)
        {
            CurrentSnapshot.KeyEventsList.Add(new KeyEvent(e.Key, true, ConvertModifiers(e.Modifiers)));
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

        protected class SimpleInputSnapshot : InputSnapshot
        {
            public List<KeyEvent> KeyEventsList { get; private set; } = new List<KeyEvent>();
            public List<MouseEvent> MouseEventsList { get; private set; } = new List<MouseEvent>();

            public IReadOnlyCollection<KeyEvent> KeyEvents => KeyEventsList;

            public IReadOnlyCollection<MouseEvent> MouseEvents => MouseEventsList;

            public System.Numerics.Vector2 MousePosition { get; set; }

            internal void Clear()
            {
                KeyEventsList.Clear();
                MouseEventsList.Clear();
            }
        }
    }
}
