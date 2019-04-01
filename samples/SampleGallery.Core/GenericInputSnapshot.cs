using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;

namespace Veldrid.SampleGallery
{
    public class GenericInputSnapshot : InputSnapshot
    {
        public List<KeyEvent> KeyEventsList { get; private set; } = new List<KeyEvent>();
        public List<MouseEvent> MouseEventsList { get; private set; } = new List<MouseEvent>();
        public List<char> KeyCharPressesList { get; private set; } = new List<char>();

        public IReadOnlyList<KeyEvent> KeyEvents => KeyEventsList;

        public IReadOnlyList<MouseEvent> MouseEvents => MouseEventsList;

        public IReadOnlyList<char> KeyCharPresses => KeyCharPressesList;

        public Vector2 MousePosition { get; set; }

        private bool[] _mouseDown = new bool[13];
        public bool[] MouseDown => _mouseDown;
        public float WheelDelta { get; set; }

        public bool IsMouseDown(MouseButton button)
        {
            return _mouseDown[(int)button];
        }

        public void Clear()
        {
            KeyEventsList.Clear();
            MouseEventsList.Clear();
            KeyCharPressesList.Clear();
            WheelDelta = 0f;
        }

        public void CopyTo(GenericInputSnapshot other)
        {
            Debug.Assert(this != other);

            other.MouseEventsList.Clear();
            foreach (var me in MouseEventsList) { other.MouseEventsList.Add(me); }

            other.KeyEventsList.Clear();
            foreach (var ke in KeyEventsList) { other.KeyEventsList.Add(ke); }

            other.KeyCharPressesList.Clear();
            foreach (var kcp in KeyCharPressesList) { other.KeyCharPressesList.Add(kcp); }

            other.MousePosition = MousePosition;
            other.WheelDelta = WheelDelta;
            _mouseDown.CopyTo(other._mouseDown, 0);
        }
    }
}
