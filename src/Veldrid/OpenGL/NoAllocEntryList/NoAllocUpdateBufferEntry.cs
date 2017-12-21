using System;

namespace Veldrid.OpenGL.NoAllocEntryList
{
    internal struct NoAllocUpdateBufferEntry
    {
        public readonly HandleTracked<DeviceBuffer> Buffer;
        public readonly uint BufferOffsetInBytes;
        public readonly HandleTrackedStagingBlock StagingBlock;

        public NoAllocUpdateBufferEntry(DeviceBuffer buffer, uint bufferOffsetInBytes, StagingBlock stagingBlock)
        {
            Buffer = new HandleTracked<DeviceBuffer>(buffer);
            BufferOffsetInBytes = bufferOffsetInBytes;
            StagingBlock = new HandleTrackedStagingBlock(stagingBlock);
        }
    }
}