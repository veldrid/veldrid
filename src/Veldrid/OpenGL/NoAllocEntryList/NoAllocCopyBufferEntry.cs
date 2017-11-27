namespace Veldrid.OpenGL.NoAllocEntryList
{
    internal struct NoAllocCopyBufferEntry
    {
        public readonly HandleTracked<Buffer> Source;
        public readonly uint SourceOffset;
        public readonly HandleTracked<Buffer> Destination;
        public readonly uint DestinationOffset;
        public readonly uint SizeInBytes;

        public NoAllocCopyBufferEntry(Buffer source, uint sourceOffset, Buffer destination, uint destinationOffset, uint sizeInBytes)
        {
            Source = source;
            SourceOffset = sourceOffset;
            Destination = destination;
            DestinationOffset = destinationOffset;
            SizeInBytes = sizeInBytes;
        }
    }
}