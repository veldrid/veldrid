namespace Veldrid.OpenGL.NoAllocEntryList
{
    internal struct NoAllocCopyBufferEntry
    {
        public readonly HandleTracked<DeviceBuffer> Source;
        public readonly uint SourceOffset;
        public readonly HandleTracked<DeviceBuffer> Destination;
        public readonly uint DestinationOffset;
        public readonly uint SizeInBytes;

        public NoAllocCopyBufferEntry(DeviceBuffer source, uint sourceOffset, DeviceBuffer destination, uint destinationOffset, uint sizeInBytes)
        {
            Source = source;
            SourceOffset = sourceOffset;
            Destination = destination;
            DestinationOffset = destinationOffset;
            SizeInBytes = sizeInBytes;
        }
    }
}