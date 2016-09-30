using OpenTK.Graphics.OpenGL;
using System;

namespace Veldrid.Graphics.OpenGL
{
    public class OpenGLDepthStencilState : DepthStencilState, IDisposable
    {
        public OpenGLDepthStencilState(bool isDepthEnabled, DepthComparison comparison, bool isDepthWriteEnabled)
        {
            IsDepthEnabled = isDepthEnabled;
            DepthComparison = comparison;
            IsDepthWriteEnabled = isDepthWriteEnabled;
        }

        public DepthComparison DepthComparison { get; }

        public bool IsDepthEnabled { get; }

        public bool IsDepthWriteEnabled { get; }

        public void Apply()
        {
            if (IsDepthEnabled)
            {
                GL.Enable(EnableCap.DepthTest);
                GL.DepthFunc(OpenGLFormats.ConvertDepthComparison(DepthComparison));
                GL.DepthMask(true);
            }
            else
            {
                GL.Disable(EnableCap.DepthTest);
            }
        }

        public void Dispose()
        {
        }
    }

}
