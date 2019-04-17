using System;
using System.Collections.Generic;
using System.Numerics;

namespace Veldrid
{
    /// <summary>
    ///  A readonly view over an <see cref="InputState"/>.
    /// </summary>
    public class InputStateView
    {
        private readonly InputState _state;

        public IReadOnlyList<KeyEvent> KeyEvents => _state.KeyEvents;
        public IReadOnlyList<char> KeyCharPresses => _state.KeyCharPresses;
        public IReadOnlyList<MouseEvent> MouseEvents => _state.MouseEvents;
        public Vector2 MousePosition => _state.MousePosition;
        public Vector2 MouseDelta => _state.MouseDelta;
        public float WheelDelta => _state.WheelDelta;
        public IReadOnlyList<bool> MouseDown => _state.MouseDown;
        public bool IsMouseDown(MouseButton button) => MouseDown[(int)button];

        public InputStateView(InputState state)
        {
            _state = state;
        }
    }

    /// <summary>
    /// A mutable object representing the full user input state.
    /// </summary>
    public class InputState
    {
        public List<KeyEvent> KeyEvents { get; set; } = new List<KeyEvent>();
        public List<MouseEvent> MouseEvents { get; set; } = new List<MouseEvent>();
        public List<char> KeyCharPresses { get; set; } = new List<char>();
        public Vector2 MousePosition { get; set; }
        public Vector2 MouseDelta { get; set; }
        public float WheelDelta { get; set; }
        public bool[] MouseDown { get; private set; } = new bool[13];

        public InputStateView View { get; private set; }

        public InputState()
        {
            View = new InputStateView(this);
        }

        public void Clear()
        {
            KeyEvents.Clear();
            MouseEvents.Clear();
            KeyCharPresses.Clear();
            MousePosition = Vector2.Zero;
            MouseDelta = Vector2.Zero;
            WheelDelta = 0f;
            Array.Clear(MouseDown, 0, MouseDown.Length);
        }

        public void AddSnapshot(InputSnapshot snapshot)
        {
            for (int i = 0; i < snapshot.KeyEvents.Count; i++)
            {
                KeyEvents.Add(snapshot.KeyEvents[i]);
            }
            for (int i = 0; i < snapshot.MouseEvents.Count; i++)
            {
                MouseEvents.Add(snapshot.MouseEvents[i]);
            }
            for (int i = 0; i < snapshot.KeyCharPresses.Count; i++)
            {
                KeyCharPresses.Add(snapshot.KeyCharPresses[i]);
            }
            MousePosition = snapshot.MousePosition;
            WheelDelta = snapshot.WheelDelta;
            for (int i = 0; i < MouseDown.Length; i++)
            {
                MouseDown[i] = snapshot.IsMouseDown((MouseButton)i);
            }
        }
    }
}
