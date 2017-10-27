namespace Veldrid.OpenGL.NoAllocEntryList
{
    internal struct NoAllocClearColorTargetEntry
    {
        public readonly uint Index;
        public readonly RgbaFloat ClearColor;

        public NoAllocClearColorTargetEntry(uint index, RgbaFloat clearColor)
        {
            Index = index;
            ClearColor = clearColor;
        }
    }
}