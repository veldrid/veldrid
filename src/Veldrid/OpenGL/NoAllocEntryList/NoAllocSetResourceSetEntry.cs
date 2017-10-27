namespace Veldrid.OpenGL.NoAllocEntryList
{
    internal struct NoAllocSetResourceSetEntry
    {
        public readonly uint Slot;
        public readonly HandleTracked<ResourceSet> ResourceSet;

        public NoAllocSetResourceSetEntry(uint slot, ResourceSet rs)
        {
            Slot = slot;
            ResourceSet = rs;
        }
    }
}