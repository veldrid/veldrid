namespace Veldrid.OpenGL
{
    internal class OpenGLTextureView : TextureView
    {
        public new OpenGLTexture Target { get; }

        public OpenGLTextureView(ref TextureViewDescription description)
            : base(description.Target)
        {
            Target = Util.AssertSubtype<Texture, OpenGLTexture>(description.Target);
        }

        public override void Dispose()
        {
        }
    }
}