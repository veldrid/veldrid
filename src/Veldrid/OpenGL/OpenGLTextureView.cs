using System.Diagnostics;
using Veldrid.OpenGLBinding;
using static Veldrid.OpenGL.OpenGLUtil;
using static Veldrid.OpenGLBinding.OpenGLNative;

namespace Veldrid.OpenGL
{
    internal sealed class OpenGLTextureView : TextureView, OpenGLDeferredResource
    {
        private readonly OpenGLGraphicsDevice _gd;
        private bool _needsTextureView;
        private uint _textureView;
        private bool _disposeRequested;
        private bool _disposed;
        public override bool IsDisposed => _disposeRequested;

        private string? _name;
        private bool _nameChanged;

        public override string? Name { get => _name; set { _name = value; _nameChanged = true; } }

        public new OpenGLTexture Target { get; }
        public TextureTarget TextureTarget { get; private set; }

        public uint GLTargetTexture
        {
            get
            {
                Debug.Assert(Created);
                if (_textureView == 0)
                {
                    Debug.Assert(Target.Created);
                    return Target.Texture;
                }
                else
                {
                    return _textureView;
                }
            }
        }

        public bool Created { get; private set; }

        public OpenGLTextureView(OpenGLGraphicsDevice gd, in TextureViewDescription description)
            : base(description)
        {
            _gd = gd;
            Target = Util.AssertSubtype<Texture, OpenGLTexture>(description.Target);

            if (BaseMipLevel != 0 || MipLevels != Target.MipLevels
                || BaseArrayLayer != 0 || ArrayLayers != Target.ArrayLayers
                || Format != Target.Format)
            {
                if (_gd.BackendType == GraphicsBackend.OpenGL)
                {
                    if (!_gd.Extensions.ARB_TextureView)
                    {
                        throw new VeldridException(
                            "TextureView objects covering a subset of a Texture's dimensions or using a different PixelFormat " +
                            "require OpenGL 4.3, or ARB_texture_view.");
                    }
                }
                else
                {
                    if (!_gd.Extensions.ARB_TextureView)
                    {
                        throw new VeldridException(
                            "TextureView objects covering a subset of a Texture's dimensions or using a different PixelFormat are " +
                            "not supported on OpenGL ES.");
                    }
                }
                _needsTextureView = true;
            }
        }

        public SizedInternalFormat GetReadWriteSizedInternalFormat()
        {
            return Target.Format switch
            {
                PixelFormat.R8_UNorm => SizedInternalFormat.R8,
                PixelFormat.R8_SNorm => (SizedInternalFormat)PixelInternalFormat.R8Snorm,
                PixelFormat.R8_UInt => SizedInternalFormat.R8ui,
                PixelFormat.R8_SInt => SizedInternalFormat.R8i,
                PixelFormat.R16_UNorm => SizedInternalFormat.R16,
                PixelFormat.R16_SNorm => (SizedInternalFormat)PixelInternalFormat.R16Snorm,
                PixelFormat.R16_UInt => SizedInternalFormat.R16ui,
                PixelFormat.R16_SInt => SizedInternalFormat.R16i,
                PixelFormat.R16_Float => SizedInternalFormat.R16f,
                PixelFormat.R32_UInt => SizedInternalFormat.R32ui,
                PixelFormat.R32_SInt => SizedInternalFormat.R32i,
                PixelFormat.R32_Float => SizedInternalFormat.R32f,
                PixelFormat.R8_G8_UNorm => SizedInternalFormat.R8,
                PixelFormat.R8_G8_SNorm => (SizedInternalFormat)PixelInternalFormat.Rg8Snorm,
                PixelFormat.R8_G8_UInt => SizedInternalFormat.Rg8ui,
                PixelFormat.R8_G8_SInt => SizedInternalFormat.Rg8i,
                PixelFormat.R16_G16_UNorm => SizedInternalFormat.R16,
                PixelFormat.R16_G16_SNorm => (SizedInternalFormat)PixelInternalFormat.Rg16Snorm,
                PixelFormat.R16_G16_UInt => SizedInternalFormat.Rg16ui,
                PixelFormat.R16_G16_SInt => SizedInternalFormat.Rg16i,
                PixelFormat.R16_G16_Float => SizedInternalFormat.Rg16f,
                PixelFormat.R32_G32_UInt => SizedInternalFormat.Rg32ui,
                PixelFormat.R32_G32_SInt => SizedInternalFormat.Rg32i,
                PixelFormat.R32_G32_Float => SizedInternalFormat.Rg32f,
                PixelFormat.R8_G8_B8_A8_UNorm or PixelFormat.B8_G8_R8_A8_UNorm => SizedInternalFormat.Rgba8,
                PixelFormat.R8_G8_B8_A8_SNorm => (SizedInternalFormat)PixelInternalFormat.Rgba8Snorm,
                PixelFormat.R8_G8_B8_A8_UInt => SizedInternalFormat.Rgba8ui,
                PixelFormat.R8_G8_B8_A8_SInt => SizedInternalFormat.Rgba16i,
                PixelFormat.R16_G16_B16_A16_UNorm => SizedInternalFormat.Rgba16,
                PixelFormat.R16_G16_B16_A16_SNorm => (SizedInternalFormat)PixelInternalFormat.Rgba16Snorm,
                PixelFormat.R16_G16_B16_A16_UInt => SizedInternalFormat.Rgba16ui,
                PixelFormat.R16_G16_B16_A16_SInt => SizedInternalFormat.Rgba16i,
                PixelFormat.R16_G16_B16_A16_Float => SizedInternalFormat.Rgba16f,
                PixelFormat.R32_G32_B32_A32_UInt => SizedInternalFormat.Rgba32ui,
                PixelFormat.R32_G32_B32_A32_SInt => SizedInternalFormat.Rgba32i,
                PixelFormat.R32_G32_B32_A32_Float => SizedInternalFormat.Rgba32f,
                PixelFormat.R10_G10_B10_A2_UNorm => (SizedInternalFormat)PixelInternalFormat.Rgb10A2,
                PixelFormat.R10_G10_B10_A2_UInt => (SizedInternalFormat)PixelInternalFormat.Rgb10A2ui,
                PixelFormat.R11_G11_B10_Float => (SizedInternalFormat)PixelInternalFormat.R11fG11fB10f,
                _ => throw Illegal.Value<PixelFormat>(),
            };
        }

