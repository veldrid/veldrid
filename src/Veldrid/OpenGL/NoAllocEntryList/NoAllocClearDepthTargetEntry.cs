namespace Veldrid.OpenGL.NoAllocEntryList
{
    internal struct NoAllocClearDepthTargetEntry
    {
        public readonly float Depth;
        public readonly byte Stencil;

        public NoAllocClearDepthTargetEntry(float depth, byte stencil)
        {
            Depth = depth;
            Stencil = stencil;
        }
    }
}