using OpenTK.Graphics.ES30;
using System.Collections.Generic;
using System.Diagnostics;

namespace Veldrid.Graphics.OpenGLES
{
    public class OpenGLESSamplerState : SamplerState
    {
        private readonly int _samplerID;

        public SamplerAddressMode AddressU { get; }
        public SamplerAddressMode AddressV { get; }
        public SamplerAddressMode AddressW { get; }
        public SamplerFilter Filter { get; }
        public int MaximumAnisotropy { get; }
        public RgbaFloat BorderColor { get; }
        public DepthComparison Comparison { get; }
        public int MinimumLod { get; }
        public int MaximumLod { get; }
        public int LodBias { get; }

        public unsafe OpenGLESSamplerState(
            SamplerAddressMode addressU,
            SamplerAddressMode addressV,
            SamplerAddressMode addressW,
            SamplerFilter filter,
            int maxAnisotropy,
            RgbaFloat borderColor,
            DepthComparison comparison,
            int minLod,
            int maxLod,
            int lodBias)
        {
            AddressU = addressU;
            AddressV = addressV;
            AddressW = addressW;
            Filter = filter;
            MaximumAnisotropy = maxAnisotropy;
            BorderColor = borderColor;
            Comparison = comparison;
            MinimumLod = minLod;
            MaximumLod = maxLod;
            LodBias = lodBias;

            _samplerID = GL.GenSampler();
#pragma warning disable CS0618 // GL.SamplerParameter isn't actually obsolete.
            GL.SamplerParameter(_samplerID, SamplerParameterName.TextureWrapR, (int)OpenGLESFormats.VeldridToGLTextureWrapMode(addressU));
            GL.SamplerParameter(_samplerID, SamplerParameterName.TextureWrapS, (int)OpenGLESFormats.VeldridToGLTextureWrapMode(addressV));
            GL.SamplerParameter(_samplerID, SamplerParameterName.TextureWrapT, (int)OpenGLESFormats.VeldridToGLTextureWrapMode(addressW));

            GL.SamplerParameter(_samplerID, SamplerParameterName.TextureMinLod, MinimumLod);
            GL.SamplerParameter(_samplerID, SamplerParameterName.TextureMaxLod, MaximumLod);

            if (filter == SamplerFilter.Anisotropic || filter == SamplerFilter.ComparisonAnisotropic)
            {
                Debug.WriteLine("Anisotropic filtering is not supported on OpenGL ES.");
            }
            else
            {
                OpenGLESFormats.VeldridToGLTextureMinMagFilter(filter, out TextureMinFilter min, out TextureMagFilter mag);
                GL.SamplerParameter(_samplerID, SamplerParameterName.TextureMinFilter, (int)min);
                GL.SamplerParameter(_samplerID, SamplerParameterName.TextureMagFilter, (int)mag);
            }

            if (s_comparisonFilters.Contains(filter))
            {
                GL.SamplerParameter(_samplerID, SamplerParameterName.TextureCompareMode, (int)All.CompareRefToTexture);
                GL.SamplerParameter(_samplerID, SamplerParameterName.TextureCompareFunc, (int)OpenGLESFormats.ConvertDepthComparison(Comparison));
            }
#pragma warning restore CS0618
        }

        public void Apply(int textureUnit)
        {
            GL.BindSampler(textureUnit, _samplerID);
        }

        public void Dispose()
        {
            GL.DeleteSampler(_samplerID);
        }

        private static readonly HashSet<SamplerFilter> s_comparisonFilters = new HashSet<SamplerFilter>
        {
            SamplerFilter.ComparisonMinMagMipPoint,
            SamplerFilter.ComparisonMinMagPointMipLinear,
            SamplerFilter.ComparisonMinPointMagLinearMipPoint,
            SamplerFilter.ComparisonMinPointMagMipLinear,
            SamplerFilter.ComparisonMinLinearMagMipPoint,
            SamplerFilter.ComparisonMinLinearMagPointMipLinear,
            SamplerFilter.ComparisonMinMagLinearMipPoint,
            SamplerFilter.ComparisonMinMagMipLinear,
            SamplerFilter.ComparisonAnisotropic,
        };
    }
}
