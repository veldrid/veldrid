using System;
using System.Text;

namespace Veldrid.Sdl2
{
    public readonly ref struct TextEditingEvent
    {
        public uint Timestamp { get; }
        public uint WindowID { get; }
        public ReadOnlySpan<Rune> Runes { get; }
        public int Offset { get; }
        public int Length { get; }

        public TextEditingEvent(uint timestamp, uint windowID, ReadOnlySpan<Rune> runes, int offset, int length)
        {
            Timestamp = timestamp;
            WindowID = windowID;
            Runes = runes;
            Offset = offset;
            Length = length;
        }
    }
}
