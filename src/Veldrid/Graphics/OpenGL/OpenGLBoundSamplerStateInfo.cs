namespace Veldrid.Graphics.OpenGL
{
    public struct OpenGLBoundSamplerStateInfo
    {
        public OpenGLSamplerState SamplerState { get; }
        public bool Mipmapped { get; }

        public OpenGLBoundSamplerStateInfo(OpenGLSamplerState samplerState, bool mipmapped)
        {
            SamplerState = samplerState;
            Mipmapped = mipmapped;
        }
    }
}
