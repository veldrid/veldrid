using System;

namespace Veldrid.Graphics.OpenGL
{
    public class OpenGLTextureBinding : ShaderTextureBinding
    {
        private readonly OpenGLTexture _texture;
        public DeviceTexture BoundTexture => _texture;

        public OpenGLTextureBinding(OpenGLTexture texture)
        {
            _texture = texture;
        }

        public void Dispose()
        {
        }
    }
}
