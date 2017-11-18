namespace Veldrid.OpenGL.NoAllocEntryList
{
    internal struct NoAllocSetGraphicsResourceSetEntry
    {
        public readonly uint Slot;
        public readonly HandleTracked<ResourceSet> ResourceSet;

        public NoAllocSetGraphicsResourceSetEntry(uint slot, ResourceSet rs)
        {
            Slot = slot;
            ResourceSet = rs;
        }
    }
}