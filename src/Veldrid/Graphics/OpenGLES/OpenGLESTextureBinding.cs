namespace Veldrid.Graphics.OpenGLES
{
    public class OpenGLESTextureBinding : ShaderTextureBinding
    {
        private readonly OpenGLESTexture _texture;
        public DeviceTexture BoundTexture => _texture;

        public OpenGLESTextureBinding(OpenGLESTexture texture)
        {
            _texture = texture;
        }

        public void Dispose()
        {
        }
    }
}
