using System;
using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;

namespace Veldrid.Graphics.OpenGL
{
    public class OpenGLSamplerState : SamplerState
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

        public unsafe OpenGLSamplerState(
            SamplerAddressMode addressU,
            SamplerAddressMode addressV,
            SamplerAddressMode addressW,
            SamplerFilter filter,
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
            BorderColor = borderColor;
            Comparison = comparison;
            MinimumLod = minLod;
            MaximumLod = maxLod;
            LodBias = lodBias;

            _samplerID = GL.GenSampler();
#pragma warning disable CS0618 // GL.SamplerParameter isn't actually obsolete.
            GL.SamplerParameter(_samplerID, SamplerParameter.TextureBorderColor, (float*)&borderColor);
            GL.SamplerParameter(_samplerID, SamplerParameter.TextureWrapR, (int)OpenGLFormats.VeldridToGLTextureWrapMode(addressU));
            GL.SamplerParameter(_samplerID, SamplerParameter.TextureWrapS, (int)OpenGLFormats.VeldridToGLTextureWrapMode(addressV));
            GL.SamplerParameter(_samplerID, SamplerParameter.TextureWrapT, (int)OpenGLFormats.VeldridToGLTextureWrapMode(addressW));

            GL.SamplerParameter(_samplerID, SamplerParameter.TextureMinLod, MinimumLod);
            GL.SamplerParameter(_samplerID, SamplerParameter.TextureMaxLod, MaximumLod);
            GL.SamplerParameter(_samplerID, SamplerParameter.TextureLodBias, LodBias);

            if (filter == SamplerFilter.Anisotropic || filter == SamplerFilter.ComparisonAnisotropic)
            {
                GL.SamplerParameter(_samplerID, SamplerParameter.TextureMaxAnisotropyExt, (float)MaximumAnisotropy);
            }
            else
            {
                OpenGLFormats.VeldridToGLTextureMinMagFilter(filter, out TextureMinFilter min, out TextureMagFilter mag);
                GL.SamplerParameter(_samplerID, SamplerParameter.TextureMinFilter, (int)min);
                GL.SamplerParameter(_samplerID, SamplerParameter.TextureMagFilter, (int)mag);
            }

            if (s_comparisonFilters.Contains(filter))
            {
                GL.SamplerParameter(_samplerID, SamplerParameter.TextureCompareMode, (int)TextureCompareMode.CompareRefToTexture);
                GL.SamplerParameter(_samplerID, SamplerParameter.TextureCompareFunc, (int)OpenGLFormats.ConvertDepthComparison(Comparison));
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
