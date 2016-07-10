using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Veldrid.Platform;
using Windows.Foundation;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace Veldrid.Platform
{
    public class UwpWindow : Window
    {
        private readonly SwapChainPanel _swp;
        protected SimpleInputSnapshot CurrentSnapshot = new SimpleInputSnapshot();
        private SimpleInputSnapshot _snapshotBackBuffer = new SimpleInputSnapshot();
        private double _pixelScale;
        private double _cachedWidth;
        private double _cachedHeight;
        private Point _currentPointerPosition;
        private string _cachedTitle;
        private WindowState _cachedWindowState;

        public UwpWindow(SwapChainPanel swp, float pixelScale)
        {
            _swp = swp;
            CoreWindow coreWindow = CoreWindow.GetForCurrentThread();
            coreWindow.KeyDown += OnKeyDown;
            coreWindow.KeyUp += OnKeyUp;
            coreWindow.CharacterReceived += OnKeyReceived;

            _swp.PointerPressed += OnPointerPressed;
            _swp.PointerReleased += OnPointerReleased;
            _swp.PointerMoved += OnPointerMoved;

            _swp.SizeChanged += OnSizeChanged;

            _pixelScale = pixelScale;
            _cachedWidth = _swp.RenderSize.Width * _pixelScale;
            _cachedHeight = _swp.RenderSize.Height * _pixelScale;

            coreWindow.Closed += OnCoreWindowClosed;

            _cachedTitle = ApplicationView.GetForCurrentView().Title;
        }

        private void OnKeyReceived(CoreWindow sender, CharacterReceivedEventArgs args)
        {
            CurrentSnapshot.KeyCharPressesList.Add((char)args.KeyCode);
        }

        private void OnCoreWindowClosed(CoreWindow sender, CoreWindowEventArgs args)
        {
            Closing?.Invoke();
            Closed?.Invoke();
        }

        private void OnPointerMoved(object sender, PointerRoutedEventArgs e)
        {
            _currentPointerPosition = e.GetCurrentPoint(_swp).Position;
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            _cachedWidth = _swp.RenderSize.Width * _pixelScale;
            _cachedHeight = _swp.RenderSize.Height * _pixelScale;
            Resized?.Invoke();
        }

        private void OnPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            CurrentSnapshot.MouseEventsList.Add(new MouseEvent(MouseButton.Left, e.Pointer.IsInContact));
        }

        private void OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            CurrentSnapshot.MouseEventsList.Add(new MouseEvent(MouseButton.Left, e.Pointer.IsInContact));
            Debug.WriteLine("PointerPressed: " + e.GetCurrentPoint(_swp).Position);
        }

        private void OnKeyUp(CoreWindow sender, KeyEventArgs e)
        {
            CurrentSnapshot.KeyEventsList.Add(new KeyEvent(MapKey(e.VirtualKey), false, ModifierKeys.None));
        }

        private void OnKeyDown(CoreWindow sender, KeyEventArgs e)
        {
            CurrentSnapshot.KeyEventsList.Add(new KeyEvent(MapKey(e.VirtualKey), true, ModifierKeys.None));
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
                return (int)_cachedHeight;
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
                return _cachedTitle;
            }
            set
            {
                if (value != _cachedTitle)
                {
                    SetTitle(value);

                    _cachedTitle = value;
                }
            }
        }

        private async void SetTitle(string value)
        {
            await _swp.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                ApplicationView.GetForCurrentView().Title = value;
            });
        }

        public bool Visible
        {
            get
            {
                return _swp.Visibility == Visibility.Visible;
            }

            set
            {
                _swp.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public int Width
        {
            get
            {
                return (int)_cachedWidth;
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
                return _cachedWindowState;
            }
            set
            {
                if (_cachedWindowState != value)
                {
                    SetWindowState(value);
                    _cachedWindowState = value;
                }
            }
        }

        private async void SetWindowState(WindowState value)
        {
            await _swp.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                ApplicationView av = ApplicationView.GetForCurrentView();
                if (value == WindowState.FullScreen)
                {
                    av.TryEnterFullScreenMode();
                }
                else
                {
                    av.ExitFullScreenMode();
                }
            });
        }

        public event Action Closed;
        public event Action Closing;
        public event Action Resized;

        public void Close()
        {
            Application.Current.Exit();
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
            snapshot.MousePosition = _currentPointerPosition.ToVector2();
            _snapshotBackBuffer = snapshot;
            return snapshot;
        }

        public System.Drawing.Point ScreenToClient(System.Drawing.Point p)
        {
            return p;
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

            internal void Clear()
            {
                KeyEventsList.Clear();
                MouseEventsList.Clear();
                KeyCharPressesList.Clear();
            }
        }

        private Key MapKey(VirtualKey key)
        {
            Key mapped = Key.Unknown;
            switch (key)
            {
                case VirtualKey.None:
                    break;
                case VirtualKey.LeftButton:
                    mapped = Key.Left;
                    break;
                case VirtualKey.RightButton:
                    mapped = Key.Right;
                    break;
                case VirtualKey.Cancel:
                    mapped = Key.Unknown;
                    break;
                case VirtualKey.MiddleButton:
                    mapped = Key.Unknown;
                    break;
                case VirtualKey.XButton1:
                    mapped = Key.Unknown;
                    break;
                case VirtualKey.XButton2:
                    mapped = Key.Unknown;
                    break;
                case VirtualKey.Back:
                    mapped = Key.Back;
                    break;
                case VirtualKey.Tab:
                    mapped = Key.Tab;
                    break;
                case VirtualKey.Clear:
                    mapped = Key.Clear;
                    break;
                case VirtualKey.Enter:
                    mapped = Key.Enter;
                    break;
                case VirtualKey.Shift:
                    mapped = Key.ShiftLeft;
                    break;
                case VirtualKey.Control:
                    mapped = Key.ControlLeft;
                    break;
                case VirtualKey.Menu:
                    mapped = Key.Menu;
                    break;
                case VirtualKey.Pause:
                    mapped = Key.Pause;
                    break;
                case VirtualKey.CapitalLock:
                    mapped = Key.CapsLock;
                    break;
                case VirtualKey.Kana:
                    mapped = Key.Unknown;
                    break;
                case VirtualKey.Junja:
                    mapped = Key.Unknown;
                    break;
                case VirtualKey.Final:
                    mapped = Key.Unknown;
                    break;
                case VirtualKey.Kanji:
                    mapped = Key.Unknown;
                    break;
                case VirtualKey.Escape:
                    mapped = Key.Escape;
                    break;
                case VirtualKey.Convert:
                    mapped = Key.Unknown;
                    break;
                case VirtualKey.NonConvert:
                    mapped = Key.Unknown;
                    break;
                case VirtualKey.Accept:
                    mapped = Key.Unknown;
                    break;
                case VirtualKey.ModeChange:
                    mapped = Key.Unknown;
                    break;
                case VirtualKey.Space:
                    mapped = Key.Space;
                    break;
                case VirtualKey.PageUp:
                    mapped = Key.PageUp;
                    break;
                case VirtualKey.PageDown:
                    mapped = Key.PageDown;
                    break;
                case VirtualKey.End:
                    mapped = Key.End;
                    break;
                case VirtualKey.Home:
                    mapped = Key.Home;
                    break;
                case VirtualKey.Left:
                    mapped = Key.Left;
                    break;
                case VirtualKey.Up:
                    mapped = Key.Up;
                    break;
                case VirtualKey.Right:
                    mapped = Key.Right;
                    break;
                case VirtualKey.Down:
                    mapped = Key.Down;
                    break;
                case VirtualKey.Select:
                    mapped = Key.Unknown;
                    break;
                case VirtualKey.Print:
                    mapped = Key.Unknown;
                    break;
                case VirtualKey.Execute:
                    mapped = Key.Unknown;
                    break;
                case VirtualKey.Snapshot:
                    mapped = Key.Unknown;
                    break;
                case VirtualKey.Insert:
                    mapped = Key.Insert;
                    break;
                case VirtualKey.Delete:
                    mapped = Key.Delete;
                    break;
                case VirtualKey.Help:
                    mapped = Key.Unknown;
                    break;
                case VirtualKey.Number0:
                    mapped = Key.Number0;
                    break;
                case VirtualKey.Number1:
                    mapped = Key.Number1;
                    break;
                case VirtualKey.Number2:
                    mapped = Key.Number2;
                    break;
                case VirtualKey.Number3:
                    mapped = Key.Number3;
                    break;
                case VirtualKey.Number4:
                    mapped = Key.Number4;
                    break;
                case VirtualKey.Number5:
                    mapped = Key.Number5;
                    break;
                case VirtualKey.Number6:
                    mapped = Key.Number6;
                    break;
                case VirtualKey.Number7:
                    mapped = Key.Number7;
                    break;
                case VirtualKey.Number8:
                    mapped = Key.Number8;
                    break;
                case VirtualKey.Number9:
                    mapped = Key.Number9;
                    break;
                case VirtualKey.A:
                    mapped = Key.A;
                    break;
                case VirtualKey.B:
                    mapped = Key.B;
                    break;
                case VirtualKey.C:
                    mapped = Key.C;
                    break;
                case VirtualKey.D:
                    mapped = Key.D;
                    break;
                case VirtualKey.E:
                    mapped = Key.E;
                    break;
                case VirtualKey.F:
                    mapped = Key.F;
                    break;
                case VirtualKey.G:
                    mapped = Key.G;
                    break;
                case VirtualKey.H:
                    mapped = Key.H;
                    break;
                case VirtualKey.I:
                    mapped = Key.I;
                    break;
                case VirtualKey.J:
                    mapped = Key.J;
                    break;
                case VirtualKey.K:
                    mapped = Key.K;
                    break;
                case VirtualKey.L:
                    mapped = Key.L;
                    break;
                case VirtualKey.M:
                    mapped = Key.M;
                    break;
                case VirtualKey.N:
                    mapped = Key.N;
                    break;
                case VirtualKey.O:
                    mapped = Key.O;
                    break;
                case VirtualKey.P:
                    mapped = Key.P;
                    break;
                case VirtualKey.Q:
                    mapped = Key.Q;
                    break;
                case VirtualKey.R:
                    mapped = Key.R;
                    break;
                case VirtualKey.S:
                    mapped = Key.S;
                    break;
                case VirtualKey.T:
                    mapped = Key.T;
                    break;
                case VirtualKey.U:
                    mapped = Key.U;
                    break;
                case VirtualKey.V:
                    mapped = Key.V;
                    break;
                case VirtualKey.W:
                    mapped = Key.W;
                    break;
                case VirtualKey.X:
                    mapped = Key.X;
                    break;
                case VirtualKey.Y:
                    mapped = Key.Y;
                    break;
                case VirtualKey.Z:
                    mapped = Key.Z;
                    break;
                case VirtualKey.LeftWindows:
                    mapped = Key.WinLeft;
                    break;
                case VirtualKey.RightWindows:
                    mapped = Key.WinRight;
                    break;
                case VirtualKey.Application:
                    mapped = Key.Unknown;
                    break;
                case VirtualKey.Sleep:
                    mapped = Key.Sleep;
                    break;
                case VirtualKey.NumberPad0:
                    mapped = Key.Keypad0;
                    break;
                case VirtualKey.NumberPad1:
                    mapped = Key.Keypad1;
                    break;
                case VirtualKey.NumberPad2:
                    mapped = Key.Keypad2;
                    break;
                case VirtualKey.NumberPad3:
                    mapped = Key.Keypad3;
                    break;
                case VirtualKey.NumberPad4:
                    mapped = Key.Keypad4;
                    break;
                case VirtualKey.NumberPad5:
                    mapped = Key.Keypad5;
                    break;
                case VirtualKey.NumberPad6:
                    mapped = Key.Keypad6;
                    break;
                case VirtualKey.NumberPad7:
                    mapped = Key.Keypad7;
                    break;
                case VirtualKey.NumberPad8:
                    mapped = Key.Keypad8;
                    break;
                case VirtualKey.NumberPad9:
                    mapped = Key.Keypad9;
                    break;
                case VirtualKey.Multiply:
                    mapped = Key.KeypadMultiply;
                    break;
                case VirtualKey.Add:
                    mapped = Key.KeypadAdd;
                    break;
                case VirtualKey.Separator:
                    mapped = Key.Unknown;
                    break;
                case VirtualKey.Subtract:
                    mapped = Key.KeypadSubtract;
                    break;
                case VirtualKey.Decimal:
                    mapped = Key.KeypadDecimal;
                    break;
                case VirtualKey.Divide:
                    mapped = Key.KeypadDivide;
                    break;
                case VirtualKey.F1:
                    mapped = Key.F1;
                    break;
                case VirtualKey.F2:
                    mapped = Key.F2;
                    break;
                case VirtualKey.F3:
                    mapped = Key.F3;
                    break;
                case VirtualKey.F4:
                    mapped = Key.F4;
                    break;
                case VirtualKey.F5:
                    mapped = Key.F5;
                    break;
                case VirtualKey.F6:
                    mapped = Key.F6;
                    break;
                case VirtualKey.F7:
                    mapped = Key.F7;
                    break;
                case VirtualKey.F8:
                    mapped = Key.F8;
                    break;
                case VirtualKey.F9:
                    mapped = Key.F9;
                    break;
                case VirtualKey.F10:
                    mapped = Key.F10;
                    break;
                case VirtualKey.F11:
                    mapped = Key.F11;
                    break;
                case VirtualKey.F12:
                    mapped = Key.F12;
                    break;
                case VirtualKey.F13:
                    mapped = Key.F13;
                    break;
                case VirtualKey.F14:
                    mapped = Key.F14;
                    break;
                case VirtualKey.F15:
                    mapped = Key.F15;
                    break;
                case VirtualKey.F16:
                    mapped = Key.F16;
                    break;
                case VirtualKey.F17:
                    mapped = Key.F17;
                    break;
                case VirtualKey.F18:
                    mapped = Key.F18;
                    break;
                case VirtualKey.F19:
                    mapped = Key.F19;
                    break;
                case VirtualKey.F20:
                    mapped = Key.F20;
                    break;
                case VirtualKey.F21:
                    mapped = Key.F21;
                    break;
                case VirtualKey.F22:
                    mapped = Key.F22;
                    break;
                case VirtualKey.F23:
                    mapped = Key.F23;
                    break;
                case VirtualKey.F24:
                    mapped = Key.F24;
                    break;
                case VirtualKey.NavigationView:
                    mapped = Key.Unknown;
                    break;
                case VirtualKey.NavigationMenu:
                    mapped = Key.Menu;
                    break;
                case VirtualKey.NavigationUp:
                    mapped = Key.Unknown;
                    break;
                case VirtualKey.NavigationDown:
                    mapped = Key.Unknown;
                    break;
                case VirtualKey.NavigationLeft:
                    mapped = Key.Unknown;
                    break;
                case VirtualKey.NavigationRight:
                    mapped = Key.Unknown;
                    break;
                case VirtualKey.NavigationAccept:
                    mapped = Key.Unknown;
                    break;
                case VirtualKey.NavigationCancel:
                    mapped = Key.Unknown;
                    break;
                case VirtualKey.NumberKeyLock:
                    mapped = Key.NumLock;
                    break;
                case VirtualKey.Scroll:
                    mapped = Key.ScrollLock;
                    break;
                case VirtualKey.LeftShift:
                    mapped = Key.ShiftLeft;
                    break;
                case VirtualKey.RightShift:
                    mapped = Key.ShiftRight;
                    break;
                case VirtualKey.LeftControl:
                    mapped = Key.ControlLeft;
                    break;
                case VirtualKey.RightControl:
                    mapped = Key.ControlRight;
                    break;
                case VirtualKey.LeftMenu:
                    mapped = Key.Unknown;
                    break;
                case VirtualKey.RightMenu:
                    mapped = Key.Unknown;
                    break;
                case VirtualKey.GoBack:
                    mapped = Key.Back;
                    break;
                case VirtualKey.GoForward:
                    mapped = Key.Unknown;
                    break;
                case VirtualKey.Refresh:
                    mapped = Key.Unknown;
                    break;
                case VirtualKey.Stop:
                    mapped = Key.Unknown;
                    break;
                case VirtualKey.Search:
                    mapped = Key.Unknown;
                    break;
                case VirtualKey.Favorites:
                    mapped = Key.Unknown;
                    break;
                case VirtualKey.GoHome:
                    mapped = Key.Home;
                    break;
                case VirtualKey.GamepadA:
                    mapped = Key.Unknown;
                    break;
                case VirtualKey.GamepadB:
                    mapped = Key.Unknown;
                    break;
                case VirtualKey.GamepadX:
                    mapped = Key.Unknown;
                    break;
                case VirtualKey.GamepadY:
                    mapped = Key.Unknown;
                    break;
                case VirtualKey.GamepadRightShoulder:
                    mapped = Key.Unknown;
                    break;
                case VirtualKey.GamepadLeftShoulder:
                    mapped = Key.Unknown;
                    break;
                case VirtualKey.GamepadLeftTrigger:
                    mapped = Key.Unknown;
                    break;
                case VirtualKey.GamepadRightTrigger:
                    mapped = Key.Unknown;
                    break;
                case VirtualKey.GamepadDPadUp:
                    mapped = Key.Unknown;
                    break;
                case VirtualKey.GamepadDPadDown:
                    mapped = Key.Unknown;
                    break;
                case VirtualKey.GamepadDPadLeft:
                    mapped = Key.Unknown;
                    break;
                case VirtualKey.GamepadDPadRight:
                    mapped = Key.Unknown;
                    break;
                case VirtualKey.GamepadMenu:
                    mapped = Key.Unknown;
                    break;
                case VirtualKey.GamepadView:
                    mapped = Key.Unknown;
                    break;
                case VirtualKey.GamepadLeftThumbstickButton:
                    mapped = Key.Unknown;
                    break;
                case VirtualKey.GamepadRightThumbstickButton:
                    mapped = Key.Unknown;
                    break;
                case VirtualKey.GamepadLeftThumbstickUp:
                    mapped = Key.Unknown;
                    break;
                case VirtualKey.GamepadLeftThumbstickDown:
                    mapped = Key.Unknown;
                    break;
                case VirtualKey.GamepadLeftThumbstickRight:
                    mapped = Key.Unknown;
                    break;
                case VirtualKey.GamepadLeftThumbstickLeft:
                    mapped = Key.Unknown;
                    break;
                case VirtualKey.GamepadRightThumbstickUp:
                    mapped = Key.Unknown;
                    break;
                case VirtualKey.GamepadRightThumbstickDown:
                    mapped = Key.Unknown;
                    break;
                case VirtualKey.GamepadRightThumbstickRight:
                    mapped = Key.Unknown;
                    break;
                case VirtualKey.GamepadRightThumbstickLeft:
                    mapped = Key.Unknown;
                    break;
                default:
                    break;
            }

            return mapped;
        }
    }
}
