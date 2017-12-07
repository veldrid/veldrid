using System.Diagnostics;
using Veldrid.OpenGLBinding;
using static Veldrid.OpenGLBinding.OpenGLNative;
using static Veldrid.OpenGL.OpenGLUtil;
using System;

namespace Veldrid.OpenGL
{
    internal class OpenGLTextureView : TextureView, OpenGLDeferredResource
    {
        private readonly OpenGLGraphicsDevice _gd;
        private bool _needsTextureView;
        private uint _textureView;
        private bool _disposed;

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
                || BaseArrayLayer != 0 || ArrayLayers != Target.ArrayLayers)
            {
                if (!_gd.Extensions.ARB_TextureView)
                {
                    throw new VeldridException(
                        "TextureView objects covering a subset of a Texture's dimensions require OpenGL 4.3, or ARB_texture_view.");
                }
                _needsTextureView = true;
            }
        }

        public SizedInternalFormat GetReadWriteSizedInternalFormat()
        {
            switch (Target.Format)
            {
                case PixelFormat.R8_G8_B8_A8_UNorm:
                case PixelFormat.B8_G8_R8_A8_UNorm:
                    return SizedInternalFormat.Rgba8ui;
                case PixelFormat.R8_UNorm:
                    return SizedInternalFormat.R8ui;
                case PixelFormat.R16_UNorm:
                    return SizedInternalFormat.R16ui;
                case PixelFormat.R32_G32_B32_A32_Float:
                    return SizedInternalFormat.Rgba32f;
                case PixelFormat.R32_Float:
                    return SizedInternalFormat.R32f;
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
                SetObjectLabel(ObjectLabelIdentifier.Texture, _textureView, _name);
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
            if (originalTarget == TextureTarget.Texture2D)
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

            PixelInternalFormat internalFormat = GetCompatibleInternalFormat(
                Target.Format,
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

        private PixelInternalFormat GetCompatibleInternalFormat(PixelFormat vdFormat, bool depthFormat)
        {
            switch (vdFormat)
            {
                case PixelFormat.R8_G8_B8_A8_UNorm:
                case PixelFormat.B8_G8_R8_A8_UNorm:
                    return PixelInternalFormat.Rgba8ui;
                case PixelFormat.R8_UNorm:
                    return PixelInternalFormat.R8ui;
                case PixelFormat.R16_UNorm:
                    return depthFormat ? PixelInternalFormat.DepthComponent16 : PixelInternalFormat.R16ui;
                case PixelFormat.R32_G32_B32_A32_Float:
                    return PixelInternalFormat.Rgba32f;
                case PixelFormat.R32_Float:
                    return depthFormat ? PixelInternalFormat.DepthComponent32f : PixelInternalFormat.R32f;
                default:
                    throw Illegal.Value<PixelInternalFormat>();
            }
        }

        public override void Dispose()
        {
            _gd.EnqueueDisposal(this);
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