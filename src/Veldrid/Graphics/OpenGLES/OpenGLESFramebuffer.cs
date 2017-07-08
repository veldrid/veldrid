using OpenTK.Graphics.ES30;
using System;
using System.Diagnostics;

namespace Veldrid.Graphics.OpenGLES
{
    public class OpenGLESFramebuffer : OpenGLESFramebufferBase, Framebuffer, IDisposable
    {
        private readonly int _framebufferID;

        private OpenGLESTexture2D[] _colorTextures = new OpenGLESTexture2D[RenderContext.MaxRenderTargets];
        private OpenGLESTexture2D _depthTexture;

        public int Width => _colorTextures[0].Width;

        public int Height => _colorTextures[0].Height;

        DeviceTexture2D Framebuffer.ColorTexture
        {
            get { return GetColorTexture(0); }
            set { AttachColorTexture(0, value); }
        }

        DeviceTexture2D Framebuffer.DepthTexture
        {
            get
            {
                return _depthTexture;
            }
            set
            {
                if (value != null)
                {
                    Debug.Assert(value is OpenGLESTexture2D);
                    _depthTexture = (OpenGLESTexture2D)value;
                }
                else
                {
                    _depthTexture = null;
                }

                AttachDepthTexture();
            }
        }

        public bool HasDepthAttachment => _depthTexture != null;

        public OpenGLESFramebuffer()
        {
            _framebufferID = GL.GenFramebuffer();
            Utilities.CheckLastGLES3Error();
        }

        public OpenGLESFramebuffer(OpenGLESTexture2D colorTexture, OpenGLESTexture2D depthTexture)
            : this()
        {
            _depthTexture = depthTexture;

            Bind();
            AttachColorTexture(0, colorTexture);
            AttachDepthTexture();
            Unbind();
        }

        public DeviceTexture2D GetColorTexture(int index)
        {
            return _colorTextures[index];
        }

        public void AttachColorTexture(int index, DeviceTexture2D texture)
        {
            Bind();
            Debug.Assert(texture is OpenGLESTexture2D);
            OpenGLESTexture2D glTex = (OpenGLESTexture2D)texture;
            _colorTextures[index] = glTex;
            GL.ActiveTexture(TextureUnit.Texture0);
            Utilities.CheckLastGLES3Error();
            glTex.Bind();
            GL.FramebufferTexture2D(
                FramebufferTarget.Framebuffer,
                FramebufferAttachment.ColorAttachment0,
                TextureTarget2d.Texture2D,
                glTex.ID,
                0);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            Utilities.CheckLastGLES3Error();
            DrawBufferMode mode = DrawBufferMode.ColorAttachment0 + index;
            GL.DrawBuffers(1, ref mode);
            Utilities.CheckLastGLES3Error();

            Unbind();
        }

        private void AttachDepthTexture()
        {
            Bind();
            int depthTextureID;
            if (_depthTexture != null)
            {
                _depthTexture.Bind();
                depthTextureID = _depthTexture.ID;
            }
            else
            {
                depthTextureID = 0;
            }
            GL.FramebufferTexture2D(
                FramebufferTarget.Framebuffer,
                FramebufferAttachment.DepthAttachment,
                TextureTarget2d.Texture2D,
                depthTextureID,
                0);
            Unbind();
        }

        private void Bind()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, _framebufferID);
            Utilities.CheckLastGLES3Error();
        }

        private static void Unbind()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            Utilities.CheckLastGLES3Error();
        }

        internal override void Apply()
        {
            Bind();
            FramebufferErrorCode errorCode = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            Utilities.CheckLastGLES3Error();
            if (errorCode != FramebufferErrorCode.FramebufferComplete)
            {
                throw new VeldridException("Framebuffer was not complete: " + errorCode);
            }
        }

        public void Dispose()
        {
            GL.DeleteFramebuffer(_framebufferID);
            Utilities.CheckLastGLES3Error();
        }
    }
}
