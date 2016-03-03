using OpenTK;
using System.Threading;
using System.Threading.Tasks;
using System;
using OpenTK.Input;
using System.Collections.Concurrent;

namespace Veldrid.Graphics
{
    public class ThreadedWindow
    {
        private NativeWindow _nativeWindow;
        private bool _shouldClose;

        private ConcurrentQueue<KeyEvent> _keyEvents = new ConcurrentQueue<KeyEvent>();
        private ConcurrentQueue<MouseEvent> _mouseEvents = new ConcurrentQueue<MouseEvent>();

        internal volatile bool NeedsResizing;

        private Vector2 _mousePosition;
        internal Vector2 MousePosition
        {
            get
            {
                return _mousePosition;
            }
            set
            {
                _mousePosition = value;
            }
        }

        public ThreadedWindow()
        {
            using (ManualResetEvent mre = new ManualResetEvent(initialState: false))
            {
                Task.Factory.StartNew(WindowOwnerRoutine, mre, TaskCreationOptions.LongRunning);
                mre.WaitOne();
            }
        }


        public WindowInfo WindowInfo { get; private set; }
        public NativeWindow NativeWindow => _nativeWindow;

        public void Close() => _shouldClose = true;

        private void WindowOwnerRoutine(object state)
        {
            ManualResetEvent initializationEvent = (ManualResetEvent)state;

            _nativeWindow = new NativeWindow();
            _nativeWindow.Visible = true;
            _nativeWindow.X = 100;
            _nativeWindow.Y = 100;
            WindowInfo = new OpenTKWindowInfo(_nativeWindow);

            _nativeWindow.Resize += OnWindowResized;
            _nativeWindow.KeyDown += OnKeyDown;
            _nativeWindow.KeyUp += OnKeyUp;
            _nativeWindow.MouseDown += OnMouseDown;
            _nativeWindow.MouseUp += OnMouseUp;
            _nativeWindow.MouseMove += OnMouseMove;

            initializationEvent.Set();

            while (_nativeWindow.Exists && !_shouldClose)
            {
                _nativeWindow.ProcessEvents();
            }
        }

        private void OnMouseMove(object sender, MouseMoveEventArgs e)
        {
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            _mouseEvents.Enqueue(new MouseEvent(e.Button, false));
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            _mouseEvents.Enqueue(new MouseEvent(e.Button, true));
        }

        private void OnKeyUp(object sender, KeyboardKeyEventArgs e)
        {
            _keyEvents.Enqueue(new KeyEvent(e.Key, false));
        }

        private void OnKeyDown(object sender, KeyboardKeyEventArgs e)
        {
            _keyEvents.Enqueue(new KeyEvent(e.Key, true));
        }

        private void OnWindowResized(object sender, EventArgs e)
        {
            NeedsResizing = true;
        }
    }
}
