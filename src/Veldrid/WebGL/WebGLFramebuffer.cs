using static Veldrid.WebGL.WebGLConstants;

namespace Veldrid.WebGL
{
    internal class WebGLFramebuffer : Framebuffer
    {
        private readonly WebGLSwapchain _swapchain;

        public override string Name { get; set; }

        private readonly WebGLGraphicsDevice _gd;

        public WebGLDotNET.WebGLFramebuffer WglFramebuffer { get; }
        public bool IsSwapchainFramebuffer => _swapchain != null;

        public WebGLFramebuffer(WebGLGraphicsDevice gd, ref FramebufferDescription description)
            : base(description.DepthTarget, description.ColorTargets)
        {
            _gd = gd;
            WglFramebuffer = gd.Ctx.CreateFramebuffer();

            _gd.Ctx.BindFramebuffer(READ_FRAMEBUFFER, WglFramebuffer);
            _gd.CheckError();

            uint colorCount = (uint)ColorTargets.Count;

            if (colorCount > 0)
            {
                for (uint i = 0; i < colorCount; i++)
                {
                    FramebufferAttachment colorAttachment = ColorTargets[(int)i];
                    WebGLTexture glTex = Util.AssertSubtype<Texture, WebGLTexture>(colorAttachment.Target);

                    _gd.TextureSamplerManager.SetTextureTransient(glTex.Target, glTex.WglTexture);
                    _gd.CheckError();

                    if (glTex.ArrayLayers == 1)
                    {
                        _gd.Ctx.FramebufferTexture2D(
                            READ_FRAMEBUFFER,
                            COLOR_ATTACHMENT0 + i,
                            glTex.Target,
                            glTex.WglTexture,
                            (int)colorAttachment.MipLevel);
                        _gd.CheckError();
                    }
                    else
                    {
                        _gd.Ctx.FramebufferTextureLayer(
                            READ_FRAMEBUFFER,
                            COLOR_ATTACHMENT0 + i,
                            glTex.WglTexture,
                            (int)colorAttachment.MipLevel,
                            (int)colorAttachment.ArrayLayer);
                    }
                }
            }

            WebGLDotNET.WebGLTexture wglDepthTex = null;
            uint depthTarget = TEXTURE_2D;
            if (DepthTarget != null)
            {
                WebGLTexture glDepthTex = Util.AssertSubtype<Texture, WebGLTexture>(DepthTarget.Value.Target);
                depthTarget = glDepthTex.Target;

                wglDepthTex = glDepthTex.WglTexture;

                _gd.TextureSamplerManager.SetTextureTransient(depthTarget, glDepthTex.WglTexture);
                _gd.CheckError();

                uint framebufferAttachment = DEPTH_ATTACHMENT;
                if (FormatHelpers.IsStencilFormat(glDepthTex.Format))
                {
                    framebufferAttachment = DEPTH_STENCIL_ATTACHMENT;
                }

                if (glDepthTex.ArrayLayers == 1)
                {
                    _gd.Ctx.FramebufferTexture2D(
                        READ_FRAMEBUFFER,
                        framebufferAttachment,
                        depthTarget,
                        wglDepthTex,
                        (int)DepthTarget.Value.MipLevel);
                    _gd.CheckError();
                }
                else
                {
                    _gd.Ctx.FramebufferTextureLayer(
                        READ_FRAMEBUFFER,
                        framebufferAttachment,
                        wglDepthTex,
                        (int)DepthTarget.Value.MipLevel,
                        (int)DepthTarget.Value.ArrayLayer);
                    _gd.CheckError();
                }
            }

            uint status = _gd.Ctx.CheckFramebufferStatus(READ_FRAMEBUFFER);
            _gd.CheckError();
            if (status != FRAMEBUFFER_COMPLETE)
            {
                throw new VeldridException("Framebuffer was not successfully created: " + status);
            }
        }

        public override void Dispose()
        {
            WglFramebuffer.Dispose();
        }
    }
}
