using System;

namespace Vd2.OpenGL
{
    internal class UpdateBufferEntry : OpenGLCommandEntry
    {
        public readonly Buffer Buffer;
        public readonly uint BufferOffsetInBytes;
        public readonly StagingBlock StagingBlock;

        public UpdateBufferEntry(Buffer buffer, uint bufferOffsetInBytes, StagingBlock stagingBlock)
        {
            Buffer = buffer;
            BufferOffsetInBytes = bufferOffsetInBytes;
            StagingBlock = stagingBlock;
        }
    }
}