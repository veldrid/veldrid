using WebGLDotNET;

namespace Veldrid.WebGL
{
    internal class WebGLSampler : Sampler
    {
        private WebGLGraphicsDevice _gd;

        public WebGLSampler(WebGLGraphicsDevice gd, ref SamplerDescription description)
        {
            _gd = gd;
        }

        public WebGLDotNET.WebGLSampler MipmapSampler { get; }
        public WebGLDotNET.WebGLSampler NoMipmapSampler { get; }
        public override string Name { get; set; }

        public override void Dispose()
        {
            MipmapSampler.Dispose();
            NoMipmapSampler.Dispose();
        }
    }
}
