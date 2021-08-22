using System;
using System.Numerics;
using System.Text;

namespace Veldrid
{
    public interface InputSnapshot
    {
        ReadOnlySpan<Rune> InputEvents { get; }
        ReadOnlySpan<KeyEvent> KeyEvents { get; }
        ReadOnlySpan<MouseButtonEvent> MouseEvents { get; }
        Vector2 MousePosition { get; }
        Vector2 WheelDelta { get; }
        MouseButton MouseDown { get; }
    }
}
