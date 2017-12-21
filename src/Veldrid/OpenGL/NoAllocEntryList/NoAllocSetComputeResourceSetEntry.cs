namespace Veldrid.OpenGL.NoAllocEntryList
{
    internal struct NoAllocSetComputeResourceSetEntry
    {
        public readonly uint Slot;
        public readonly Tracked<ResourceSet> ResourceSet;

        public NoAllocSetComputeResourceSetEntry(uint slot, Tracked<ResourceSet> rs)
        {
            Slot = slot;
            ResourceSet = rs;
        }
    }
}