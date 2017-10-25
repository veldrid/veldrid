namespace Vd2.OpenGL.NoAllocEntryList
{
    internal struct NoAllocSetFramebufferEntry
    {
        public readonly HandleTracked<Framebuffer> Framebuffer;

        public NoAllocSetFramebufferEntry(Framebuffer fb)
        {
            Framebuffer = new HandleTracked<Framebuffer>(fb);
        }
    }
}