#if !EXCLUDE_OPENGL_BACKEND
using System;
using Veldrid.OpenGL;
using Veldrid.OpenGLBinding;

namespace Veldrid
{
    /// <summary>
    /// Exposes OpenGL-specific functionality, useful for interoperating with native components which interface directly with
    /// OpenGL. Can only be used on a GraphicsDevice whose GraphicsBackend is OpenGL.
    /// </summary>
    public class BackendInfoOpenGL
    {
        private readonly OpenGLGraphicsDevice _gd;

        internal BackendInfoOpenGL(OpenGLGraphicsDevice gd)
        {
            _gd = gd;
        }

        /// <summary>
        /// Executes the given delegate in the OpenGL device's main execution thread. In the delegate, OpenGL commands can be
        /// executed directly. This method does not return until the delegate's execution is fully completed.
        /// </summary>
        public void ExecuteOnGLThread(Action action) => _gd.ExecuteOnGLThread(action);

        /// <summary>
        /// Executes a glFlush and a glFinish command, and waits for their completion.
        /// </summary>
        public void FlushAndFinish() => _gd.FlushAndFinish();

        /// <summary>
        /// Gets the name of the OpenGL texture object wrapped by the given Veldrid Texture.
        /// </summary>
        /// <returns>The Veldrid Texture's underlying OpenGL texture name.</returns>
        public uint GetTextureName(Texture texture) => Util.AssertSubtype<Texture, OpenGLTexture>(texture).Texture;

        /// <summary>
        /// Sets the texture target of the OpenGL texture object wrapped by the given Veldrid Texture to to a custom value.
        /// This could be used to set platform specific texture target values like Veldrid.OpenGLBinding.TextureTarget.TextureExternalOes.
        /// </summary>
        public void SetTextureTarget(Texture texture, uint textureTarget) => Util.AssertSubtype<Texture, OpenGLTexture>(texture).TextureTarget = (TextureTarget)textureTarget;

    }
}
#endif
