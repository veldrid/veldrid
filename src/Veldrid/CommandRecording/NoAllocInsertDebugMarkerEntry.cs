namespace Veldrid.CommandRecording
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
