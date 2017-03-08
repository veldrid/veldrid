using OpenTK.Graphics.ES30;
using System;

namespace Veldrid.Graphics.OpenGLES
{
    public class OpenGLESDepthStencilState : DepthStencilState, IDisposable
    {
        public OpenGLESDepthStencilState(bool isDepthEnabled, DepthComparison comparison, bool isDepthWriteEnabled)
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
                Utilities.CheckLastGLES3Error();
                GL.DepthFunc(OpenGLESFormats.ConvertDepthComparison(DepthComparison));
                Utilities.CheckLastGLES3Error();
                GL.DepthMask(true);
                Utilities.CheckLastGLES3Error();
            }
            else
            {
                GL.Disable(EnableCap.DepthTest);
                Utilities.CheckLastGLES3Error();
            }
        }

        public void Dispose()
        {
        }
    }

}
