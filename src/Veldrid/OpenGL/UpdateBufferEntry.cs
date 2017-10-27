using System;

namespace Veldrid.OpenGL
{
    internal class UpdateBufferEntry : OpenGLCommandEntry
    {
        public Buffer Buffer;
        public uint BufferOffsetInBytes;
        public StagingBlock StagingBlock;

        public UpdateBufferEntry(Buffer buffer, uint bufferOffsetInBytes, StagingBlock stagingBlock)
        {
            Buffer = buffer;
            BufferOffsetInBytes = bufferOffsetInBytes;
            StagingBlock = stagingBlock;
        }

        public UpdateBufferEntry() { }

        public UpdateBufferEntry Init(Buffer buffer, uint bufferOffsetInBytes, StagingBlock stagingBlock)
        {
            Buffer = buffer;
            BufferOffsetInBytes = bufferOffsetInBytes;
            StagingBlock = stagingBlock;
            return this;
        }

        public override void ClearReferences()
        {
            Buffer = null;
        }
    }
}