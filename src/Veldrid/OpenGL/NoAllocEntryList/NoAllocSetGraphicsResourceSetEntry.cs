namespace Veldrid.OpenGL.NoAllocEntryList
{
    internal struct NoAllocSetGraphicsResourceSetEntry
    {
        public readonly uint Slot;
        public readonly Tracked<ResourceSet> ResourceSet;

        public NoAllocSetGraphicsResourceSetEntry(uint slot, Tracked<ResourceSet> rs)
        {
            Slot = slot;
            ResourceSet = rs;
        }
    }
}