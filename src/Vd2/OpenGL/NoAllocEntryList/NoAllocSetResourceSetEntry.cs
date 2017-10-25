namespace Vd2.OpenGL.NoAllocEntryList
{
    internal struct NoAllocSetResourceSetEntry
    {
        public readonly HandleTracked<ResourceSet> ResourceSet;

        public NoAllocSetResourceSetEntry(ResourceSet rs)
        {
            ResourceSet = rs;
        }
    }
}