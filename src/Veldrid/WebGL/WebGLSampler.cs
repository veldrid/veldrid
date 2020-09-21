using static Veldrid.WebGL.WebGLConstants;

namespace Veldrid.WebGL
{
    internal class WebGLSampler : Sampler
    {
        private readonly WebGLGraphicsDevice _gd;
        private readonly SamplerDescription _description;
        private bool _disposed;

        public WebGLSampler(WebGLGraphicsDevice gd, ref SamplerDescription description)
        {
            _description = description;
            _gd = gd;

            NoMipmapSampler = CreateSampler(mipmapped: false);
            MipmapSampler = CreateSampler(mipmapped: true);
        }

        public WebGLDotNET.WebGLSampler NoMipmapSampler { get; }
        public WebGLDotNET.WebGLSampler MipmapSampler { get; }
        public override string Name { get; set; }

        public override bool IsDisposed => _disposed;

        public override void Dispose()
        {
            NoMipmapSampler.Dispose();
            MipmapSampler.Dispose();
            _disposed = true;
        }

        private WebGLDotNET.WebGLSampler CreateSampler(bool mipmapped)
        {
            WebGLDotNET.WebGLSampler sampler = _gd.Ctx.CreateSampler();

            _gd.Ctx.SamplerParameteri(sampler, TEXTURE_WRAP_S, (int)WebGLUtil.VdToGLTextureWrapMode(_description.AddressModeU));
            _gd.CheckError();
            _gd.Ctx.SamplerParameteri(sampler, TEXTURE_WRAP_T, (int)WebGLUtil.VdToGLTextureWrapMode(_description.AddressModeV));
            _gd.CheckError();
            _gd.Ctx.SamplerParameteri(sampler, TEXTURE_WRAP_R, (int)WebGLUtil.VdToGLTextureWrapMode(_description.AddressModeW));
            _gd.CheckError();

            if (_description.AddressModeU == SamplerAddressMode.Border
                || _description.AddressModeV == SamplerAddressMode.Border
                || _description.AddressModeW == SamplerAddressMode.Border)
            {
                throw new VeldridException("WebGL does not support SamplerAddressMode.Border.");
            }

            _gd.Ctx.SamplerParameterf(sampler, TEXTURE_MIN_LOD, _description.MinimumLod);
            _gd.CheckError();
            _gd.Ctx.SamplerParameterf(sampler, TEXTURE_MAX_LOD, _description.MaximumLod);
            _gd.CheckError();

            WebGLUtil.VdToGLTextureMinMagFilter(_description.Filter, mipmapped, out uint min, out uint mag);
            _gd.Ctx.SamplerParameteri(sampler, TEXTURE_MIN_FILTER, (int)min);
            _gd.CheckError();
            _gd.Ctx.SamplerParameteri(sampler, TEXTURE_MAG_FILTER, (int)mag);
            _gd.CheckError();

            if (_description.ComparisonKind != null)
            {
                _gd.Ctx.SamplerParameteri(sampler, TEXTURE_COMPARE_MODE, (int)COMPARE_REF_TO_TEXTURE);
                _gd.CheckError();
                _gd.Ctx.SamplerParameteri(sampler, TEXTURE_COMPARE_FUNC, (int)WebGLUtil.VdToGLDepthFunction(_description.ComparisonKind.Value));
                _gd.CheckError();
            }

            return sampler;
        }
    }
}
