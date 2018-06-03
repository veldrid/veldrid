namespace Veldrid.OpenGL.NoAllocEntryList
{
    internal struct NoAllocUpdateBufferEntry
    {
        public readonly Tracked<DeviceBuffer> Buffer;
        public readonly uint BufferOffsetInBytes;
        public readonly StagingBlock StagingBlock;
        public readonly uint StagingBlockSize;

        public NoAllocUpdateBufferEntry(Tracked<DeviceBuffer> buffer, uint bufferOffsetInBytes, StagingBlock stagingBlock, uint stagingBlockSize)
        {
            Buffer = buffer;
            BufferOffsetInBytes = bufferOffsetInBytes;
            StagingBlock = stagingBlock;
            StagingBlockSize = stagingBlockSize;
        }
    }
}