using static Veldrid.OpenGLBinding.OpenGLNative;
using static Veldrid.OpenGL.OpenGLUtil;
using Veldrid.OpenGLBinding;

namespace Veldrid.OpenGL
{
    internal unsafe class OpenGLSampler : Sampler, OpenGLDeferredResource
    {
        private readonly OpenGLGraphicsDevice _gd;
        private readonly SamplerDescription _description;
        private readonly InternalSamplerState _noMipmapState;
        private readonly InternalSamplerState _mipmapState;
        private bool _disposeRequested;

        private string _name;
        private bool _nameChanged;
        public override string Name { get => _name; set { _name = value; _nameChanged = true; } }

        public override bool IsDisposed => _disposeRequested;

        public uint NoMipmapSampler => _noMipmapState.Sampler;
        public uint MipmapSampler => _mipmapState.Sampler;

        public OpenGLSampler(OpenGLGraphicsDevice gd, ref SamplerDescription description)
        {
            _gd = gd;
            _description = description;

            _mipmapState = new InternalSamplerState();
            _noMipmapState = new InternalSamplerState();
        }

        public bool Created { get; private set; }

        public void EnsureResourcesCreated()
        {
            if (!Created)
            {
                CreateGLResources();
            }
            if (_nameChanged)
            {
                _nameChanged = false;
                if (_gd.Extensions.KHR_Debug)
                {
                    SetObjectLabel(ObjectLabelIdentifier.Sampler, _noMipmapState.Sampler, string.Format("{0}_WithoutMipmapping", _name));
                    SetObjectLabel(ObjectLabelIdentifier.Sampler, _mipmapState.Sampler, string.Format("{0}_WithMipmapping", _name));
                }
            }
        }

        private void CreateGLResources()
        {
            GraphicsBackend backendType = _gd.BackendType;
            _noMipmapState.CreateGLResources(_description, false, backendType);
            _mipmapState.CreateGLResources(_description, true, backendType);
            Created = true;
        }

        public override void Dispose()
        {
            if (!_disposeRequested)
            {
                _disposeRequested = true;
                _gd.EnqueueDisposal(this);
            }
        }

        public void DestroyGLResources()
        {
            _mipmapState.DestroyGLResources();
            _noMipmapState.DestroyGLResources();
        }

        private class InternalSamplerState
        {
            private uint _sampler;

            public uint Sampler => _sampler;

            public void CreateGLResources(SamplerDescription description, bool mipmapped, GraphicsBackend backend)
            {
                glGenSamplers(1, out _sampler);
                CheckLastError();

                glSamplerParameteri(_sampler, SamplerParameterName.TextureWrapS, (int)OpenGLFormats.VdToGLTextureWrapMode(description.AddressModeU));
                CheckLastError();
                glSamplerParameteri(_sampler, SamplerParameterName.TextureWrapT, (int)OpenGLFormats.VdToGLTextureWrapMode(description.AddressModeV));
                CheckLastError();
                glSamplerParameteri(_sampler, SamplerParameterName.TextureWrapR, (int)OpenGLFormats.VdToGLTextureWrapMode(description.AddressModeW));
                CheckLastError();

                if (description.AddressModeU == SamplerAddressMode.Border
                    || description.AddressModeV == SamplerAddressMode.Border
                    || description.AddressModeW == SamplerAddressMode.Border)
                {
                    RgbaFloat borderColor = ToRgbaFloat(description.BorderColor);
                    glSamplerParameterfv(_sampler, SamplerParameterName.TextureBorderColor, (float*)&borderColor);
                    CheckLastError();
                }

                glSamplerParameterf(_sampler, SamplerParameterName.TextureMinLod, description.MinimumLod);
                CheckLastError();
                glSamplerParameterf(_sampler, SamplerParameterName.TextureMaxLod, description.MaximumLod);
                CheckLastError();
                if (backend == GraphicsBackend.OpenGL && description.LodBias != 0)
                {
                    glSamplerParameterf(_sampler, SamplerParameterName.TextureLodBias, description.LodBias);
                    CheckLastError();
                }

                if (description.Filter == SamplerFilter.Anisotropic)
                {
                    glSamplerParameterf(_sampler, SamplerParameterName.TextureMaxAnisotropyExt, description.MaximumAnisotropy);
                    CheckLastError();
                    glSamplerParameteri(_sampler, SamplerParameterName.TextureMinFilter, mipmapped ? (int)TextureMinFilter.LinearMipmapLinear : (int)TextureMinFilter.Linear);
                    CheckLastError();
                    glSamplerParameteri(_sampler, SamplerParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                    CheckLastError();
                }
                else
                {
                    OpenGLFormats.VdToGLTextureMinMagFilter(description.Filter, mipmapped, out TextureMinFilter min, out TextureMagFilter mag);
                    glSamplerParameteri(_sampler, SamplerParameterName.TextureMinFilter, (int)min);
                    CheckLastError();
                    glSamplerParameteri(_sampler, SamplerParameterName.TextureMagFilter, (int)mag);
                    CheckLastError();
                }

                if (description.ComparisonKind != null)
                {
                    glSamplerParameteri(_sampler, SamplerParameterName.TextureCompareMode, (int)TextureCompareMode.CompareRefToTexture);
                    CheckLastError();
                    glSamplerParameteri(_sampler, SamplerParameterName.TextureCompareFunc, (int)OpenGLFormats.VdToGLDepthFunction(description.ComparisonKind.Value));
                    CheckLastError();
                }
            }

            public void DestroyGLResources()
            {
                glDeleteSamplers(1, ref _sampler);
                CheckLastError();
            }

            private RgbaFloat ToRgbaFloat(SamplerBorderColor borderColor)
            {
                switch (borderColor)
                {
                    case SamplerBorderColor.TransparentBlack:
                        return new RgbaFloat(0, 0, 0, 0);
                    case SamplerBorderColor.OpaqueBlack:
                        return new RgbaFloat(0, 0, 0, 1);
                    case SamplerBorderColor.OpaqueWhite:
                        return new RgbaFloat(1, 1, 1, 1);
                    default:
                        throw Illegal.Value<SamplerBorderColor>();
                }
            }
        }
    }
}
