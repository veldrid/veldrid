using OpenTK.Graphics.OpenGL;
using System;

namespace Veldrid.Graphics.OpenGL
{
    public class OpenGLRasterizerState : RasterizerState
    {
        public FaceCullingMode CullMode { get; }
        public TriangleFillMode FillMode { get; }
        public bool IsDepthClipEnabled { get; }
        public bool IsScissorTestEnabled { get; }

        public OpenGLRasterizerState(
            FaceCullingMode cullMode,
            TriangleFillMode fillMode,
            bool depthClipEnabled,
            bool scissorTestEnabled)
        {
            CullMode = cullMode;
            FillMode = fillMode;
            IsDepthClipEnabled = depthClipEnabled;
            IsScissorTestEnabled = scissorTestEnabled;
        }

        public void Apply()
        {
            if (CullMode == FaceCullingMode.None)
            {
                GL.Disable(EnableCap.CullFace);
            }
            else
            {
                GL.Enable(EnableCap.CullFace);
                GL.CullFace(OpenGLFormats.ConvertCullMode(CullMode));
            }

            GL.PolygonMode(MaterialFace.FrontAndBack, OpenGLFormats.ConvertFillMode(FillMode));

            if (IsScissorTestEnabled)
            {
            }
            else
            {
                GL.Disable(EnableCap.ScissorTest);
            }

            if (IsDepthClipEnabled)
            {
                GL.Enable(EnableCap.DepthClamp);
            }
            else
            {
                GL.Disable(EnableCap.DepthClamp);
            }
        }

        public void Dispose()
        {
        }
    }
}
