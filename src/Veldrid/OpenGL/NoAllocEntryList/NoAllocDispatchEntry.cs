namespace Veldrid.OpenGL.NoAllocEntryList
{
    internal struct NoAllocDispatchEntry
    {
        public uint GroupCountX;
        public uint GroupCountY;
        public uint GroupCountZ;

        public NoAllocDispatchEntry(uint groupCountX, uint groupCountY, uint groupCountZ)
        {
            GroupCountX = groupCountX;
            GroupCountY = groupCountY;
            GroupCountZ = groupCountZ;
        }
    }
}