using OpenTK;
using OpenTK.Input;
using System;
using System.Collections.Generic;

namespace Veldrid.Graphics
{
    public class MainThreadWindow : WindowInputProvider
    {
        private readonly NativeWindow _nativeWindow;
        private readonly SimpleInputSnapshot _currentSnapshot = new SimpleInputSnapshot();

        internal volatile bool NeedsResizing;

        public MainThreadWindow()
        {
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
        }

        public WindowInfo WindowInfo { get; }
        public NativeWindow NativeWindow => _nativeWindow;

        public void Close()
        {
            _nativeWindow.Close();
        }

        private void OnMouseMove(object sender, MouseMoveEventArgs e)
        {
            _currentSnapshot.MousePosition = new System.Numerics.Vector2(e.X, e.Y);
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            _currentSnapshot.MouseEventsList.Add(new MouseEvent(e.Button, false));
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            _currentSnapshot.MouseEventsList.Add(new MouseEvent(e.Button, true));
        }

        private void OnKeyUp(object sender, KeyboardKeyEventArgs e)
        {
            _currentSnapshot.KeyEventsList.Add(new KeyEvent(e.Key, false));
        }

        private void OnKeyDown(object sender, KeyboardKeyEventArgs e)
        {
            _currentSnapshot.KeyEventsList.Add(new KeyEvent(e.Key, true));
        }

        private void OnWindowResized(object sender, EventArgs e)
        {
            NeedsResizing = true;
        }

        public InputSnapshot GetInputSnapshot()
        {
            _currentSnapshot.Clear();
            _nativeWindow.ProcessEvents();
            return _currentSnapshot;
        }

        private class SimpleInputSnapshot : InputSnapshot
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
