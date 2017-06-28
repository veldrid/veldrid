namespace Veldrid.Graphics.OpenGL
{
    public class OpenGLTextureBinding : ShaderTextureBinding
    {
        public OpenGLTexture BoundTexture { get; }
        DeviceTexture ShaderTextureBinding.BoundTexture => BoundTexture;

        public OpenGLTextureBinding(OpenGLTexture texture)
        {
            BoundTexture = texture;
        }

        public void Dispose()
        {
        }
    }
}
