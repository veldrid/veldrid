using System.Numerics;

namespace Veldrid.Sdl2
{
    public readonly struct MouseMoveEvent
    {
        public uint Timestamp { get; }
        public uint WindowID { get; }
        public Vector2 MousePosition { get; }
        public Vector2 Delta { get; }

        public MouseMoveEvent(uint timestamp, uint windowID, Vector2 mousePosition, Vector2 delta)
        {
            Timestamp = timestamp;
            WindowID = windowID;
            MousePosition = mousePosition;
            Delta = delta;
        }
    }
}
