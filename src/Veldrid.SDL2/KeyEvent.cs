namespace Veldrid
{
    public readonly struct KeyEvent
    {
        public uint Timestamp { get; }
        public uint WindowID { get; }
        public bool Down { get; }
        public bool Repeat { get; }
        public Key Physical { get; }
        public VKey Virtual { get; }
        public ModifierKeys Modifiers { get; }

        public KeyEvent(
            uint timestamp, uint windowID,
            bool down, bool repeat, Key physical, VKey @virtual, ModifierKeys modifiers)
        {
            Timestamp = timestamp;
            WindowID = windowID;
            Down = down;
            Repeat = repeat;
            Physical = physical;
            Virtual = @virtual;
            Modifiers = modifiers;
        }

        public override string ToString()
        {
            return $"{Physical}->{Virtual} {(Down ? "Down" : "Up") + (Repeat ? " Repeat" : "")} [{Modifiers}]";
        }
    }
}
