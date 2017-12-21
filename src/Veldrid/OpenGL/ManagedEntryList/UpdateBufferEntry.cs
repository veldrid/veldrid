using System;

namespace Veldrid.OpenGL.ManagedEntryList
{
    internal class UpdateBufferEntry : OpenGLCommandEntry
    {
        public DeviceBuffer Buffer;
        public uint BufferOffsetInBytes;
        public StagingBlock StagingBlock;

        public UpdateBufferEntry(DeviceBuffer buffer, uint bufferOffsetInBytes, StagingBlock stagingBlock)
        {
            Buffer = buffer;
            BufferOffsetInBytes = bufferOffsetInBytes;
            StagingBlock = stagingBlock;
        }

        public UpdateBufferEntry() { }

        public UpdateBufferEntry Init(DeviceBuffer buffer, uint bufferOffsetInBytes, StagingBlock stagingBlock)
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