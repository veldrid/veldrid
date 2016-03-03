using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Numerics;

namespace Veldrid.Graphics
{
    public interface WindowInputProvider
    {
        InputSnapshot GetInputSnapshot();
    }

    public interface InputSnapshot
    {
        IReadOnlyCollection<KeyEvent> KeyEvents { get; }
        IReadOnlyCollection<MouseEvent> MouseEvents { get; }
        Vector2 MousePosition { get; }
    }
}