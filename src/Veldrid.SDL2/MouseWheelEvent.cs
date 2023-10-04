using System.Numerics;

namespace Veldrid.Sdl2
{
    public readonly struct MouseWheelEvent
    {
        public uint Timestamp { get; }
        public uint WindowID { get; }
        public Vector2 WheelDelta { get; }

        public MouseWheelEvent(uint timestamp, uint windowID, Vector2 wheelDelta)
        {
            Timestamp = timestamp;
            WindowID = windowID;
            WheelDelta = wheelDelta;
        }
    }
}
