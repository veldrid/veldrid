namespace Veldrid.WebGL
{
    /// <summary>
    /// A utility class managing the relationships between textures, samplers, and their binding locations.
    /// </summary>
    internal unsafe class WebGLTextureSamplerManager
    {
        private readonly WebGLGraphicsDevice _gd;
        private readonly int _maxTextureUnits;
        private readonly uint _lastTextureUnit;
        private readonly WebGLTextureView[] _textureUnitTextures;
        private readonly BoundSamplerStateInfo[] _textureUnitSamplers;
        private uint _currentActiveUnit = 0;

        public WebGLTextureSamplerManager(WebGLGraphicsDevice gd)
        {
            _gd = gd;
            _maxTextureUnits = 8; // TODO
            _textureUnitTextures = new WebGLTextureView[_maxTextureUnits];
            _textureUnitSamplers = new BoundSamplerStateInfo[_maxTextureUnits];

            _lastTextureUnit = (uint)(_maxTextureUnits - 1);
        }

        public void SetTexture(uint textureUnit, WebGLTextureView textureView)
        {
            if (_textureUnitTextures[textureUnit] != textureView)
            {
                SetActiveTextureUnit(textureUnit);
                _gd.Ctx.BindTexture(textureView.WglTarget, textureView.WglTexture);
                _gd.CheckError();

                EnsureSamplerMipmapState(textureUnit, textureView.MipLevels > 1);
                _textureUnitTextures[textureUnit] = textureView;
            }
        }

        public void SetTextureTransient(uint target, WebGLDotNET.WebGLTexture texture)
        {
            _textureUnitTextures[_lastTextureUnit] = null;
            SetActiveTextureUnit(_lastTextureUnit);
            _gd.Ctx.BindTexture(target, texture);
            _gd.CheckError();
        }

        public void SetSampler(uint textureUnit, WebGLSampler sampler)
        {
            if (_textureUnitSamplers[textureUnit].Sampler != sampler)
            {
                bool mipmapped = false;
                WebGLTextureView texBinding = _textureUnitTextures[textureUnit];
                if (texBinding != null)
                {
                    mipmapped = texBinding.MipLevels > 1;
                }

                var wglSampler = mipmapped ? sampler.MipmapSampler : sampler.NoMipmapSampler;
                _gd.Ctx.BindSampler(textureUnit, wglSampler);
                _gd.CheckError();

                _textureUnitSamplers[textureUnit] = new BoundSamplerStateInfo(sampler, mipmapped);
            }
            else if (_textureUnitTextures[textureUnit] != null)
            {
                EnsureSamplerMipmapState(textureUnit, _textureUnitTextures[textureUnit].MipLevels > 1);
            }
        }

        private void SetActiveTextureUnit(uint textureUnit)
        {
            if (_currentActiveUnit != textureUnit)
            {
                _gd.Ctx.ActiveTexture(WebGLConstants.TEXTURE0 + textureUnit);
                _gd.CheckError();
                _currentActiveUnit = textureUnit;
            }
        }

        private void EnsureSamplerMipmapState(uint textureUnit, bool mipmapped)
        {
            if (_textureUnitSamplers[textureUnit].Sampler != null && _textureUnitSamplers[textureUnit].Mipmapped != mipmapped)
            {
                WebGLSampler sampler = _textureUnitSamplers[textureUnit].Sampler;
                WebGLDotNET.WebGLSampler wglSampler = mipmapped ? sampler.MipmapSampler : sampler.NoMipmapSampler;
                _gd.Ctx.BindSampler(textureUnit, wglSampler);
                _gd.CheckError();
                _textureUnitSamplers[textureUnit].Mipmapped = mipmapped;
            }
        }

        private struct BoundSamplerStateInfo
        {
            public WebGLSampler Sampler;
            public bool Mipmapped;

            public BoundSamplerStateInfo(WebGLSampler sampler, bool mipmapped)
            {
                Sampler = sampler;
                Mipmapped = mipmapped;
            }
        }
    }
}
