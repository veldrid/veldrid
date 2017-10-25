using System;

namespace Vd2.OpenGL.NoAllocEntryList
{
    internal struct NoAllocUpdateBufferEntry
    {
        public readonly HandleTracked<Buffer> Buffer;
        public readonly uint BufferOffsetInBytes;
        public readonly StagingBlock StagingBlock;

        public NoAllocUpdateBufferEntry(Buffer buffer, uint bufferOffsetInBytes, StagingBlock stagingBlock)
        {
            Buffer = new HandleTracked<Buffer>(buffer);
            BufferOffsetInBytes = bufferOffsetInBytes;
            StagingBlock = stagingBlock;
        }
    }
}