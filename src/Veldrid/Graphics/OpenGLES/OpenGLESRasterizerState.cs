using OpenTK.Graphics.ES30;
using System;

namespace Veldrid.Graphics.OpenGLES
{
    public class OpenGLESRasterizerState : RasterizerState
    {
        public FaceCullingMode CullMode { get; }
        public TriangleFillMode FillMode { get; }
        public bool IsDepthClipEnabled { get; }
        public bool IsScissorTestEnabled { get; }

        public OpenGLESRasterizerState(
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
                Utilities.CheckLastGLES3Error();
            }
            else
            {
                GL.Enable(EnableCap.CullFace);
                Utilities.CheckLastGLES3Error();
                GL.CullFace(OpenGLESFormats.ConvertCullMode(CullMode));
                Utilities.CheckLastGLES3Error();
            }

            var mode = OpenGLESFormats.ConvertFillMode(FillMode);
            if (mode != PolygonMode.Fill)
            {
                throw new NotSupportedException();
            }

            if (IsScissorTestEnabled)
            {
            }
            else
            {
                GL.Disable(EnableCap.ScissorTest);
                Utilities.CheckLastGLES3Error();
            }

            // IsDepthClipEnabled is not really supported on OpenGL ES.
        }

        public void Dispose()
        {
        }
    }
}
