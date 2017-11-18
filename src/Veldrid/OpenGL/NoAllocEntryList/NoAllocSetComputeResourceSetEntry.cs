namespace Veldrid.OpenGL.NoAllocEntryList
{
    internal struct NoAllocSetComputeResourceSetEntry
    {
        public readonly uint Slot;
        public readonly HandleTracked<ResourceSet> ResourceSet;

        public NoAllocSetComputeResourceSetEntry(uint slot, ResourceSet rs)
        {
            Slot = slot;
            ResourceSet = rs;
        }
    }
}