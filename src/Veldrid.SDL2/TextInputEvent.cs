using System;
using System.Text;

namespace Veldrid.Sdl2
{
    public readonly ref struct TextInputEvent
    {
        public uint Timestamp { get; }
        public uint WindowID { get; }
        public ReadOnlySpan<Rune> Runes { get; }

        public TextInputEvent(uint timestamp, uint windowID, ReadOnlySpan<Rune> runes)
        {
            Timestamp = timestamp;
            WindowID = windowID;
            Runes = runes;
        }
    }
}
