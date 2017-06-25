using OpenTK.Graphics.ES30;
using System.Collections.Generic;
using System;

namespace Veldrid.Graphics.OpenGLES
{
    public class OpenGLESSamplerState : SamplerState
    {
        private InternalSamplerState _mipmapState;
        private InternalSamplerState _noMipmapState;

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
        }

        private InternalSamplerState GetInternalState(bool mipmap)
        {
            if (mipmap)
            {
                return _mipmapState ?? (_mipmapState = CreateInternalState(mipmap));
            }
            else
            {
                return _noMipmapState ?? (_noMipmapState = CreateInternalState(mipmap));
            }
        }

        private InternalSamplerState CreateInternalState(bool mipmap)
        {
            return new InternalSamplerState(
                AddressU, AddressV, AddressW, Filter, MaximumAnisotropy, BorderColor, Comparison, MinimumLod, MaximumLod, LodBias, mipmap);
        }

        public void Apply(int textureUnit, bool mipmap)
        {
            InternalSamplerState internalState = GetInternalState(mipmap);
            internalState.Bind(textureUnit);
        }

        public void Dispose()
        {
            _mipmapState?.Dispose();
            _noMipmapState?.Dispose();
        }

        /// <summary>
        /// Encapsulates a single sampler state. Each OpenGLSamplerState uses two InternalSamplerState objects to represent
        /// a mipmapped and non-mipmapped sampler, and binds one depending on whether the current texture has mipmaps.
        /// </summary>
        private class InternalSamplerState : IDisposable
        {
            private readonly int _samplerID;

            public unsafe InternalSamplerState(
                SamplerAddressMode addressU,
                SamplerAddressMode addressV,
                SamplerAddressMode addressW,
                SamplerFilter filter,
                int maxAnisotropy,
                RgbaFloat borderColor,
                DepthComparison comparison,
                int minLod,
                int maxLod,
                int lodBias,
                bool mip)
            {
                _samplerID = GL.GenSampler();

                GL.SamplerParameter(_samplerID, SamplerParameterName.TextureWrapR, (int)OpenGLESFormats.VeldridToGLTextureWrapMode(addressU));
                Utilities.CheckLastGLError();
                GL.SamplerParameter(_samplerID, SamplerParameterName.TextureWrapS, (int)OpenGLESFormats.VeldridToGLTextureWrapMode(addressV));
                Utilities.CheckLastGLError();
                GL.SamplerParameter(_samplerID, SamplerParameterName.TextureWrapT, (int)OpenGLESFormats.VeldridToGLTextureWrapMode(addressW));
                Utilities.CheckLastGLError();

                if (addressU == SamplerAddressMode.Border || addressV == SamplerAddressMode.Border || addressW == SamplerAddressMode.Border)
                {
#pragma warning disable CS0618 // TextureBorderColor is not exposed on SamplerParameterName
                    GL.SamplerParameter(_samplerID, All.TextureBorderColor, (float*)&borderColor);
#pragma warning restore CS0618
                    Utilities.CheckLastGLError();
                }

                GL.SamplerParameter(_samplerID, SamplerParameterName.TextureMinLod, (float)minLod);
                Utilities.CheckLastGLError();
                GL.SamplerParameter(_samplerID, SamplerParameterName.TextureMaxLod, (float)maxLod);
                Utilities.CheckLastGLError();

                if (filter == SamplerFilter.Anisotropic || filter == SamplerFilter.ComparisonAnisotropic)
                {
#pragma warning disable CS0618 // TextureMaxAnisotropyExt is not exposed on SamplerParameterName
                    GL.SamplerParameter(_samplerID, All.TextureMaxAnisotropyExt, (float)maxAnisotropy);
#pragma warning restore CS0618 // Type or member is obsolete
                    Utilities.CheckLastGLError();
                    GL.SamplerParameter(_samplerID, SamplerParameterName.TextureMinFilter, mip ? (int)TextureMinFilter.LinearMipmapLinear : (int)TextureMinFilter.Linear);
                    Utilities.CheckLastGLError();
                    GL.SamplerParameter(_samplerID, SamplerParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                    Utilities.CheckLastGLError();
                }
                else
                {
                    OpenGLESFormats.VeldridToGLTextureMinMagFilter(filter, mip, out TextureMinFilter min, out TextureMagFilter mag);
                    GL.SamplerParameter(_samplerID, SamplerParameterName.TextureMinFilter, (int)min);
                    Utilities.CheckLastGLError();
                    GL.SamplerParameter(_samplerID, SamplerParameterName.TextureMagFilter, (int)mag);
                    Utilities.CheckLastGLError();
                }

                if (s_comparisonFilters.Contains(filter))
                {
                    GL.SamplerParameter(_samplerID, SamplerParameterName.TextureCompareMode, (int)All.CompareRefToTexture);
                    Utilities.CheckLastGLError();
                    GL.SamplerParameter(_samplerID, SamplerParameterName.TextureCompareFunc, (int)OpenGLESFormats.ConvertDepthComparison(comparison));
                    Utilities.CheckLastGLError();
                }
            }

            public void Bind(int textureUnit)
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
}
