using OpenTK.Graphics.OpenGL;
using System;

namespace Veldrid.Graphics.OpenGL
{
    public class OpenGLDepthStencilState : DepthStencilState, IDisposable
    {
        public OpenGLDepthStencilState(bool isDepthEnabled, DepthComparison comparison)
        {
            IsDepthEnabled = isDepthEnabled;
            DepthComparison = comparison;
        }

        public DepthComparison DepthComparison { get; }

        public bool IsDepthEnabled { get; }

        public void Apply()
        {
            if (IsDepthEnabled)
            {
                GL.Enable(EnableCap.DepthTest);
                GL.DepthFunc(OpenGLFormats.ConvertDepthComparison(DepthComparison));
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
