using OpenTK.Graphics.OpenGL;

namespace Veldrid.Graphics.OpenGL
{
    public class OpenGLDepthStencilState : DepthStencilState
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
    }

}
