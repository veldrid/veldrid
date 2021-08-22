namespace Veldrid
{
    public readonly struct MouseButtonEvent
    {
        public uint Timestamp { get; }
        public uint WindowID { get; }
        public MouseButton MouseButton { get; }
        public bool Down { get; }
        public byte Clicks { get; }

        public MouseButtonEvent(uint timestamp, uint windowID, MouseButton mouseButton, bool down, byte clicks)
        {
            Timestamp = timestamp;
            WindowID = windowID;
            MouseButton = mouseButton;
            Down = down;
            Clicks = clicks;
        }
    }
}
