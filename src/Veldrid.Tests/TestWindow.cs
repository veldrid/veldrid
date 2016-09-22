using OpenTK;
using OpenTK.Input;
using OpenTK.Platform;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Drawing;

namespace Veldrid.Platform
{
    public class TestWindow : Window, OpenTKWindow
    {
        private NativeWindow _nativeWindow;

        public event Action Resized;
        public event Action Closing;
        public event Action Closed;

        protected SimpleInputSnapshot CurrentSnapshot = new SimpleInputSnapshot();

        public TestWindow()
        {
            ConstructDefaultWindow();
        }

        protected virtual void ConstructDefaultWindow()
        {
            _nativeWindow = new NativeWindow();
            _nativeWindow.Visible = false;
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
                        throw new InvalidOperationException();
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
                        throw new InvalidOperationException();
                }
            }
        }

        IWindowInfo OpenTKWindow.OpenTKWindowInfo => _nativeWindow.WindowInfo;

        public System.Numerics.Vector2 ScaleFactor => System.Numerics.Vector2.One;

        public System.Drawing.Rectangle Bounds
        {
            get
            {
                var nativeBounds = _nativeWindow.Bounds;
                return new System.Drawing.Rectangle(nativeBounds.X, nativeBounds.Y, nativeBounds.Width, nativeBounds.Height);
            }
        }

        public bool CursorVisible { get; set; }

        public bool Focused { get; } = true;

        public void Close() { throw new NotImplementedException(); }

        public InputSnapshot GetInputSnapshot() { throw new NotImplementedException(); }

        private void OnMouseMove(object sender, MouseMoveEventArgs e)
        {
            CurrentSnapshot.MousePosition = new System.Numerics.Vector2(e.X, e.Y);
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            CurrentSnapshot.MouseEventsList.Add(new MouseEvent((MouseButton)e.Button, false));
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            CurrentSnapshot.MouseEventsList.Add(new MouseEvent((MouseButton)e.Button, true));
        }

        private void OnKeyUp(object sender, KeyboardKeyEventArgs e)
        {
            CurrentSnapshot.KeyEventsList.Add(new KeyEvent((Key)e.Key, false, ConvertModifiers(e.Modifiers)));
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

        public System.Drawing.Point ScreenToClient(System.Drawing.Point p)
        {
            var tkPoint = _nativeWindow.PointToClient(new OpenTK.Point(p.X, p.Y));
            return new System.Drawing.Point(tkPoint.X, tkPoint.Y);
        }

        public System.Drawing.Point ClientToScreen(System.Drawing.Point p)
        {
            return new System.Drawing.Point(p.X + _nativeWindow.X, p.Y + _nativeWindow.Y);
        }

        protected class SimpleInputSnapshot : InputSnapshot
        {
            public List<KeyEvent> KeyEventsList { get; private set; } = new List<KeyEvent>();
            public List<MouseEvent> MouseEventsList { get; private set; } = new List<MouseEvent>();
            public List<char> KeyCharPressesList { get; private set; } = new List<char>();

            public IReadOnlyCollection<KeyEvent> KeyEvents => KeyEventsList;

            public IReadOnlyCollection<MouseEvent> MouseEvents => MouseEventsList;

            public IReadOnlyCollection<char> KeyCharPresses => KeyCharPressesList;

            public System.Numerics.Vector2 MousePosition { get; set; }

            public float WheelDelta { get { throw new NotImplementedException(); } }

            internal void Clear()
            {
                KeyEventsList.Clear();
                MouseEventsList.Clear();
            }

            public bool IsMouseDown(MouseButton button)
            {
                throw new NotImplementedException();
            }
        }
    }
}
