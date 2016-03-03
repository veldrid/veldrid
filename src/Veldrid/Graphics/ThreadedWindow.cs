using OpenTK;
using System.Threading;
using System.Threading.Tasks;
using System;
using OpenTK.Input;
using System.Collections.Generic;
using System.Numerics;

namespace Veldrid.Graphics
{
    public class ThreadedWindow : WindowInputProvider
    {
        private NativeWindow _nativeWindow;
        private bool _shouldClose;

        internal volatile bool NeedsResizing;

        private ThreadedInputSnapshot _currentSnapshot;
        private ThreadedInputSnapshot _snapshotBackBuffer;

        public ThreadedWindow()
        {
            _currentSnapshot = new ThreadedInputSnapshot();
            _snapshotBackBuffer = new ThreadedInputSnapshot();

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
            _currentSnapshot.MousePosition = new System.Numerics.Vector2(e.X, e.Y);
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            _currentSnapshot.MouseEvents.Add(new MouseEvent(e.Button, false));
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            _currentSnapshot.MouseEvents.Add(new MouseEvent(e.Button, true));
        }

        private void OnKeyUp(object sender, KeyboardKeyEventArgs e)
        {
            _currentSnapshot.KeyEvents.Add(new KeyEvent(e.Key, false));
        }

        private void OnKeyDown(object sender, KeyboardKeyEventArgs e)
        {
            _currentSnapshot.KeyEvents.Add(new KeyEvent(e.Key, true));
        }

        private void OnWindowResized(object sender, EventArgs e)
        {
            NeedsResizing = true;
        }

        public InputSnapshot GetInputSnapshot()
        {
            _snapshotBackBuffer.Clear();
            ThreadedInputSnapshot snapshot = Interlocked.Exchange(ref _currentSnapshot, _snapshotBackBuffer);
            _snapshotBackBuffer = snapshot;
            return snapshot;
        }

        private class ThreadedInputSnapshot : InputSnapshot
        {
            public List<MouseEvent> MouseEvents = new List<MouseEvent>();
            public List<KeyEvent> KeyEvents = new List<KeyEvent>();
            public System.Numerics.Vector2 MousePosition;

            private int __ID = Environment.TickCount;

            IReadOnlyCollection<KeyEvent> InputSnapshot.KeyEvents => KeyEvents;

            IReadOnlyCollection<MouseEvent> InputSnapshot.MouseEvents => MouseEvents;

            System.Numerics.Vector2 InputSnapshot.MousePosition => MousePosition;

            internal void Clear()
            {
                KeyEvents.Clear();
                MouseEvents.Clear();
            }
        }
    }
}
