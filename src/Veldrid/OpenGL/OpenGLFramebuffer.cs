using static Veldrid.OpenGLBinding.OpenGLNative;
using static Veldrid.OpenGL.OpenGLUtil;
using Veldrid.OpenGLBinding;

namespace Veldrid.OpenGL
{
    internal unsafe class OpenGLFramebuffer : Framebuffer, OpenGLDeferredResource
    {
        private readonly OpenGLGraphicsDevice _gd;
        private uint _framebuffer;

        private string _name;
        private bool _nameChanged;
        public string Name { get => _name; set { _name = value; _nameChanged = true; } }

        public uint Framebuffer => _framebuffer;

        public bool Created { get; private set; }

        public OpenGLFramebuffer(OpenGLGraphicsDevice gd, ref FramebufferDescription description)
            : base(description.DepthTarget, description.ColorTargets)
        {
            _gd = gd;
        }

        public void EnsureResourcesCreated()
        {
            if (!Created)
            {
                CreateGLResources();
            }
            if (_nameChanged)
            {
                _nameChanged = false;
                SetObjectLabel(ObjectLabelIdentifier.Framebuffer, _framebuffer, _name);
            }
        }

        public void CreateGLResources()
        {
            glGenFramebuffers(1, out _framebuffer);
            CheckLastError();

            glBindFramebuffer(FramebufferTarget.Framebuffer, _framebuffer);
            CheckLastError();

            uint colorCount = (uint)ColorTextures.Count;

            if (colorCount > 0)
            {
                for (int i = 0; i < colorCount; i++)
                {
                    Texture2D target = ColorTextures[i];
                    OpenGLTexture2D glTex2D = Util.AssertSubtype<Texture2D, OpenGLTexture2D>(target);
                    glTex2D.EnsureResourcesCreated();

                    glActiveTexture(TextureUnit.Texture0);
                    CheckLastError();

                    glBindTexture(TextureTarget.Texture2D, glTex2D.Texture);
                    CheckLastError();

                    glFramebufferTexture2D(
                        FramebufferTarget.Framebuffer,
                        FramebufferAttachment.ColorAttachment0 + i,
                        TextureTarget.Texture2D,
                        glTex2D.Texture,
                        0);
                    CheckLastError();
                }

                DrawBuffersEnum* bufs = stackalloc DrawBuffersEnum[(int)colorCount];
                for (int i = 0; i < colorCount; i++)
                {
                    bufs[i] = DrawBuffersEnum.ColorAttachment0 + i;
                }
                glDrawBuffers(colorCount, bufs);
                CheckLastError();
            }

            uint depthTextureID = 0;
            if (DepthTexture != null)
            {
                OpenGLTexture2D glDepthTex = Util.AssertSubtype<Texture2D, OpenGLTexture2D>(DepthTexture);
                glDepthTex.EnsureResourcesCreated();

                depthTextureID = glDepthTex.Texture;
                glActiveTexture(TextureUnit.Texture0);
                CheckLastError();

                glBindTexture(TextureTarget.Texture2D, glDepthTex.Texture);
                CheckLastError();
            }

            glFramebufferTexture2D(
                FramebufferTarget.Framebuffer,
                FramebufferAttachment.DepthAttachment,
                TextureTarget.Texture2D,
                depthTextureID,
                0);
            CheckLastError();

            FramebufferErrorCode errorCode = glCheckFramebufferStatus(FramebufferTarget.Framebuffer);
            CheckLastError();
            if (errorCode != FramebufferErrorCode.FramebufferComplete)
            {
                throw new VeldridException("Framebuffer was not successfully created: " + errorCode);
            }

            Created = true;
        }

        public override void Dispose()
        {
            _gd.EnqueueDisposal(this);
        }

        public void DestroyGLResources()
        {
            uint framebuffer = _framebuffer;
            glDeleteFramebuffers(1, ref framebuffer);
            CheckLastError();
        }
    }
}