        public void EnsureResourcesCreated()
        {
            Target.EnsureResourcesCreated();
            if (!Created)
            {
                CreateGLResources();
                Created = true;
            }
            if (_nameChanged && _needsTextureView)
            {
                if (_gd.Extensions.KHR_Debug)
                {
                    SetObjectLabel(ObjectLabelIdentifier.Texture, _textureView, _name);
                }
            }
        }

        private unsafe void CreateGLResources()
        {
            if (!_needsTextureView)
            {
                TextureTarget = Target.TextureTarget;
                return;
            }

            uint texView;
            glGenTextures(1, &texView);
            CheckLastError();
            _textureView = texView;

            TextureTarget originalTarget = Target.TextureTarget;
            uint effectiveArrayLayers = ArrayLayers;
            if (originalTarget == TextureTarget.Texture1D)
            {
                TextureTarget = TextureTarget.Texture1D;
            }
            else if (originalTarget == TextureTarget.Texture1DArray)
            {
                if (ArrayLayers > 1)
                {
                    TextureTarget = TextureTarget.Texture1DArray;
                }
                else
                {
                    TextureTarget = TextureTarget.Texture1D;
                }
            }
            else if (originalTarget == TextureTarget.Texture2D)
            {
                TextureTarget = TextureTarget.Texture2D;
            }
            else if (originalTarget == TextureTarget.Texture2DArray)
            {
                if (ArrayLayers > 1)
                {
                    TextureTarget = TextureTarget.Texture2DArray;
                }
                else
                {
                    TextureTarget = TextureTarget.Texture2D;
                }
            }
            else if (originalTarget == TextureTarget.Texture2DMultisample)
            {
                TextureTarget = TextureTarget.Texture2DMultisample;
            }
            else if (originalTarget == TextureTarget.Texture2DMultisampleArray)
            {
                if (ArrayLayers > 1)
                {
                    TextureTarget = TextureTarget.Texture2DMultisampleArray;
                }
                else
                {
                    TextureTarget = TextureTarget.Texture2DMultisample;
                }
            }
            else if (originalTarget == TextureTarget.Texture3D)
            {
                TextureTarget = TextureTarget.Texture3D;
            }
            else if (originalTarget == TextureTarget.TextureCubeMap)
            {
                if (ArrayLayers > 1)
                {
                    TextureTarget = TextureTarget.TextureCubeMap;
                    effectiveArrayLayers *= 6;
                }
                else
                {
                    TextureTarget = TextureTarget.TextureCubeMapArray;
                    effectiveArrayLayers *= 6;
                }
            }
            else
            {
                throw new VeldridException("The given TextureView parameters are not supported with the OpenGL backend.");
            }

            PixelInternalFormat internalFormat = (PixelInternalFormat)OpenGLFormats.VdToGLSizedInternalFormat(
                Format,
                (Target.Usage & TextureUsage.DepthStencil) == TextureUsage.DepthStencil);
            Debug.Assert(Target.Created);
            glTextureView(
                _textureView,
                TextureTarget,
                Target.Texture,
                internalFormat,
                BaseMipLevel,
                MipLevels,
                BaseArrayLayer,
                effectiveArrayLayers);
            CheckLastError();
        }

        public override void Dispose()
        {
            if (!_disposeRequested)
            {
                _disposeRequested = true;
                _gd.EnqueueDisposal(this);
            }
        }

        public unsafe void DestroyGLResources()
        {
            if (!_disposed)
            {
                _disposed = true;
                if (_textureView != 0)
                {
                    uint texView = _textureView;
                    glDeleteTextures(1, &texView);
                    CheckLastError();
                    _textureView = texView;
                }
            }
        }
    }
}
