using System.Diagnostics;
using Veldrid.OpenGLBinding;
using static Veldrid.OpenGLBinding.OpenGLNative;
using static Veldrid.OpenGL.OpenGLUtil;

namespace Veldrid.OpenGL
{
    internal class OpenGLTextureView : TextureView, OpenGLDeferredResource
    {
        private readonly OpenGLGraphicsDevice _gd;
        private bool _needsTextureView;
        private uint _textureView;
        private bool _disposeRequested;
        private bool _disposed;
        public override bool IsDisposed => _disposeRequested;

        private string _name;
        private bool _nameChanged;
        public override string Name { get => _name; set { _name = value; _nameChanged = true; } }

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

        public OpenGLTextureView(OpenGLGraphicsDevice gd, ref TextureViewDescription description)
            : base(ref description)
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
                    throw new VeldridException(
                        "TextureView objects covering a subset of a Texture's dimensions or using a different PixelFormat are " +
                        "not supported on OpenGL ES.");
                }
                _needsTextureView = true;
            }
        }

        public SizedInternalFormat GetReadWriteSizedInternalFormat()
        {
            switch (Target.Format)
            {
                case PixelFormat.R8_UNorm:
                    return SizedInternalFormat.R8;
                case PixelFormat.R8_SNorm:
                    return (SizedInternalFormat)PixelInternalFormat.R8Snorm;
                case PixelFormat.R8_UInt:
                    return SizedInternalFormat.R8ui;
                case PixelFormat.R8_SInt:
                    return SizedInternalFormat.R8i;

                case PixelFormat.R16_UNorm:
                    return SizedInternalFormat.R16;
                case PixelFormat.R16_SNorm:
                    return (SizedInternalFormat)PixelInternalFormat.R16Snorm;
                case PixelFormat.R16_UInt:
                    return SizedInternalFormat.R16ui;
                case PixelFormat.R16_SInt:
                    return SizedInternalFormat.R16i;
                case PixelFormat.R16_Float:
                    return SizedInternalFormat.R16f;

                case PixelFormat.R32_UInt:
                    return SizedInternalFormat.R32ui;
                case PixelFormat.R32_SInt:
                    return SizedInternalFormat.R32i;
                case PixelFormat.R32_Float:
                    return SizedInternalFormat.R32f;

                case PixelFormat.R8_G8_UNorm:
                    return SizedInternalFormat.R8;
                case PixelFormat.R8_G8_SNorm:
                    return (SizedInternalFormat)PixelInternalFormat.Rg8Snorm;
                case PixelFormat.R8_G8_UInt:
                    return SizedInternalFormat.Rg8ui;
                case PixelFormat.R8_G8_SInt:
                    return SizedInternalFormat.Rg8i;

                case PixelFormat.R16_G16_UNorm:
                    return SizedInternalFormat.R16;
                case PixelFormat.R16_G16_SNorm:
                    return (SizedInternalFormat)PixelInternalFormat.Rg16Snorm;
                case PixelFormat.R16_G16_UInt:
                    return SizedInternalFormat.Rg16ui;
                case PixelFormat.R16_G16_SInt:
                    return SizedInternalFormat.Rg16i;
                case PixelFormat.R16_G16_Float:
                    return SizedInternalFormat.Rg16f;

                case PixelFormat.R32_G32_UInt:
                    return SizedInternalFormat.Rg32ui;
                case PixelFormat.R32_G32_SInt:
                    return SizedInternalFormat.Rg32i;
                case PixelFormat.R32_G32_Float:
                    return SizedInternalFormat.Rg32f;

                case PixelFormat.R8_G8_B8_A8_UNorm:
                case PixelFormat.B8_G8_R8_A8_UNorm:
                    return SizedInternalFormat.Rgba8;
                case PixelFormat.R8_G8_B8_A8_SNorm:
                    return (SizedInternalFormat)PixelInternalFormat.Rgba8Snorm;
                case PixelFormat.R8_G8_B8_A8_UInt:
                    return SizedInternalFormat.Rgba8ui;
                case PixelFormat.R8_G8_B8_A8_SInt:
                    return SizedInternalFormat.Rgba16i;

                case PixelFormat.R16_G16_B16_A16_UNorm:
                    return SizedInternalFormat.Rgba16;
                case PixelFormat.R16_G16_B16_A16_SNorm:
                    return (SizedInternalFormat)PixelInternalFormat.Rgba16Snorm;
                case PixelFormat.R16_G16_B16_A16_UInt:
                    return SizedInternalFormat.Rgba16ui;
                case PixelFormat.R16_G16_B16_A16_SInt:
                    return SizedInternalFormat.Rgba16i;
                case PixelFormat.R16_G16_B16_A16_Float:
                    return SizedInternalFormat.Rgba16f;

                case PixelFormat.R32_G32_B32_A32_UInt:
                    return SizedInternalFormat.Rgba32ui;
                case PixelFormat.R32_G32_B32_A32_SInt:
                    return SizedInternalFormat.Rgba32i;
                case PixelFormat.R32_G32_B32_A32_Float:
                    return SizedInternalFormat.Rgba32f;

                case PixelFormat.R10_G10_B10_A2_UNorm:
                    return (SizedInternalFormat)PixelInternalFormat.Rgb10A2;
                case PixelFormat.R10_G10_B10_A2_UInt:
                    return (SizedInternalFormat)PixelInternalFormat.Rgb10A2ui;
                case PixelFormat.R11_G11_B10_Float:
                    return (SizedInternalFormat)PixelInternalFormat.R11fG11fB10f;

                case PixelFormat.D24_UNorm_S8_UInt:
                case PixelFormat.D32_Float_S8_UInt:
                case PixelFormat.BC1_Rgb_UNorm:
                case PixelFormat.BC1_Rgba_UNorm:
                case PixelFormat.BC2_UNorm:
                case PixelFormat.BC3_UNorm:
                case PixelFormat.BC4_UNorm:
                case PixelFormat.BC4_SNorm:
                case PixelFormat.BC5_UNorm:
                case PixelFormat.BC5_SNorm:
                case PixelFormat.BC7_UNorm:
                default:
                    throw Illegal.Value<PixelFormat>();
            }
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

        private void CreateGLResources()
        {
            if (!_needsTextureView)
            {
                TextureTarget = Target.TextureTarget;
                return;
            }

            glGenTextures(1, out _textureView);
            CheckLastError();

            TextureTarget originalTarget = Target.TextureTarget;
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
                ArrayLayers);
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

        public void DestroyGLResources()
        {
            if (!_disposed)
            {
                _disposed = true;
                if (_textureView != 0)
                {
                    glDeleteTextures(1, ref _textureView);
                    CheckLastError();
                }
            }
        }
    }
}
