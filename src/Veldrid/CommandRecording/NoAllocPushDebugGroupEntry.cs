namespace Veldrid.CommandRecording
{
    internal struct NoAllocPushDebugGroupEntry
    {
        public Tracked<string> Name;

        public NoAllocPushDebugGroupEntry(Tracked<string> name)
        {
            Name = name;
        }
    }
}
