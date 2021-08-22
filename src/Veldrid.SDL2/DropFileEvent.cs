using System;

namespace Veldrid.Sdl2
{
    public readonly ref struct DropFileEvent
    {
        /// <summary>
        /// The dropped file name in UTF8. 
        /// </summary>
        public ReadOnlySpan<byte> FileNameUtf8 { get; }

        /// <summary>
        /// Timestamp of the event.
        /// </summary>
        public uint Timestamp { get; }

        /// <summary>
        /// The window that was dropped on, if any.
        /// </summary>
        public uint WindowID { get; }

        public DropFileEvent(ReadOnlySpan<byte> fileNameUtf8, uint timestamp, uint windowID)
        {
            FileNameUtf8 = fileNameUtf8;
            Timestamp = timestamp;
            WindowID = windowID;
        }
    }
}
