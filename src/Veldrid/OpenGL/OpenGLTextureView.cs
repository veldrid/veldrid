namespace Veldrid.OpenGL
{
    internal class OpenGLTextureView : TextureView
    {
        public OpenGLTextureView(ref TextureViewDescription description)
            : base(description.Target)
        {
        }

        public override void Dispose()
        {
        }
    }
}