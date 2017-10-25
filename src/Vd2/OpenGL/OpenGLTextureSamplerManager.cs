using static Vd2.OpenGLBinding.OpenGLNative;
using static Vd2.OpenGL.OpenGLUtil;
using Vd2.OpenGLBinding;
using System;

namespace Vd2.OpenGL
{
    /// <summary>
    /// A utility class managing the relationships between textures, samplers, and their binding locations.
    /// </summary>
    internal unsafe class OpenGLTextureSamplerManager
    {
        private readonly bool _dsaAvailable;
        private readonly int _maxTextureUnits;
        private readonly OpenGLTextureView[] _textureUnitTextures;
        private readonly BoundSamplerStateInfo[] _textureUnitSamplers;

        public OpenGLTextureSamplerManager(OpenGLExtensions extensions)
        {
            _dsaAvailable = extensions.ARB_DirectStateAccess;
            int maxTextureUnits;
            glGetIntegerv(GetPName.MaxCombinedTextureImageUnits, &maxTextureUnits);
            CheckLastError();
            _maxTextureUnits = Math.Max(maxTextureUnits, 8); // OpenGL spec indicates that implementations must support at least 8.
            _textureUnitTextures = new OpenGLTextureView[_maxTextureUnits];
            _textureUnitSamplers = new BoundSamplerStateInfo[_maxTextureUnits];
        }

        public void SetTexture(uint textureUnit, OpenGLTextureView textureView)
        {
            if (_textureUnitTextures[textureUnit] != textureView)
            {
                GetTextureAndTarget(textureView.Target, out uint textureID, out TextureTarget textureTarget);

                if (_dsaAvailable)
                {
                    glBindTextureUnit(textureUnit, textureID);
                    CheckLastError();
                }
                else
                {
                    glActiveTexture(TextureUnit.Texture0 + (int)textureUnit);
                    CheckLastError();
                    glBindTexture(textureTarget, textureID);
                    CheckLastError();
                }

                EnsureSamplerMipmapState(textureUnit, textureView.Target.MipLevels > 1);
                _textureUnitTextures[textureUnit] = textureView;
            }
        }

        public void SetSampler(uint textureUnit, OpenGLSampler sampler)
        {
            if (_textureUnitSamplers[textureUnit].Sampler != sampler)
            {
                bool mipmapped = false;
                OpenGLTextureView texBinding = _textureUnitTextures[textureUnit];
                if (texBinding != null)
                {
                    mipmapped = texBinding.Target.MipLevels > 1;
                }

                uint samplerID = mipmapped ? sampler.MipmapSampler : sampler.NoMipmapSampler;
                glBindSampler(textureUnit, samplerID);
                CheckLastError();

                _textureUnitSamplers[textureUnit] = new BoundSamplerStateInfo(sampler, mipmapped);
            }
            else if (_textureUnitTextures[textureUnit] != null)
            {
                EnsureSamplerMipmapState(textureUnit, _textureUnitTextures[textureUnit].Target.MipLevels > 1);
            }
        }

        private void EnsureSamplerMipmapState(uint textureUnit, bool mipmapped)
        {
            if (_textureUnitSamplers[textureUnit].Sampler != null && _textureUnitSamplers[textureUnit].Mipmapped != mipmapped)
            {
                OpenGLSampler sampler = _textureUnitSamplers[textureUnit].Sampler;
                uint samplerID = mipmapped ? sampler.MipmapSampler : sampler.NoMipmapSampler;
                glBindSampler(textureUnit, samplerID);
                CheckLastError();

                _textureUnitSamplers[textureUnit].Mipmapped = mipmapped;
            }
        }

        private void GetTextureAndTarget(Texture tex, out uint textureID, out TextureTarget target)
        {
            if (tex is OpenGLTexture2D glTex2D)
            {
                textureID = glTex2D.Texture;
                target = TextureTarget.Texture2D;
            }
            else if (tex is OpenGLTextureCube glTexCube)
            {
                textureID = glTexCube.Texture;
                target = TextureTarget.TextureCubeMap;
            }
            else
            {
                throw new VdException("Invalid texture type used in OpenGL backend: " + tex.GetType().Name);
            }
        }

        private struct BoundSamplerStateInfo
        {
            public OpenGLSampler Sampler;
            public bool Mipmapped;

            public BoundSamplerStateInfo(OpenGLSampler sampler, bool mipmapped)
            {
                Sampler = sampler;
                Mipmapped = mipmapped;
            }
        }
    }
}
