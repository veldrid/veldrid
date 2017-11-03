namespace Veldrid.OpenGL.ManagedEntryList
{
    internal class SetFramebufferEntry : OpenGLCommandEntry
    {
        public Framebuffer Framebuffer;

        public SetFramebufferEntry(Framebuffer fb)
        {
            Framebuffer = fb;
        }

        public SetFramebufferEntry() { }

        public SetFramebufferEntry Init(Framebuffer fb)
        {
            Framebuffer = fb;
            return this;
        }

        public override void ClearReferences()
        {
            Framebuffer = null;
        }
    }
}