namespace Vd2.OpenGL
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