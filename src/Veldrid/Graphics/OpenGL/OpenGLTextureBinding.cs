namespace Veldrid.Graphics.OpenGL
{
    public class OpenGLTextureBinding : ShaderTextureBinding
    {
        private readonly OpenGLTexture2D _texture;
        public DeviceTexture BoundTexture => _texture;

        public OpenGLTextureBinding(OpenGLTexture2D texture)
        {
            _texture = texture;
        }
        
        public OpenGLTextureBinding(OpenGLCubemapTexture texture)
        {
            
        }

        public void Dispose()
        {
        }
    }
}
