using System.Diagnostics;

namespace Veldrid
{
    /// <summary>
    /// Represents a copy operation between a source and destination buffer.
    /// </summary>
    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    public struct BufferCopyCommand
    {
        /// <summary>
        /// An offset into the source at which the copy region begins.
        /// </summary>
        public ulong ReadOffset;

        /// <summary>
        /// An offset into the destination at which the data will be copied.
        /// </summary>
        public ulong WriteOffset;

        /// <summary>
        /// The number of bytes to copy.
        /// </summary>
        public ulong Length;

        public BufferCopyCommand(ulong readOffset, ulong writeOffset, ulong length)
        {
            ReadOffset = readOffset;
            WriteOffset = writeOffset;
            Length = length;
        }

        public override readonly string ToString()
        {
            return $"Copy {Length} from {ReadOffset} to {WriteOffset}";
        }

        private readonly string GetDebuggerDisplay()
        {
            return ToString();
        }
    }
}
