using System;
using Veldrid.OpenGL;

namespace Veldrid
{
    public class BackendInfoOpenGL
    {
        private readonly OpenGLGraphicsDevice _gd;

        internal BackendInfoOpenGL(OpenGLGraphicsDevice gd)
        {
            _gd = gd;
        }

        public void ExecuteOnGLThread(Action action) => _gd.ExecuteOnGLThread(action);

        public void FlushAndFinish() => _gd.FlushAndFinish();

        public uint GetTextureName(Texture texture)
        {
            return Util.AssertSubtype<Texture, OpenGLTexture>(texture).Texture;
        }
    }
}
