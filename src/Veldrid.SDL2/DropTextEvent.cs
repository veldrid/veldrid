using System;

namespace Veldrid.Sdl2
{
    public ref struct DropTextEvent
    {
        /// <summary>
        /// The dropped text in UTF8. 
        /// </summary>
        public ReadOnlySpan<byte> TextUtf8 { get; }

        /// <summary>
        /// Timestamp of the event.
        /// </summary>
        public uint Timestamp { get; }

        /// <summary>
        /// The window that was dropped on, if any.
        /// </summary>
        public uint WindowID { get; }

        public DropTextEvent(ReadOnlySpan<byte> textUtf8, uint timestamp, uint windowID)
        {
            TextUtf8 = textUtf8;
            Timestamp = timestamp;
            WindowID = windowID;
        }
    }
}
