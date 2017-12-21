namespace Veldrid.OpenGL.NoAllocEntryList
{
    internal struct NoAllocSetFramebufferEntry
    {
        public readonly Tracked<Framebuffer> Framebuffer;

        public NoAllocSetFramebufferEntry(Tracked<Framebuffer> fb)
        {
            Framebuffer = fb;
        }
    }
}