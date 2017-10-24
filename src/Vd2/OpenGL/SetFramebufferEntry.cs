namespace Vd2.OpenGL
{
    internal class SetFramebufferEntry : OpenGLCommandEntry
    {
        public readonly Framebuffer Framebuffer;

        public SetFramebufferEntry(Framebuffer fb)
        {
            Framebuffer = fb;
        }
    }
}