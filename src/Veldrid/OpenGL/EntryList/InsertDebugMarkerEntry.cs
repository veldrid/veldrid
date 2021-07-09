namespace Veldrid.OpenGL.NoAllocEntryList
{
    internal struct NoAllocInsertDebugMarkerEntry
    {
        public Tracked<string> Name;

        public NoAllocInsertDebugMarkerEntry(Tracked<string> name)
        {
            Name = name;
        }
    }
}
