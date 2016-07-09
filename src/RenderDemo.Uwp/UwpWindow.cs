using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using Veldrid.Platform;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace RenderDemo.Uwp
{
    public class UwpWindow : Veldrid.Platform.Window
    {
        private readonly Page _applicationPage;
        private readonly SwapChainPanel _swp;
        protected SimpleInputSnapshot CurrentSnapshot = new SimpleInputSnapshot();

        public UwpWindow(Page page, SwapChainPanel swp )
        {
            _applicationPage = page;
            _swp = swp;

            _swp.KeyDown += OnKeyDown;
            _swp.KeyUp += OnKeyUp;
            _swp.PointerPressed += OnPointerPressed;
            _swp.PointerReleased += OnPointerReleased;

            _swp.SizeChanged += OnSizeChanged;
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            Resized?.Invoke();
        }

        private void OnKeyUp(object sender, KeyRoutedEventArgs e)
        {
            CurrentSnapshot.KeyEventsList.Add(new KeyEvent(MapKey(e.Key), e.KeyStatus.WasKeyDown, ModifierKeys.None));
        }

        private void OnPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            CurrentSnapshot.MouseEventsList.Add(new MouseEvent(MouseButton.Left, e.Pointer.IsInContact));
        }

        private void OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            CurrentSnapshot.MouseEventsList.Add(new MouseEvent(MouseButton.Left, e.Pointer.IsInContact));
        }

        private void OnKeyDown(object sender, KeyRoutedEventArgs e)
        {
            CurrentSnapshot.KeyEventsList.Add(new KeyEvent(MapKey(e.Key), e.KeyStatus.WasKeyDown, ModifierKeys.None));
        }

        public System.Drawing.Rectangle Bounds
        {
            get
            {
                return new System.Drawing.Rectangle(0, 0, (int)_swp.Width, (int)_swp.Height);
            }
        }

        public bool Exists
        {
            get
            {
                return true; // TODO: What is this?
            }
        }

        public IntPtr Handle
        {
            get
            {
                throw new InvalidOperationException();
            }
        }

        public int Height
        {
            get
            {
                return (int)_swp.Height;
            }

            set
            {
                _swp.Height = value; // Suggested height
            }
        }

        public Vector2 ScaleFactor
        {
            get
            {
                return Vector2.One;
            }
        }

        public string Title
        {
            get
            {
                throw new InvalidOperationException();
            }

            set
            {
                throw new InvalidOperationException();
            }
        }

        public bool Visible
        {
            get
            {
                return _swp.Visibility == Windows.UI.Xaml.Visibility.Visible;
            }

            set
            {
                _swp.Visibility = value ? Windows.UI.Xaml.Visibility.Visible : Windows.UI.Xaml.Visibility.Collapsed;
            }
        }

        public int Width
        {
            get
            {
                return (int)_swp.Width;
            }

            set
            {
                _swp.Width = value; // Suggested width
            }
        }

        public WindowState WindowState
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public event Action Closed;
        public event Action Closing;
        public event Action Resized;

        public void Close()
        {
            App.Current.Exit();
        }

        public InputSnapshot GetInputSnapshot()
        {
            SimpleInputSnapshot snapshot = GetAvailableSnapshot();
            return snapshot;
        }

        private SimpleInputSnapshot GetAvailableSnapshot()
        {
            _snapshotBackBuffer.Clear();
            SimpleInputSnapshot snapshot = Interlocked.Exchange(ref CurrentSnapshot, _snapshotBackBuffer);
            _snapshotBackBuffer = snapshot;
            return snapshot;
        }

        public System.Drawing.Point ScreenToClient(System.Drawing.Point p)
        {
            return p;
        }

        private SimpleInputSnapshot _snapshotBackBuffer = new SimpleInputSnapshot();


        protected class SimpleInputSnapshot : InputSnapshot
        {
            public List<KeyEvent> KeyEventsList { get; private set; } = new List<KeyEvent>();
            public List<MouseEvent> MouseEventsList { get; private set; } = new List<MouseEvent>();
            public List<char> KeyCharPressesList { get; private set; } = new List<char>();

            public IReadOnlyCollection<KeyEvent> KeyEvents => KeyEventsList;

            public IReadOnlyCollection<MouseEvent> MouseEvents => MouseEventsList;

            public IReadOnlyCollection<char> KeyCharPresses => KeyCharPressesList;

            public System.Numerics.Vector2 MousePosition { get; set; }

            internal void Clear()
            {
                KeyEventsList.Clear();
                MouseEventsList.Clear();
                KeyCharPressesList.Clear();
            }
        }

        private Key MapKey(VirtualKey key)
        {
            switch (key)
            {
                case VirtualKey.A:
                    return Key.A;
                case VirtualKey.B:
                    return Key.B;
                case VirtualKey.C:
                    return Key.C;
                case VirtualKey.D:
                    return Key.D;
                case VirtualKey.E:
                    return Key.E;
                case VirtualKey.F:
                    return Key.F;
                case VirtualKey.G:
                    return Key.G;
                case VirtualKey.H:
                    return Key.H;
                case VirtualKey.I:
                    return Key.I;
                case VirtualKey.J:
                    return Key.J;
                case VirtualKey.K:
                    return Key.K;
                case VirtualKey.L:
                    return Key.L;
                case VirtualKey.M:
                    return Key.M;
                case VirtualKey.N:
                    return Key.N;
                case VirtualKey.O:
                    return Key.O;
                case VirtualKey.P:
                    return Key.P;
                case VirtualKey.Q:
                    return Key.Q;
                case VirtualKey.R:
                    return Key.R;
                case VirtualKey.S:
                    return Key.S;
                case VirtualKey.T:
                    return Key.T;
                case VirtualKey.U:
                    return Key.U;
                case VirtualKey.V:
                    return Key.V;
                case VirtualKey.W:
                    return Key.W;
                case VirtualKey.X:
                    return Key.X;
                case VirtualKey.Y:
                    return Key.Y;
                case VirtualKey.Z:
                    return Key.Z;
                case VirtualKey.F1:
                    return Key.F1;
                case VirtualKey.F2:
                    return Key.F2;
                case VirtualKey.F3:
                    return Key.F3;
                case VirtualKey.F4:
                    return Key.F4;
                case VirtualKey.F5:
                    return Key.F5;
                case VirtualKey.F6:
                    return Key.F6;
                case VirtualKey.F7:
                    return Key.F7;
                case VirtualKey.F8:
                    return Key.F8;
                case VirtualKey.F9:
                    return Key.F9;
                case VirtualKey.F10:
                    return Key.F10;
                case VirtualKey.F11:
                    return Key.F11;
                case VirtualKey.F12:
                    return Key.F12;
                default:
                    return Key.Unknown;
            }
        }
    }
}
